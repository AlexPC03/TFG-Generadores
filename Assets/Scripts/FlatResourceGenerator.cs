using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatResourceGenerator : LocatorFunctions
{
    public float minHeight = 1f;
    public float coldHeight=15;
    public float forestMinHeight=7.5f;
    public float maxSeaHeight=2.5f;
    public float maxDistanceRiver= 15f;
    public float maxDistanceDelta= 30f;
    private float forestDistanceMultiplicator= 2.25f;
    private float regionSize = 2.5f; // Tamaño de la región a evaluar (en unidades del terreno)
    private int gridResolution = 5; // Número de puntos en la cuadrícula (más alto, más puntos)
    private float maxSlope = 0.25f; // Máxima inclinación permitida para que la región sea considerada plana
    private float fieldDistance = 15f; 
    List<Vector2> flatRegions;
    public Mesh circleMesh;
    private bool calculate;
    private ElementManagement manager;


    void Start()
    {
        calculate = true;
        flatRegions = new List<Vector2>();
        manager = GetComponent<ElementManagement>();
    }

    private void LateUpdate()
    {
        if (calculate)
        {
            calculate = false;
            flatRegions.Clear();
            FindFlatRegions(GetComponent<Terrain>());
            foreach (Vector2 flat in flatRegions)
            {
                RaycastHit hit;
                Physics.Raycast(new Vector3(flat.x, 100, flat.y), Vector3.down, out hit, Mathf.Infinity);
                if (hit.collider == null) continue;
                if (hit.point.y > 1 && NearestOtherResource(hit.point, manager.resources) >= fieldDistance*Random.Range(0.9f,1.5f))
                {
                    GameObject field = null;
                    if (hit.point.y < coldHeight && (NearestRiver(hit.point, true)<= maxDistanceRiver || NearestDelta(hit.point, true) <= maxDistanceDelta))
                    {
                        field= new GameObject("FertileField");
                        field.tag = "FertileField";
                        MeshFilter mf=field.AddComponent<MeshFilter>();
                        mf.mesh = circleMesh;
                        MeshRenderer mr= field.AddComponent<MeshRenderer>();
                        mr.material= new Material(Shader.Find("Sprites/Default")) { color = new Color(0.4f,0.2f,0)};
                        field.transform.localScale = new Vector3(regionSize * 2f, regionSize, regionSize * 2f);
                        field.transform.position=hit.point;
                    }
                    else if(hit.point.y < maxSeaHeight)
                    {
                        field = new GameObject("SeaField");
                        field.tag = "SeaField";
                        MeshFilter mf = field.AddComponent<MeshFilter>();
                        mf.mesh = circleMesh;
                        MeshRenderer mr = field.AddComponent<MeshRenderer>();
                        mr.material = new Material(Shader.Find("Sprites/Default")) { color = Color.blue};
                        field.transform.localScale = new Vector3(regionSize * 2f, regionSize, regionSize * 2f);
                        field.transform.position = hit.point;
                    }
                    else
                    {
                        if (hit.point.y > coldHeight)
                        {
                            field = new GameObject("ColdField");
                            field.tag = "ColdField";
                            MeshFilter mf = field.AddComponent<MeshFilter>();
                            mf.mesh = circleMesh;
                            MeshRenderer mr = field.AddComponent<MeshRenderer>();
                            mr.material = new Material(Shader.Find("Sprites/Default")) { color = Color.white };
                            field.transform.localScale = new Vector3(regionSize * 2f, regionSize, regionSize * 2f);
                            field.transform.position = hit.point;
                        }
                        else
                        {
                            if (hit.point.y > forestMinHeight && (NearestRiver(hit.point, true) <= maxDistanceRiver* forestDistanceMultiplicator || NearestDelta(hit.point, true) <= maxDistanceDelta* forestDistanceMultiplicator))
                            {
                                field = new GameObject("ForestField");
                                field.tag = "ForestField";
                                MeshFilter mf = field.AddComponent<MeshFilter>();
                                mf.mesh = circleMesh;
                                MeshRenderer mr = field.AddComponent<MeshRenderer>();
                                mr.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(0.15f, 0.4f, 0) };
                                field.transform.localScale = new Vector3(regionSize * 2f, regionSize, regionSize * 2f);
                                field.transform.position = hit.point;
                            }
                            else
                            {
                                field = new GameObject("EmptyField");
                                field.tag = "EmptyField";
                                MeshFilter mf = field.AddComponent<MeshFilter>();
                                mf.mesh = circleMesh;
                                MeshRenderer mr = field.AddComponent<MeshRenderer>();
                                mr.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(0.3f, 0.8f, 0) };
                                field.transform.localScale = new Vector3(regionSize * 2f, regionSize, regionSize * 2f);
                                field.transform.position = hit.point;
                            }
                        }
                    }
                    if (field != null)
                    {
                        field.AddComponent<ResourceInfo>();
                        manager.resources.Add(field.GetComponent<ResourceInfo>());
                    }
                }
            }
        }
    }

    void FindFlatRegions(Terrain terrain)
    {
        // Obtiene las dimensiones del terreno
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = terrain.transform.position;

        // Recorre el terreno buscando áreas planas
        for (float x = terrainPosition.x; x < terrainPosition.x + terrainData.size.x; x += regionSize)
        {
            for (float z = terrainPosition.z; z < terrainPosition.z + terrainData.size.z; z += regionSize)
            {
                // Comienza el chequeo de la región en (x, z)
                if (IsFlatRegion(new Vector3(x, 0, z)))
                {
                    flatRegions.Add(new Vector2(x,z));
                }
            }
        }
    }

    bool IsFlatRegion(Vector3 center)
    {
        // Calcula la posición central para la búsqueda
        Vector3 regionCenter = new Vector3(center.x, 0, center.z);

        // Lista para almacenar las normales de los raycasts
        int hitCount = 0;

        // Recorre una cuadrícula de puntos dentro de la región
        for (int i = 0; i < gridResolution; i++)
        {
            for (int j = 0; j < gridResolution; j++)
            {
                // Calcular las posiciones en la cuadrícula
                float offsetX = regionCenter.x + (i - gridResolution / 2) * (regionSize / gridResolution);
                float offsetZ = regionCenter.z + (j - gridResolution / 2) * (regionSize / gridResolution);
                Vector3 rayOrigin = new Vector3(offsetX, 100f, offsetZ); // Lanza raycasts desde arriba

                // Lanzar el raycast hacia abajo
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity))
                {
                    // Si la normal de la superficie es lo suficientemente plana, cuenta este raycast
                    if (IsNormalFlat(hit.normal))
                    {
                        hitCount++;
                    }
                }
            }
        }

        // Si la mayoría de los raycasts fueron planos, consideramos esta región plana
        return (hitCount / (float)(gridResolution * gridResolution)) > 0.75f;
    }

    bool IsNormalFlat(Vector3 normal)
    {
        // Compara la normal con la dirección (0, 1, 0), permitiendo un pequeño margen de error
        return Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 1f - maxSlope;
    }

    public void Recalculate()
    {
        calculate = true;
    }
}
