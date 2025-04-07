using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElementManagement : MonoBehaviour
{
    public List<GameObject> river;
    public List<ResourceInfo> resources;
    public List<KingdomController> kingdoms;
    public List<GameObject> kingdomsFrontiers;

    private void Awake()
    {
        river = new List<GameObject>();
        resources = new List<ResourceInfo>();
        kingdoms = new List<KingdomController>();
        kingdomsFrontiers = new List<GameObject>();
    }

    public void RemoveElements()
    {
        foreach (GameObject g in river)
        {
            Destroy(g);
        }
        river.Clear();
        foreach (ResourceInfo g in resources)
        {
            Destroy(g.gameObject);
        }
        resources.Clear();
        foreach (KingdomController g in kingdoms)
        {
            Destroy(g.gameObject);
        }
        kingdoms.Clear();
        foreach (GameObject g in kingdomsFrontiers)
        {
            Destroy(g);
        }
        kingdomsFrontiers.Clear();
    }

    public void KingdomRelations()
    {
        foreach(ResourceInfo r in resources)
        {
            r.interestedKingdoms=r.interestedKingdoms.Distinct().ToList();
        }
        foreach(KingdomController k in kingdoms)
        {
            k.SetRelations();
        }
        foreach (KingdomController k in kingdoms)
        {
            k.rivals= k.rivals.Distinct().ToList();
            k.allies= k.allies.Distinct().ToList();
            k.rivals.RemoveAll(x => k.allies.Contains(x));
        }
        foreach(KingdomController k in kingdoms)
        {

            // Obtener todos los aliados en cualquier nivel de profundidad
            PropagarAlianzas(kingdoms);

            List<KingdomController> rivalsToRemove = new List<KingdomController>();

            foreach (KingdomController rival in k.rivals)
            {
                // Verificar si el rival es aliado de algún aliado
                foreach (KingdomController ally in k.allies)
                {
                    if (k.allies.Contains(rival))
                    {
                        rivalsToRemove.Add(rival);
                    }
                }
            }
            // Eliminar los rivales que no cumplen la condición
            foreach (KingdomController rival in rivalsToRemove)
            {
                k.rivals.Remove(rival);
                if(!k.allies.Contains(rival))
                {
                    k.allies.Add(rival);
                }
            }
        }
        foreach (KingdomController k in kingdoms)
        {
            k.rivals = k.rivals.Distinct().ToList();
            k.allies = k.allies.Distinct().ToList();
            k.rivals.RemoveAll(x => k.allies.Contains(x));
        }
        foreach (KingdomController k in kingdoms)
        {
            k.rivals = k.rivals.Distinct().ToList();
            k.allies = k.allies.Distinct().ToList();
            k.allies.RemoveAll(x => k.rivals.Contains(x));
        }
        foreach (KingdomController k in kingdoms)
        {
            k.SetRelationLines();
        }
    }

    public static void PropagarAlianzas(List<KingdomController> ciudades)
    {
        HashSet<KingdomController> visitados = new();

        foreach (var ciudad in ciudades)
        {
            if (!visitados.Contains(ciudad))
            {
                HashSet<KingdomController> grupo = new();
                ConstruirGrupo(ciudad, grupo);

                // Añadir todos los miembros del grupo como aliados entre sí
                foreach (var miembro in grupo)
                {
                    miembro.allies = grupo.Where(c => c != miembro).ToList();
                    visitados.Add(miembro);
                }
            }
        }
    }

    private static void ConstruirGrupo(KingdomController ciudad, HashSet<KingdomController> grupo)
    {
        if (grupo.Contains(ciudad)) return;

        grupo.Add(ciudad);

        foreach (var aliado in ciudad.allies)
        {
            if (aliado != null)
                ConstruirGrupo(aliado, grupo);
        }
    }

    public void resetResourceInterests(KingdomController kingdom)
    {
        foreach(ResourceInfo res in resources)
        {
            res.interestedKingdoms.RemoveAll(x => x == kingdom);
        }
    }

    public float NearestKingdomDistance(Vector3 pos)
    {
        var arr = kingdoms.ToArray();
        if (arr.Length <= 0)
        {
            return float.PositiveInfinity;
        }
        float minDistance = float.PositiveInfinity;
        foreach (KingdomController obj in arr)
        {
            float dist = 0;
            dist = (obj.transform.position - pos).magnitude;
            if (dist < minDistance)
            {
                minDistance = dist;
            }
        }
        return minDistance;
    }
}
