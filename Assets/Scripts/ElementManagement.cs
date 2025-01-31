using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementManagement : MonoBehaviour
{
    public List<GameObject> river;
    public List<ResourceInfo> resources;

    private void Awake()
    {
        river = new List<GameObject>();
        resources = new List<ResourceInfo>();
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
    }
}
