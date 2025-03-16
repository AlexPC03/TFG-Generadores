using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElementManagement : MonoBehaviour
{
    public List<GameObject> river;
    public List<ResourceInfo> resources;
    public List<KingdomController> kingdoms;

    private void Awake()
    {
        river = new List<GameObject>();
        resources = new List<ResourceInfo>();
        kingdoms = new List<KingdomController>();
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
            k.allies.RemoveAll(x => k.rivals.Contains(x));
        }
        foreach(KingdomController k in kingdoms)
        {

            // Obtener todos los aliados en cualquier nivel de profundidad
            HashSet<KingdomController> allAllies = GetAllAllies(new HashSet<KingdomController>(),k,3);

            List<KingdomController> rivalsToRemove = new List<KingdomController>();

            foreach (KingdomController rival in k.rivals)
            {
                // Verificar si el rival es aliado de algún aliado
                foreach (KingdomController ally in k.allies)
                {
                    if (allAllies.Contains(rival))
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
            k.SetRelationLines();
        }
    }

    private HashSet<KingdomController> GetAllAllies(HashSet<KingdomController> checkedAllies,KingdomController k, int depth)
    {
        foreach (KingdomController ally in k.allies)
        {
            if (!checkedAllies.Contains(ally))
            {
                checkedAllies.Add(ally);
                if (depth >0)
                {
                    GetAllAllies(checkedAllies,ally,depth-1); // Llamada recursiva
                }
            }
        }
        return checkedAllies;
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
