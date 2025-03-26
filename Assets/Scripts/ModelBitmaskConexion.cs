using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelBitmaskConexion : MonoBehaviour
{
    public Mesh[] conexionMeshes;
    public MeshFilter meshFilter;
    public Vector2 tile;
    public void Set(int sprite)
    {
        meshFilter.mesh = conexionMeshes[Mathf.Clamp(sprite, 0, conexionMeshes.Length - 1)];
    }
}
