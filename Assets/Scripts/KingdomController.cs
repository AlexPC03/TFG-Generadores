using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingdomController : LocatorFunctions
{
    public List<GameObject> resources = new List<GameObject>();
    public List<KingdomController> rivals = new List<KingdomController>();
    public List<KingdomController> allies = new List<KingdomController>();
    public int mineralForce;
    public int agriculture;
    public int animals;
    public int seaOpenings;
    public int foodSources;
    public KingdomType type;
    public KingdomType secondaryType;
    public enum KingdomType
    {
        None,
        Agricultor,
        Ganadero,
        Minero,
        Cazador,
        Pesquero
    }
    private int[] arr = { 0, 0, 0, 0, 0 };

    public void SetType()
    {
        foreach(GameObject res in resources)
        {
            switch (res.tag)
            {
                case "EmptyField":
                    animals+=2;
                    agriculture++;
                    foodSources+=2;
                    arr[1]+=2;
                    break;
                case "FertileField":
                    agriculture += 3;
                    foodSources += 4;
                    arr[0]+=5;
                    break;
                case "ForestField":
                    animals++;
                    foodSources++;
                    arr[2]++;
                    break;
                case "SeaField":
                    foodSources += 3;
                    seaOpenings++;
                    arr[3]+=3;
                    break;
                case "SmallMine":
                    mineralForce++;
                    arr[4]+=5;
                    break;
                case "GreatMine":
                    mineralForce+=3;
                    arr[4] += 10;
                    break;
            }
        }
        int max = -1;
        int max2 = -1;
        int pos = -1;
        int pos2 = -1;
        for (int i=0; i < arr.Length; i++)
        {
            if (arr[i] >= max)
            {
                pos = i;
                max=arr[i];
            }
        }
        for (int i = 0; i < arr.Length; i++)
        {
            if (i == pos) continue;
            if (arr[i] >= max2)
            {
                pos2 = i;
                max2 = arr[i];
            }
        }
        switch (pos)
        {
            case 0:
                type = KingdomType.Agricultor;
                break;
            case 1:
                type = KingdomType.Ganadero;
                break;
            case 2:
                type = KingdomType.Cazador;
                break;
            case 3:
                type = KingdomType.Pesquero;
                break;
            case 4:
                type = KingdomType.Minero;
                break;
        }
        switch (pos2)
        {
            case 0:
                secondaryType = KingdomType.Agricultor;
                break;
            case 1:
                secondaryType = KingdomType.Ganadero;
                break;
            case 2:
                secondaryType = KingdomType.Cazador;
                break;
            case 3:
                secondaryType = KingdomType.Pesquero;
                break;
            case 4:
                secondaryType = KingdomType.Minero;
                break;
        }
    }

    public void SetRelations()
    {
        foreach (GameObject res in resources)
        {
            if (res.GetComponent<ResourceInfo>().interestedKingdoms.Count > 1)
            {
                List<KingdomController> aux = new List<KingdomController>(res.GetComponent<ResourceInfo>().interestedKingdoms);
                aux.RemoveAll(x => x == this);
                foreach(KingdomController k in aux)
                {
                    switch (res.tag)
                    {
                        case "EmptyField":
                            if((Math.Abs(animals-k.animals)<3 && Math.Abs(agriculture - k.agriculture) < 3) || 
                                (type==KingdomType.Ganadero && k.type==KingdomType.Ganadero) || (secondaryType == KingdomType.Ganadero && k.secondaryType == KingdomType.Ganadero) ||
                                (type == KingdomType.Agricultor && k.type == KingdomType.Agricultor) || (secondaryType == KingdomType.Agricultor && k.secondaryType == KingdomType.Agricultor))
                            {
                                allies.Add(k);
                            }
                            else
                            {
                                rivals.Add(k);
                            }
                            break;
                        case "FertileField":
                            if (Math.Abs(agriculture - k.agriculture) < 3 ||
                                (type == KingdomType.Agricultor && k.type == KingdomType.Agricultor) || (secondaryType == KingdomType.Agricultor && k.secondaryType == KingdomType.Agricultor))
                            {
                                allies.Add(k);
                            }
                            else
                            {
                                rivals.Add(k);
                            }
                            break;
                        case "ForestField":
                            if (Math.Abs(animals - k.animals) < 3 ||
                                (((type == KingdomType.Cazador && k.type != KingdomType.Cazador)|| (secondaryType == KingdomType.Cazador && k.secondaryType != KingdomType.Cazador)) && Math.Abs(foodSources - k.foodSources) < 5))
                            {
                                allies.Add(k);
                            }
                            else
                            {
                                rivals.Add(k);
                            }
                            break;
                        case "SeaField":
                            if (Math.Abs(foodSources - k.foodSources) < 5 ||
                                (type == KingdomType.Pesquero && k.type == KingdomType.Pesquero) || (secondaryType == KingdomType.Pesquero && k.secondaryType == KingdomType.Pesquero))
                            {
                                allies.Add(k);
                            }
                            else
                            {
                                rivals.Add(k);
                            }
                            break;
                        case "SmallMine":
                            if ((type != KingdomType.Minero && k.type != KingdomType.Minero)|| (secondaryType != KingdomType.Minero && k.secondaryType != KingdomType.Minero))
                            {
                                allies.Add(k);
                            }
                            else
                            {
                                rivals.Add(k);
                            }
                            break;
                        case "GreatMine":
                            rivals.Add(k);
                            break;
                    }
                }
            }
        }
    }

    public void SetRelationLines()
    {
        if (rivals.Count > 0)
        {
            foreach(KingdomController k in rivals)
            {
                GameObject a = new GameObject("RivalLine");
                a.transform.position = transform.position;
                a.transform.SetParent(transform);
                LineRenderer rLine = a.AddComponent<LineRenderer>();
                rLine.positionCount = 4;
                rLine.SetPosition(0, transform.position);
                rLine.SetPosition(1, new Vector3(transform.position.x, 45, transform.position.z));
                rLine.SetPosition(2, new Vector3(k.transform.position.x, 45, k.transform.position.z));
                rLine.SetPosition(3, k.transform.position);
                rLine.startWidth=1.5f;
                rLine.endWidth=1.5f;
                rLine.material = new Material(Shader.Find("Sprites/Default")) { color = Color.red };
            }
        }
        if (allies.Count > 0)
        {
            foreach (KingdomController k in allies)
            {
                GameObject a = new GameObject("AllyLine");
                a.transform.position = transform.position;
                a.transform.SetParent(transform);
                LineRenderer rLine = a.AddComponent<LineRenderer>();
                rLine.positionCount = 4;
                rLine.SetPosition(0, transform.position);
                rLine.SetPosition(1, new Vector3(transform.position.x, 45, transform.position.z));
                rLine.SetPosition(2, new Vector3(k.transform.position.x, 45, k.transform.position.z));
                rLine.SetPosition(3, k.transform.position);
                rLine.startWidth = 1.5f;
                rLine.endWidth = 1.5f;
                rLine.material = new Material(Shader.Find("Sprites/Default")) { color = Color.blue };
            }
        }
    }
}
