using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerationPerlinNoise : MonoBehaviour
{
    public int depth = 20;

    public int width = 256;
    public int height = 256;
    [Range(-1, 1)]
    public float startHeight;

    [Range(0, 1)]
    public float maxHeightAbs;
    [Range(0, 1)]
    public float minHeightAbs;



    public NoiseLayer[] capas;//Escalas para cada una de las iteraciones

    [Serializable]
    public struct NoiseLayer
    {
        public string name;
        public float scale;
        public Operation operation;
        [Header("Jumps")]
        [Range(0, 1)]
        public float[] jumps;
        [Range(0, 1)]
        public float maxCap;
        [Range(0, 1)]
        public float minCap;
        public CapType capType;
        public float heightOffset;
        [Range(0,1)]
        public float weight;
    }

    public enum Operation
    {
        sum,
        rest,
        mult,
        div
    }

    public enum CapType
    {
        normal,
        cliff
    }

    private List<Vector2> offsets;
    public bool newOffsets;

    private void Start()
    {
        ResetOffset();
    }


    private void Update()
    {
        if (newOffsets)
        {
            newOffsets = false;
            ResetOffset();
        }
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    private TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size=new Vector3(width,depth,height);

        terrainData.SetHeights(0, 0, GenerateHeights());

        return terrainData;
    }

    private float[,] GenerateHeights()
    {
        float[,] totalheights = new float[width, height];
        for (int i = 0; i < capas.Length; i++)
        {
            float[,] heights = new float[width, height];
            for (int x=0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    heights[x, y] = CalculateHeight(x, y, capas[i].scale,i);
                    totalheights[x,y]= LayerOperation(totalheights[x, y], heights[x, y],capas[i]);
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (totalheights[x,y]>maxHeightAbs)
                {
                    totalheights[x, y] = maxHeightAbs;
                }
                if (totalheights[x, y] < minHeightAbs)
                {
                    totalheights[x, y] = minHeightAbs;
                }
            }
        }
        return totalheights;
    }

    private float CalculateHeight(int x, int y,float scale,int index)
    {
        float xCoord=(float)x/width * scale + offsets[index].x;
        float yCoord=(float)y /height * scale+ offsets[index].y;
        return Mathf.Clamp(Mathf.PerlinNoise(xCoord,yCoord),0,1);
    }

    private float LayerOperation(float totalHeight, float height, NoiseLayer capa)
    {
        float totalH=0;
        if (capa.jumps.Length > 1)
        {
            for(int j=0; j< capa.jumps.Length-1;j++)
            {
                if(height> capa.jumps[j] && height < capa.jumps[j+1])
                {
                    switch (capa.operation)
                    {
                        case Operation.sum:
                            totalH += capa.jumps[j] * capa.weight;
                            break;
                        case Operation.rest:
                            totalH -= capa.jumps[j] * capa.weight;
                            break;
                    }
                }
            }
        }
        switch (capa.operation)
        {
            case Operation.sum:
                totalH += height *capa.weight;
                break;
            case Operation.rest:
                totalH -= height * capa.weight;
                break;
            case Operation.mult:
                return totalHeight * (height+capa.heightOffset+capa.weight*5);
            case Operation.div:
                return totalHeight / (height + capa.heightOffset + capa.weight * 5);
        }
        if (height > capa.maxCap)
        {
            if (capa.capType == CapType.cliff)
            {
                return totalHeight;
            }
            totalH += capa.maxCap * capa.weight;
        }
        if (height < capa.minCap)
        {
            if (capa.capType == CapType.cliff)
            {
               return totalHeight;
            }
            totalH += capa.minCap * capa.weight;
        }
        if (capa.heightOffset != 0)
        {
            totalH += capa.heightOffset;
        }
        return totalHeight + totalH;
    }

    private void ResetOffset()
    {
        offsets = new List<Vector2>();
        for (int i = 0; i < capas.Length; i++)
        {
            offsets.Add(new Vector2(UnityEngine.Random.Range(0, 9999f), UnityEngine.Random.Range(0, 9999f)));
        }
    }
}
