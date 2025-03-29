using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentedModelBitmaskConexion : MonoBehaviour
{
    public Mesh[] completeModels;
    public Mesh[] straight1Models;
    public Mesh[] straight2Models;
    public Mesh[] cornerModels;
    public Mesh[] interiorCornerModels;
    public MeshFilter meshFilterUR;
    public MeshFilter meshFilterDR;
    public MeshFilter meshFilterDL;
    public MeshFilter meshFilterUL;
    public Vector2 tile;
    public void Set(bool U, bool UR, bool R, bool DR, bool D, bool DL, bool L, bool UL)
    {
        if (U)
        {
            if (UR)
            {
                if (R)
                {
                    meshFilterUR.mesh = completeModels[Random.Range(0, completeModels.Length)]; //Completo
                }
                else
                {
                    meshFilterUR.mesh = straight1Models[Random.Range(0, straight1Models.Length)]; //Recto1
                }
            }
            else
            {
                if (R)
                {
                    meshFilterUR.mesh = interiorCornerModels[Random.Range(0, interiorCornerModels.Length)]; //Esquina interior
                }
                else
                {
                    meshFilterUR.mesh = straight1Models[Random.Range(0, straight1Models.Length)]; //Recto1
                }
            }
            if (UL)
            {
                if (L)
                {
                    meshFilterUL.mesh = completeModels[Random.Range(0, completeModels.Length)]; //Completo
                }
                else
                {
                    meshFilterUL.mesh = straight1Models[Random.Range(0, straight1Models.Length)]; //Recto1
                }
            }
            else
            {
                if (L)
                {
                    meshFilterUL.mesh = interiorCornerModels[Random.Range(0, interiorCornerModels.Length)]; //Esquina interior
                }
                else
                {
                    meshFilterUL.mesh = straight1Models[Random.Range(0, straight1Models.Length)]; //Recto1
                }
            }
        }
        else
        {
            if (R)
            {
                meshFilterUR.mesh = straight2Models[Random.Range(0, straight2Models.Length)]; //Recto2
            }
            else
            {
                meshFilterUR.mesh = cornerModels[Random.Range(0, cornerModels.Length)]; //Esquina exterior
            }
            if (L)
            {
                meshFilterUL.mesh = straight2Models[Random.Range(0, straight2Models.Length)]; //Recto2
            }
            else
            {
                meshFilterUL.mesh = cornerModels[Random.Range(0, cornerModels.Length)]; //Esquina exterior
            }
        }
        if (D)
        {
            if (DR)
            {
                if (R)
                {
                    meshFilterDR.mesh = completeModels[Random.Range(0, completeModels.Length)]; //Completo
                }
                else
                {
                    meshFilterDR.mesh = straight1Models[Random.Range(0, straight1Models.Length)]; //Recto1
                }
            }
            else
            {
                if (R)
                {
                    meshFilterDR.mesh = interiorCornerModels[Random.Range(0, interiorCornerModels.Length)]; //Esquina interior
                }
                else
                {
                    meshFilterDR.mesh = straight1Models[Random.Range(0, straight1Models.Length)]; //Recto1
                }
            }
            if (DL)
            {
                if (L)
                {
                    meshFilterDL.mesh = completeModels[Random.Range(0, completeModels.Length)]; //Completo
                }
                else
                {
                    meshFilterDL.mesh = straight1Models[Random.Range(0, straight1Models.Length)]; //Recto1
                }
            }
            else
            {
                if (L)
                {
                    meshFilterDL.mesh = interiorCornerModels[Random.Range(0, interiorCornerModels.Length)]; //Esquina interior
                }
                else
                {
                    meshFilterDL.mesh = straight1Models[Random.Range(0, straight1Models.Length)]; //Recto1
                }
            }
        }
        else
        {
            if (R)
            {
                meshFilterDR.mesh = straight2Models[Random.Range(0, straight2Models.Length)]; //Recto2
            }
            else
            {
                meshFilterDR.mesh = cornerModels[Random.Range(0, cornerModels.Length)]; //Esquina exterior
            }
            if (L)
            {
                meshFilterDL.mesh = straight2Models[Random.Range(0, straight2Models.Length)]; //Recto2
            }
            else
            {
                meshFilterDL.mesh = cornerModels[Random.Range(0, cornerModels.Length)]; //Esquina exterior
            }
        }
    }
}
