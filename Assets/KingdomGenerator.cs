using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KingdomGenerator : MonoBehaviour
{
    public float paso = 2f; // Espaciado entre puntos de muestreo
    public float radio = 5f; // Radio de análisis en cada punto
    public int númeroDeReinos = 5; // Número de mejores puntos a guardar
    public int cantidadAlimentoMinimo = 20; // Número de alimento mínimo para que no decrezca el reino
    public int cantidadAlimentoParaCrecer=40; // Número de alimento mínimo para que crezca el reino
    [Range(1,5)]
    public float decaimientoVecinos=1;
    public Mesh boxMesh;
    public Mesh circleMesh;

    ElementManagement manager;
    Terrain terrain;
    FlatResourceGenerator resourceGenerator;
    private bool calculate;

    Dictionary<string, float> valoresPorTag = new Dictionary<string, float>
    {
        { "ColdField", 0f },
        { "EmptyField", 2f },
        { "ForestField", 1.5f },
        { "SeaField", 3f },
        { "FertileField", 5f },
        { "SmallMine", 5f },
        { "GreatMine", 10f }
    };

    // Start is called before the first frame update
    void Start()
    {
        terrain = GetComponent<Terrain>();
        manager = GetComponent<ElementManagement>();
        resourceGenerator = GetComponent<FlatResourceGenerator>();
    }

    void LateUpdate()
    {
        if (calculate)
        {
            calculate = false;
            for (int n=0;n< númeroDeReinos;n++)
            {
                Vector3 pos = GetTopValuePoint(manager.resources);
                RaycastHit hit;
                Physics.Raycast(new Vector3(pos.x, 100, pos.z), Vector3.down, out hit, Mathf.Infinity);
                if (hit.collider != null)
                {
                    if(hit.point.y>= 0)
                    {
                        GameObject kingdom = new GameObject("Kingdom");
                        kingdom.tag = "Kingdom";
                        MeshFilter mf = kingdom.AddComponent<MeshFilter>();
                        mf.mesh = boxMesh;
                        MeshRenderer mr = kingdom.AddComponent<MeshRenderer>();
                        mr.material = new Material(Shader.Find("Sprites/Default")) { color = Color.red };
                        kingdom.transform.localScale = new Vector3(5f, 7.5f, 5f);
                        kingdom.transform.position = new Vector3(pos.x, hit.point.y, pos.z);

                        GameObject a = new GameObject("Radius");
                        MeshFilter mf2 = a.AddComponent<MeshFilter>();
                        mf2.mesh = circleMesh;
                        MeshRenderer mr2 = a.AddComponent<MeshRenderer>();
                        mr2.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(1,1,1,0.05f) };
                        a.transform.localScale = new Vector3(radio * 2, 20, radio * 2);
                        a.transform.position = new Vector3(pos.x, hit.point.y, pos.z);
                        a.transform.parent=kingdom.transform;

                        KingdomController controller= kingdom.AddComponent<KingdomController>();
                        GetValueInRadius(hit.point, radio, manager.resources, controller);
                        controller.SetType();
                        manager.kingdoms.Add(controller);
                    }
                }
            }
            KingdomRecalculation();
            manager.KingdomRelations();
        }
    }

    public Vector3 GetTopValuePoint(List<ResourceInfo> objectsToCheck)
    {
        List<(Vector3 position, float value)> allPoints = new List<(Vector3, float)>();

        // Obtener los límites del terreno
        Vector3 terrainPosition = terrain.transform.position;
        float terrainWidth = terrain.terrainData.size.x;
        float terrainLength = terrain.terrainData.size.z;

        // Generar una cuadrícula sobre el terreno
        for (float x = terrainPosition.x; x < terrainPosition.x + terrainWidth; x += paso)
        {
            for (float z = terrainPosition.z; z < terrainPosition.z + terrainLength; z += paso)
            {
                // Obtener la altura del terreno en este punto
                float y = terrainPosition.y;

                Vector3 point = new Vector3(x, y, z);
                float value = GetValueInRadius(point, radio, objectsToCheck);
                RaycastHit hit;
                Physics.Raycast(new Vector3(point.x, 100, point.z), Vector3.down, out hit, Mathf.Infinity);
                if (hit.point.y>= resourceGenerator.minHeight)
                {
                allPoints.Add((point, value));
                }
            }
        }

        // Ordenar por valor descendente y tomar los X mejores puntos
        List<Vector3> topPoints = allPoints.OrderByDescending(p => p.value).Select(p => p.position).ToList();

        return topPoints[0];
    }

    public float GetValueInRadius(Vector3 center, float radius, List<ResourceInfo> objectsToCheck,KingdomController kingdom=null)
    {
        float totalValue = 0f;

        foreach (ResourceInfo obj in objectsToCheck)
        {
            if (obj == null) continue; // Evita errores con referencias nulas

            float distance = Vector3.Distance(center, obj.transform.position);
            if (distance <= radius)
            {
                if (valoresPorTag.TryGetValue(obj.gameObject.tag, out float value))
                {
                    float v = value;
                    foreach (KingdomController k in obj.interestedKingdoms)
                    {
                        if (k == kingdom) continue;
                        v *= (distance / (radius * decaimientoVecinos));
                    }
                    totalValue+=v;
                }
                if (kingdom != null)
                {
                    obj.interestedKingdoms.Add(kingdom);
                    kingdom.resources.Add(obj.gameObject);
                }
            }
        }

        return totalValue;
    }

    public void KingdomRecalculation()
    {
        foreach (KingdomController k in manager.kingdoms)
        {
            if (k.foodSources > cantidadAlimentoParaCrecer)
            {
                k.resources.Clear();
                manager.resetResourceInterests(k);
                k.transform.localScale += new Vector3(3, 2, 3);
                Destroy(k.transform.GetChild(0).gameObject);

                GameObject a = new GameObject("Radius");
                MeshFilter mf2 = a.AddComponent<MeshFilter>();
                mf2.mesh = circleMesh;
                MeshRenderer mr2 = a.AddComponent<MeshRenderer>();
                mr2.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(1, 1, 1, 0.05f) };
                float r = Mathf.Clamp(manager.NearestKingdomDistance(k.transform.position)-1,radio,radio*1.5f);
                a.transform.localScale = new Vector3(r*2, 15, r*2);
                a.transform.position = k.transform.position;
                a.transform.parent = k.transform;
                GetValueInRadius(k.transform.position, r, manager.resources, k);
                k.SetType();
            }
            else if(k.foodSources <= cantidadAlimentoMinimo)
            {
                k.resources.Clear();
                manager.resetResourceInterests(k);
                k.transform.localScale += new Vector3(-2, -3, -2);
                Destroy(k.transform.GetChild(0).gameObject);

                GameObject a = new GameObject("Radius");
                MeshFilter mf2 = a.AddComponent<MeshFilter>();
                mf2.mesh = circleMesh;
                MeshRenderer mr2 = a.AddComponent<MeshRenderer>();
                mr2.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(1, 1, 1, 0.05f) };
                a.transform.localScale = new Vector3((radio * 2) * 0.75f, 15, (radio * 2) * 0.75f);
                a.transform.position = k.transform.position;
                a.transform.parent = k.transform;
                GetValueInRadius(k.transform.position, radio * 0.75f, manager.resources, k);
                k.SetType();
            }
        }
    }

    public void Recalculate()
    {
        calculate = true;
    }
}
