using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocatorFunctions : MonoBehaviour
{
    protected float NearestOther(Vector3 pos,List<GameObject> otherPos,bool noHight=false)
    {
        float minDistance = float.PositiveInfinity;
        if (otherPos.Count <= 0)
        {
            return float.PositiveInfinity;
        }
        foreach (GameObject obj in otherPos)
        {
            var dist = (obj.transform.position - pos).magnitude; 
            if (dist < minDistance)
            {
                minDistance = dist;
            }
        }
        return minDistance;
    }

    protected float NearestRiver(Vector3 pos, bool noHight = false)
    {
        var arr = GameObject.FindGameObjectsWithTag("River");
        if (arr.Length <= 0)
        {
            return float.PositiveInfinity;
        }
        float minDistance = float.PositiveInfinity;
        GameObject nearest = null;
        foreach (GameObject obj in arr)
        {
            float dist = 0;
            if (noHight)
            {
                dist = (new Vector3(obj.transform.position.x,0,obj.transform.position.z)  - new Vector3(pos.x, 0, pos.z)).magnitude;
            }
            else
            {
                dist = (obj.transform.position - pos).magnitude;
            }
            if (dist < minDistance)
            {
                nearest = obj;
                minDistance = dist;
            }
        }
        return minDistance;
    }

    protected Vector3 NearestRiverPoint(Vector3 pos)
    {
        var arr = GameObject.FindGameObjectsWithTag("River");
        if (arr.Length <= 0)
        {
            return pos;
        }
        float minDistance = float.PositiveInfinity;
        GameObject nearest = null;
        foreach (GameObject obj in arr)
        {
            var dist = (obj.transform.position - pos).magnitude;
            if (dist < minDistance)
            {
                nearest = obj;
                minDistance = dist;
            }
        }
        return nearest.transform.position;
    }

    protected float NearestFertileField(Vector3 pos, bool noHight = false)
    {
        var arr = GameObject.FindGameObjectsWithTag("FertileField");
        if (arr.Length <= 0)
        {
            return float.PositiveInfinity;
        }
        float minDistance = float.PositiveInfinity;
        GameObject nearest = null;
        foreach (GameObject obj in arr)
        {
            float dist = 0;
            if (noHight)
            {
                dist = (new Vector3(obj.transform.position.x, 0, obj.transform.position.z) - new Vector3(pos.x, 0, pos.z)).magnitude;
            }
            else
            {
                dist = (obj.transform.position - pos).magnitude;
            }
            if (dist < minDistance)
            {
                nearest = obj;
                minDistance = dist;
            }
        }
        return minDistance;
    }

    protected float NearestEmptyField(Vector3 pos, bool noHight = false)
    {
        var arr = GameObject.FindGameObjectsWithTag("EmptyField");
        if (arr.Length <= 0)
        {
            return float.PositiveInfinity;
        }
        float minDistance = float.PositiveInfinity;
        GameObject nearest = null;
        foreach (GameObject obj in arr)
        {
            float dist = 0;
            if (noHight)
            {
                dist = (new Vector3(obj.transform.position.x, 0, obj.transform.position.z) - new Vector3(pos.x, 0, pos.z)).magnitude;
            }
            else
            {
                dist = (obj.transform.position - pos).magnitude;
            }
            if (dist < minDistance)
            {
                nearest = obj;
                minDistance = dist;
            }
        }
        return minDistance;
    }

    protected float NearestColdField(Vector3 pos, bool noHight = false)
    {
        var arr = GameObject.FindGameObjectsWithTag("EmptyField");
        if (arr.Length <= 0)
        {
            return float.PositiveInfinity;
        }
        float minDistance = float.PositiveInfinity;
        GameObject nearest = null;
        foreach (GameObject obj in arr)
        {
            float dist = 0;
            if (noHight)
            {
                dist = (new Vector3(obj.transform.position.x, 0, obj.transform.position.z) - new Vector3(pos.x, 0, pos.z)).magnitude;
            }
            else
            {
                dist = (obj.transform.position - pos).magnitude;
            }
            if (dist < minDistance)
            {
                nearest = obj;
                minDistance = dist;
            }
        }
        return minDistance;
    }
}
