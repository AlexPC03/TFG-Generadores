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
        foreach (KingdomController k in kingdoms)
        {
            k.SetRelationLines();
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
