using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GK;
using UnityEngine;

public class CountryGenerator : MonoBehaviour
{
    private ElementManagement manager;
    private void Start()
    {
        manager = GetComponent<ElementManagement>();
    }
    private class PaisData
    {
        public int id;
        public List<List<Vector2>> celdas = new();
        public Color color;
        public GameObject meshGO;
    }

    private Dictionary<KingdomController, int> ciudadAPais = new();
    private Dictionary<int, PaisData> paises = new();
    public Material materialPais;
    public bool coloresGenerados;

    public void GenerarVoronoi(List<KingdomController> ciudades,Vector2 terrenoOrigen, Vector2 terrenoTamaño)
    {
        VoronoiCalculator vcalc = new VoronoiCalculator();
        List<Vector2> sitios = new();
        foreach(KingdomController k in ciudades)
        {
            sitios.Add(new Vector2(k.transform.position.x, k.transform.position.z));
        }
        VoronoiDiagram voronoi = vcalc.CalculateDiagram(sitios);

        // Crear polígono del terreno (convexo, en sentido horario o antihorario)
        List<Vector2> poligonoDelimitador = new List<Vector2>
        {
            terrenoOrigen,
            terrenoOrigen + new Vector2(terrenoTamaño.x, 0),
            terrenoOrigen + terrenoTamaño,
            terrenoOrigen + new Vector2(0, terrenoTamaño.y)
        };

        VoronoiClipper clipper = new VoronoiClipper();
        List<List<Vector2>> celdasClipped = new List<List<Vector2>>();

        for (int i = 0; i < sitios.Count; i++)
        {
            List<Vector2> clipped = new();
            clipper.ClipSite(voronoi, poligonoDelimitador, i, ref clipped);
            celdasClipped.Add(clipped);
        }

        AgruparCiudadesEnPaises(ciudades,celdasClipped);

        foreach (var pais in paises.Values)
        {
            CrearMeshParaPais(pais);
        }

        CrearFronterasConLineRenderers(ciudades, celdasClipped);
    }

    private void AgruparCiudadesEnPaises(List<KingdomController> ciudades, List<List<Vector2>> celdasClipped)
    {
        ciudadAPais.Clear();
        paises.Clear();

        int paisID = 0;
        HashSet<KingdomController> visitadas = new();

        for (int i = 0; i < ciudades.Count; i++)
        {
            var ciudad = ciudades[i];
            if (visitadas.Contains(ciudad))
                continue;

            Queue<KingdomController> cola = new();
            cola.Enqueue(ciudad);
            visitadas.Add(ciudad);

            Color auxColor = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
            auxColor.a = 0.15f;
            PaisData pais = new PaisData
            {
                id = paisID,
                color = auxColor
            };

            while (cola.Count > 0)
            {
                var actual = cola.Dequeue();
                int idx = ciudades.IndexOf(actual);

                ciudadAPais[actual] = paisID;
                pais.celdas.Add(celdasClipped[idx]);

                foreach (var ally in actual.allies)
                {
                    if (!visitadas.Contains(ally))
                    {
                        visitadas.Add(ally);
                        cola.Enqueue(ally);
                    }
                }
            }

            paises.Add(paisID, pais);
            paisID++;
        }
    }

    private void CrearMeshParaPais(PaisData pais)
    {
        List<Vector3> vertices = new();
        List<int> indices = new();

        int vertexOffset = 0;

        foreach (var celda in pais.celdas)
        {
            if (celda.Count < 3) continue;

            // Triangulación simple en forma de fan
            for (int i = 1; i < celda.Count - 1; i++)
            {
                vertices.Add(new Vector3(celda[0].x, 0f, celda[0].y));
                vertices.Add(new Vector3(celda[i].x, 0f, celda[i].y));
                vertices.Add(new Vector3(celda[i + 1].x, 0f, celda[i + 1].y));

                indices.Add(vertexOffset);
                indices.Add(vertexOffset + 1);
                indices.Add(vertexOffset + 2);

                vertexOffset += 3;
            }
        }

        if (vertices.Count == 0) return;

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject go = new GameObject($"Pais_{pais.id}");
        go.transform.parent = this.transform.Find("Territorios");
        go.transform.localScale = new Vector3(1,-1,1);
        go.transform.position = new Vector3(0, 290, 0);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();

        mf.sharedMesh = mesh;

        Material mat = new Material(materialPais);
        mat.color = pais.color;
        mr.material = mat;

        pais.meshGO = go;
        manager.kingdomsFrontiers.Add(go);
    }

    private void CrearFronterasConLineRenderers(List<KingdomController> ciudades, List<List<Vector2>> celdasClipped)
    {
        var aristasDibujadas = new HashSet<(Vector2, Vector2)>();

        for (int i = 0; i < celdasClipped.Count; i++)
        {
            var celdaA = celdasClipped[i];
            var ciudadA = ciudades[i];

            for (int j = 0; j < celdaA.Count; j++)
            {
                Vector2 p0 = celdaA[j];
                Vector2 p1 = celdaA[(j + 1) % celdaA.Count];

                bool bordeCompartidoConAliado = false;

                for (int k = 0; k < celdasClipped.Count; k++)
                {
                    if (k == i) continue;

                    var ciudadB = ciudades[k];
                    if (!ciudadA.allies.Contains(ciudadB)) continue;

                    var celdaB = celdasClipped[k];

                    for (int m = 0; m < celdaB.Count; m++)
                    {
                        Vector2 q0 = celdaB[m];
                        Vector2 q1 = celdaB[(m + 1) % celdaB.Count];

                        if ((Vector2.Distance(p0, q1) < 0.01f && Vector2.Distance(p1, q0) < 0.01f) ||
                            (Vector2.Distance(p0, q0) < 0.01f && Vector2.Distance(p1, q1) < 0.01f))
                        {
                            bordeCompartidoConAliado = true;
                            break;
                        }
                    }

                    if (bordeCompartidoConAliado)
                        break;
                }

                if (!bordeCompartidoConAliado)
                {
                    var key = OrdenarPuntos(p0, p1);
                    if (!aristasDibujadas.Contains(key))
                    {
                        CrearLinea(p0, p1);
                        aristasDibujadas.Add(key);
                    }
                }
            }
        }
    }

    private void CrearLinea(Vector2 p0, Vector2 p1)
    {
        GameObject lineaGO = new GameObject($"Frontera_{p0}_{p1}");
        lineaGO.transform.parent = this.transform.Find("Fronteras");

        LineRenderer lr = lineaGO.AddComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.SetPosition(0, new Vector3(p0.x, 291f, p0.y));
        lr.SetPosition(1, new Vector3(p1.x, 291f, p1.y));

        lr.material = new Material(Shader.Find("Sprites/Default")); // Compatible con URP
        lr.startColor = lr.endColor = Color.black;
        lr.startWidth = lr.endWidth = 1f;
        lr.useWorldSpace = true;

        // Ajustes opcionales:
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.sortingOrder = 10;

        manager.kingdomsFrontiers.Add(lineaGO);
    }

    private (Vector2, Vector2) OrdenarPuntos(Vector2 a, Vector2 b)
    {
        return (a.x < b.x || (a.x == b.x && a.y < b.y)) ? (a, b) : (b, a);
    }
}
