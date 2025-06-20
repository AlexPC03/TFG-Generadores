using sc.terrain.proceduralpainter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerationPerlinNoise : MonoBehaviour
{
    public int seed;
    public int depth = 20;
    private ElementManagement manager;
    public int width = 256;
    public int height = 256;

    public Terrain[] terrains;
    public int tilesX, tilesY;
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
        [Range(0, 1)]
        public float capSet;
        public CapType capType;
        public float heightOffset;
        [Range(0,2)]
        public float weight;
        public bool cut;
        public NoiseMask noiseMask;
    }

    [Serializable]
    public struct NoiseMask
    {
        public bool enabled;
        public float scale;
        [Range(0, 1)]
        public float maxCap;
        [Range(0, 1)]
        public float minCap;
        public MaskOperation operation;
        public float heightOffset;
        [Range(0, 2)]
        public float weight;
    }

    public enum Operation
    {
        sum,
        additive,
        rest,
        mult,
        div,
        exp
    }

    public enum MaskOperation
    {
        none,
        mult,
        div,
        exp
    }

    public enum CapType
    {
        normal,
        cliff
    }

    private List<Vector2> offsets;
    private Vector2[] maskOffsets;
    [Header("Generate cities")]
    public bool cities;
    [Header("Recalculate")]
    public bool newOffsets;


    private void Start()
    {
        newOffsets = true;
        manager=GetComponent<ElementManagement>();
    }

    private void Update()
    {
        if (newOffsets)
        {
            if (seed != 0)
            {
                UnityEngine.Random.InitState(seed);
            }
            newOffsets = false;
            ResetOffset();
            manager.RemoveElements();
            GetComponent<RiverGenerator>().Recalculate();
            GetComponent<FlatResourceGenerator>().Recalculate();
            GetComponent<KingdomGenerator>().Recalculate();
            ApplyProceduralHeightmaps();
            GetComponent<TerrainPainter>().RepaintAll();
        }
        if (cities)
        {
            cities = false;
            Invoke("GenerateAllCities", 0.1f);
        }
    }

    public void GenerateAllCities()
    {
        foreach (KingdomController k in manager.kingdoms)
        {
            k.generate = true;
        }
    }

    void ApplyProceduralHeightmaps()
    {
        int tileWidth = width / tilesX;
        int tileHeight = height / tilesY;

        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                int index = x * tilesY + y; // Para recorrer el array 1D
                if (index < terrains.Length)
                {
                    terrains[index].terrainData.heightmapResolution = tileWidth + 1;
                    terrains[index].terrainData.size = new Vector3(tileWidth, depth, tileHeight);

                    terrains[index].terrainData.SetHeights(0, 0, GenerateHeights(x,y, tileWidth, tileHeight));
                }
            }
        }
    }

    //private TerrainData GenerateTerrain(TerrainData terrainData)
    //{
    //    terrainData.heightmapResolution = width + 1;
    //    terrainData.size=new Vector3(width,depth,height);

    //    terrainData.SetHeights(0, 0, GenerateHeights());

    //    return terrainData;
    //}

    private float[,] GenerateHeights(int tileX, int tileY, int tileWidth, int tileHeight)
    {
        float[,] totalheights = new float[tileWidth, tileHeight];
        for (int i = 0; i < capas.Length; i++)
        {
            float[,] heights = new float[tileWidth, tileHeight];
            float[,] maskHeights = new float[tileWidth, tileHeight];
            for (int x=0; x < tileWidth; x++)
            {
                for (int y = 0; y < tileHeight; y++)
                {
                    int globalX = tileX * tileWidth + x;
                    int globalY = tileY * tileHeight + y;
                    heights[x, y] = CalculateHeight(globalX, globalY, capas[i].scale,i);
                    if (capas[i].noiseMask.enabled)
                    {
                        maskHeights[x, y] = CalculateHeight(globalX, globalY, capas[i].noiseMask.scale, i);
                    }
                    totalheights[x,y]= LayerOperation(totalheights[x, y], heights[x, y], capas[i], maskHeights[x,y]);
                }
            }
        }

        for (int x = 0; x < tileWidth; x++)
        {
            for (int y = 0; y < tileHeight; y++)
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

    //private float[,] GenerateHeightsForTile(int tileX, int tileY, int tileWidth, int tileHeight)
    //{
    //    float[,] heights = new float[tileWidth, tileHeight];


    //    for (int x = 0; x < tileWidth; x++)
    //    {
    //        for (int y = 0; y < tileHeight; y++)
    //        {
    //            int globalX = tileX * tileWidth + x;
    //            int globalY = tileY * tileHeight + y;
    //            heights[x, y] = CalculateHeight(globalX, globalY);
    //        }
    //    }

    //    return heights;
    //}

    private float CalculateHeight(int x, int y, float scale,int index)
    {
        float xCoord= (float)x/width * scale + offsets[index].x;
        float yCoord= (float)y/height * scale+ offsets[index].y;
        return Mathf.Clamp(Mathf.PerlinNoise(xCoord,yCoord),0,1);
    }

    private float LayerOperation(float totalHeight, float height, NoiseLayer capa,float maskHeight)
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
        if (capa.noiseMask.enabled)
        {
            if (maskHeight > capa.noiseMask.maxCap)
            {
                return totalHeight;
            }
            if (maskHeight < capa.noiseMask.minCap)
            {
                return totalHeight;
            }
        }
        if (height > capa.maxCap)
        {
            if (capa.capType == CapType.cliff)
            {
                return totalHeight;
            }
            if (capa.cut && capa.capSet < totalHeight)
            {
                return totalHeight;
            }
            return capa.capSet;
        }
        if (height < capa.minCap)
        {
            if (capa.capType == CapType.cliff)
            {
               return totalHeight;
            }
            if (capa.cut && capa.capSet < totalHeight)
            {
                return totalHeight;
            }
            return capa.capSet;
        }
        switch (capa.operation)
        {
            case Operation.sum:
                totalH += height * capa.weight;
                break;
            case Operation.additive:
                if(height * capa.weight>totalHeight)
                {
                    return height * capa.weight + capa.heightOffset;
                }
                break;
            case Operation.rest:
                totalH -= height * capa.weight;
                break;
            case Operation.mult:
                return totalHeight * (height * capa.weight + capa.heightOffset);
            case Operation.div:
                return totalHeight / (height * capa.weight + capa.heightOffset);
            case Operation.exp:
                return Mathf.Pow(totalHeight, (height + capa.heightOffset) * capa.weight);
        }
        if (capa.heightOffset != 0)
        {
            totalH += capa.heightOffset;
        }
        if (capa.noiseMask.enabled)
        {
            switch (capa.noiseMask.operation)
            {
                case MaskOperation.mult:
                    totalH *= (maskHeight * capa.noiseMask.weight + capa.noiseMask.heightOffset);
                    break;
                case MaskOperation.div:
                    totalH /= (maskHeight * capa.noiseMask.weight + capa.noiseMask.heightOffset);
                    break;
                case MaskOperation.exp:
                    totalH= Mathf.Pow(totalH, (maskHeight * capa.noiseMask.weight) + capa.noiseMask.heightOffset);
                    break;
            }
        }
        if (capa.cut)
        {
            if (totalH <= totalHeight)
            {
                return totalHeight;
            }
            if (totalH > totalHeight)
            {
                return totalH;
            }
        }
        if (totalHeight + totalH <= minHeightAbs)
        {
            return minHeightAbs;
        }
        return totalHeight + totalH;
    }



    private void ResetOffset()
    {
        offsets = new List<Vector2>();
        maskOffsets = new Vector2[capas.Length];
        for (int i = 0; i < capas.Length; i++)
        {
            offsets.Add(new Vector2(UnityEngine.Random.Range(0, 9999f), UnityEngine.Random.Range(0, 9999f)));
            if (capas[i].noiseMask.enabled)
            {
                maskOffsets[i] = new Vector2(UnityEngine.Random.Range(0, 9999f), UnityEngine.Random.Range(0, 9999f));
            }
        }
    }
}
