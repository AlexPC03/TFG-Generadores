using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverGenerator : MonoBehaviour
{
    public Mesh circleMesh;
    [Range(1,64)]
    public int riverNumber;
    public float riverMinHeight;
    public float riverMaxHeight;
    private bool calculate;
    private ElementManagement manager;
    private TerrainGenerationPerlinNoise terrainGenerator;

    // Start is called before the first frame update
    void Start()
    {
        calculate=true;
        manager=GetComponent<ElementManagement>();
        terrainGenerator = GetComponent<TerrainGenerationPerlinNoise>();
    }

    private void LateUpdate()
    {
        if (calculate)
        {
            Vector3 allSize= terrainGenerator.terrains[0].terrainData.size;
            foreach (Terrain terrain in terrainGenerator.terrains)
            {

                calculate = false;
                // Calcular el punto más alto del terreno
                //float highestPoint = GetHighestPoint(terrain);
                for (int r = 0; r < riverNumber; r++)
                {
                    bool reached=false;
                    Vector3 point = Vector3.zero;
                    RaycastHit hit;
                    for(float j=riverMaxHeight; j>riverMinHeight; j -=0.1f) 
                    {



                        //GameObject aux = new GameObject("Aux");
                        //aux.transform.forward = Quaternion.AngleAxis(360/ riverNumber * r, Vector3.up) * Vector3.forward;

                        if (Physics.Raycast(terrain.transform.position + new Vector3(allSize.x / 2, j, allSize.z / 2), Quaternion.AngleAxis(360/ riverNumber * r, Vector3.up) * Vector3.forward, out hit, allSize.z*100))
                        {
                            point = hit.point;
                            reached = true;
                            break;
                        }
                    }                    
                    if (reached)
                    {
                        GameObject river = new GameObject("River");
                        manager.river.Add(river);
                        manager.river[manager.river.Count-1].tag = "River";
                        manager.river[manager.river.Count-1].AddComponent<LineRenderer>();
                        river.transform.position=point;
                        river.GetComponent<LineRenderer>().numCapVertices = 5;
                        river.GetComponent<LineRenderer>().numCapVertices = 2;
                        river.GetComponent<LineRenderer>().startWidth = Random.Range(2f,4f);
                        river.GetComponent<LineRenderer>().endWidth = Random.Range(1f, 2f);
                        river.GetComponent<LineRenderer>().material = new Material(Shader.Find("Sprites/Default")) { color = Color.cyan };
                        GenerateRiver(river.transform, 5000, 0.5f);
                    }
                }               
            }
        }
    }

    void GenerateRiver(Transform startPoint,int maxSteps, float stepSize)
    {
        Vector3 currentPosition = startPoint.position;
        List<Vector3> riverPath = new List<Vector3>();
        for (int i = 0; i < maxSteps; i++)
        {
            riverPath.Add(currentPosition+Vector3.up*0.1f);

            // Realiza un raycast hacia abajo para obtener la normal del terreno
            if (Physics.Raycast(currentPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, Mathf.Infinity))
            {
                Vector3 normal = hit.normal;

                // Calcula la dirección hacia abajo a lo largo de la pendiente
                Vector3 rawflowDirection = Vector3.Cross(Vector3.Cross(normal, Vector3.down), normal);
                Vector3 flowDirection = rawflowDirection.normalized;

                // Si la pendiente es demasiado baja, detén el flujo
                if (rawflowDirection.magnitude <= 0)
                {
                    Vector3 fallbackDirection = FindDirectionAround(hit.point,stepSize/2,5000);
                    if (fallbackDirection == Vector3.zero)
                    {
                        Debug.Log("No se encontró una pendiente válida cerca del área plana.");
                        break;
                    }

                    flowDirection = fallbackDirection;
                }

                // Calcula el próximo punto en la dirección del flujo
                Vector3 nextPosition = currentPosition + flowDirection * stepSize;

                // Comprueba si el siguiente punto está más bajo que el actual (para evitar bucles)
                if (nextPosition.y >= currentPosition.y)
                {
                    Debug.Log("El flujo del río ha alcanzado un punto plano o ascendente.");
                    break;
                }
                else if(currentPosition.y <= 0.6)
                {
                    Debug.Log("El flujo del río ha alcanzado el mar.");
                    break;
                }

                if(Physics.Raycast(nextPosition + Vector3.up, Vector3.down, Mathf.Infinity))
                {
                    currentPosition = nextPosition;
                }
                else
                {
                    Debug.Log("No se detectó terreno debajo del punto actual.");
                    break;
                }
            }
            else
            {
                Debug.Log("No se detectó terreno debajo del punto actual.");
                break;
            }
        }
        for(int n=0;n<riverPath.Count;n++)
        {
            if (n!=0 && n % 5==0)
            {
                GameObject auxriv=new GameObject("River");
                auxriv.transform.position = riverPath[n];
                auxriv.tag = "River";
                auxriv.transform.parent = startPoint;
            }
        }
        if (riverPath[riverPath.Count - 1].y < 1)
        {
            Debug.Log("Delta generado");
            GameObject auxriv = new GameObject("Delta");
            auxriv.transform.position = riverPath[riverPath.Count - 1];
            auxriv.tag = "Delta";
            auxriv.transform.parent = startPoint;
            MeshFilter mf = auxriv.AddComponent<MeshFilter>();
            mf.mesh = circleMesh;
            MeshRenderer mr = auxriv.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Sprites/Default")) { color = Color.cyan };
            auxriv.transform.localScale = new Vector3(3f, 1, 3f);
        }
        // Visualizar el río usando LineRenderer
        startPoint.GetComponent<LineRenderer>().positionCount = riverPath.Count;
        startPoint.GetComponent<LineRenderer>().SetPositions(riverPath.ToArray());
        if (startPoint.childCount <= 0)
        {
            Destroy(startPoint.gameObject);
        }
    }

    Vector3 FindDirectionAround(Vector3 centerPoint, float stepSize,float searchRadius)
    {
        float bestHeightDifference = 0f;
        Vector3 bestDirection = Vector3.zero;

        // Busca en un patrón en espiral dentro del radio definido
        for (float radius = stepSize; radius <= searchRadius; radius += stepSize)
        {
            for (float angle = 0; angle < 360; angle += 7.5f) // Incrementos de 7.5 grados
            {
                // Calcula la posición alrededor del centro
                Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
                Vector3 searchPoint = centerPoint + offset;

                if (Physics.Raycast(searchPoint + Vector3.up * 10f, Vector3.down, out RaycastHit hit, Mathf.Infinity))
                {
                    float heightDifference = centerPoint.y - hit.point.y;

                    // Busca el punto con mayor descenso
                    if (heightDifference > bestHeightDifference)
                    {
                        bestHeightDifference = heightDifference;
                        bestDirection = (hit.point - centerPoint).normalized;
                    }
                }
            }
        }

        return bestDirection;
    }

    public void Recalculate()
    {
        calculate = true;
    }
}
