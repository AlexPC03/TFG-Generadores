using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverGenerator : MonoBehaviour
{
    [Range(1,16)]
    public int riverNumber;
    public bool calculate;
    List<GameObject> riverStart;
    Terrain terrain;

    // Start is called before the first frame update
    void Start()
    {
        calculate=true;
        riverStart = new List<GameObject>();
        for(int r=0;r<riverNumber;r++)
        {
            riverStart.Add(new GameObject("River"));
            riverStart[r].tag = "River";
            riverStart[r].AddComponent<LineRenderer>();
        }
    }

    private void LateUpdate()
    {
        if (calculate)
        {
            calculate = false;

            // Obtener la referencia al terreno activo
            terrain = GetComponent<Terrain>();
            TerrainData terrainData = terrain.terrainData;
            // Calcular el punto más alto del terreno
            float highestPoint = GetHighestPoint(terrain);
            for (int r = 0; r < riverNumber; r++)
            {
                GameObject river = riverStart[r];
                RaycastHit hit;
                for(int i=0; i < terrainData.size.z; i++)
                {
                    switch (r)
                    {
                        case 1:
                            Physics.Raycast(transform.position + new Vector3(i, highestPoint - Random.Range(5f,10f), terrainData.size.z / 2), Vector3.forward, out hit, terrainData.size.z/2);
                            break;
                        case 2:
                            Physics.Raycast(transform.position + new Vector3(terrainData.size.x / 2, highestPoint - Random.Range(5f, 10f), i), Vector3.right, out hit, terrainData.size.x);
                            break;
                        case 3:
                            Physics.Raycast(transform.position + new Vector3(i, highestPoint - Random.Range(5f, 10f), terrainData.size.z / 2), -Vector3.forward, out hit, terrainData.size.z);
                            break;
                        case 4:
                            Physics.Raycast(transform.position + new Vector3(terrainData.size.x / 2, highestPoint - Random.Range(5f, 10f), i), -Vector3.right, out hit, terrainData.size.x);
                            break;
                        case 5:
                            Physics.Raycast(transform.position + new Vector3(terrainData.size.x-i, highestPoint - Random.Range(5f, 10f), terrainData.size.z / 2), Vector3.forward, out hit, terrainData.size.z);
                            break;
                        case 6:
                            Physics.Raycast(transform.position + new Vector3(terrainData.size.x / 2, highestPoint - Random.Range(5f, 10f), terrainData.size.z-i), Vector3.right, out hit, terrainData.size.x);
                            break;
                        case 7:
                            Physics.Raycast(transform.position + new Vector3(terrainData.size.x-i, highestPoint - Random.Range(5f, 10f), terrainData.size.z / 2), -Vector3.forward, out hit, terrainData.size.z);
                            break;
                        case 8:
                            Physics.Raycast(transform.position + new Vector3(terrainData.size.z / 2, highestPoint - Random.Range(5f, 10f), terrainData.size.z-i), -Vector3.right, out hit, terrainData.size.x);
                            break;

                        case 9:
                            Physics.Raycast(transform.position + new Vector3(i, highestPoint - Random.Range(5f, 10f), 15), Vector3.forward, out hit, terrainData.size.z);
                            break;
                        case 10:
                            Physics.Raycast(transform.position + new Vector3(15, highestPoint - Random.Range(5f, 10f), i), Vector3.right, out hit, terrainData.size.x);
                            break;
                        case 11:
                            Physics.Raycast(transform.position + new Vector3(i, highestPoint - Random.Range(5f, 10f), terrainData.size.z - 15), -Vector3.forward, out hit, terrainData.size.z);
                            break;
                        case 12:
                            Physics.Raycast(transform.position + new Vector3(terrainData.size.x - 15, highestPoint - Random.Range(5f, 10f), i), -Vector3.right, out hit, terrainData.size.x);
                            break;
                        case 13:
                            Physics.Raycast(transform.position + new Vector3(terrainData.size.x - i, highestPoint - Random.Range(5f, 10f), 10), Vector3.forward, out hit, terrainData.size.z);
                            break;
                        case 14:
                            Physics.Raycast(transform.position + new Vector3(10, highestPoint - Random.Range(5f, 10f), terrainData.size.z - i), Vector3.right, out hit, terrainData.size.x);
                            break;
                        case 15:
                            Physics.Raycast(transform.position + new Vector3(terrainData.size.x - i, highestPoint - Random.Range(5f, 10f), terrainData.size.z - 10), -Vector3.forward, out hit, terrainData.size.z);
                            break;
                        case 16:
                            Physics.Raycast(transform.position + new Vector3(terrainData.size.x - 10, highestPoint - Random.Range(5f, 10f), terrainData.size.z - i), -Vector3.right, out hit, terrainData.size.x);
                            break;
                        default:
                            Physics.Raycast(transform.position + new Vector3(i, highestPoint - Random.Range(5f, 10f), 10), Vector3.forward, out hit, terrainData.size.z);
                            break;
                    }
                    if (hit.collider != null)
                    {
                        river.transform.position=hit.point;
                        river.GetComponent<LineRenderer>().numCapVertices = 5;
                        river.GetComponent<LineRenderer>().numCapVertices = 2;
                        river.GetComponent<LineRenderer>().startWidth = Random.Range(2f,4f);
                        river.GetComponent<LineRenderer>().endWidth = Random.Range(1f, 2f);
                        river.GetComponent<LineRenderer>().material = new Material(Shader.Find("Sprites/Default")) { color = Color.cyan };
                        GenerateRiver(river.transform, 100, 0.5f);
                        break;
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
            riverPath.Add(currentPosition);

            // Realiza un raycast hacia abajo para obtener la normal del terreno
            if (Physics.Raycast(currentPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, Mathf.Infinity))
            {
                Vector3 normal = hit.normal;

                // Calcula la dirección hacia abajo a lo largo de la pendiente
                Vector3 flowDirection = Vector3.Cross(Vector3.Cross(normal, Vector3.down), normal).normalized;

                // Si la pendiente es demasiado baja, detén el flujo
                if (flowDirection.magnitude < -0.1)
                {
                    Vector3 fallbackDirection = FindDirectionAround(hit.point,stepSize,10);
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

                currentPosition = nextPosition;
            }
            else
            {
                Debug.Log("No se detectó terreno debajo del punto actual.");
                break;
            }
        }
        // Visualizar el río usando LineRenderer
        for(int n=0;n<riverPath.Count;n++)
        {
            if (n!=0 && n % 5==0)
            {
                GameObject auxriv=new GameObject("River");
                auxriv.transform.position = riverPath[n];
                auxriv.transform.parent = startPoint;
                auxriv.tag="River";
            }
        }
        startPoint.GetComponent<LineRenderer>().positionCount = riverPath.Count;
        startPoint.GetComponent<LineRenderer>().SetPositions(riverPath.ToArray());
    }

    Vector3 FindDirectionAround(Vector3 centerPoint, float stepSize,float searchRadius)
    {
        float bestHeightDifference = 0f;
        Vector3 bestDirection = Vector3.zero;

        // Busca en un patrón en espiral dentro del radio definido
        for (float radius = stepSize; radius <= searchRadius; radius += stepSize)
        {
            for (float angle = 0; angle < 360; angle += 15) // Incrementos de 15 grados
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

    private float GetHighestPoint(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;

        // Obtén las alturas del terreno
        float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

        float maxHeight = float.MinValue;
        Vector3 highestPoint = Vector3.zero;

        // Iterar por todas las alturas
        for (int x = 0; x < heights.GetLength(0); x++)
        {
            for (int y = 0; y < heights.GetLength(1); y++)
            {
                float height = heights[x, y] * terrainData.size.y; // Escalar la altura al tamaño real del terreno

                if (height > maxHeight)
                {
                    maxHeight = height;
                }
            }
        }
        return maxHeight;
    }
}
