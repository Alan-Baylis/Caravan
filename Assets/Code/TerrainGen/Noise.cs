using UnityEngine;
using System.Collections;

public class Noise
{
    public struct Data
    {
        public float[,] noise;

        public Parameters parameters;
        public struct Parameters
        {
            public int seed;
            public int size;
            public int octaves;
            public float scale;
            public float persistance;
            public float lacunarity;
            public float redistribution;
            public Vector2 offset;

            public Parameters(int inSeed, int inSize, int inOctaves, float inScale, float inPersistance, float inLacunarity, float inRedistribution, Vector2 inOffset)
            {
                seed = inSeed;
                size = inSize;
                octaves = inOctaves;
                scale = inScale;
                persistance = inPersistance;
                lacunarity = inLacunarity;
                redistribution = inRedistribution;
                offset = inOffset;
            }
        }

        public Data(Parameters inParameters, float[,] inNoise)
        {
            parameters = inParameters;
            noise = inNoise;
        }
    }

    static public Data GenerateNoiseData(int seed, int mapSize, Vector2 mapOffset, int octaves, float scale, float persistance, float lacunarity, float redistribution, float baseFloorOffset, float falloffDistanceMultiplier, float falloffDropOffSpeed, float falloffEdgeSlope, AnimationCurve falloffMultiplierCurve)
    {
        float maxPossibleHeight = 0;
        float amplitude = 1;

        System.Random rng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float octaveOffsetX = rng.Next(-100000, 100000) + (mapOffset.x * mapSize);
            float octaveOffsetY = rng.Next(-100000, 100000) - (mapOffset.y * mapSize);
            octaveOffsets[i] = new Vector2(octaveOffsetX, octaveOffsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        mapSize += 1;

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float[,] noiseMap = new float[mapSize, mapSize];

        float halfSize = mapSize / 2f;

        for (int y = 0; y < mapSize; y++)
            for (int x = 0; x < mapSize; x++)
            {
                amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfSize + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfSize + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }

        // Normalize noise map (-1 to 1) -> (0 to 1)
        for (int y = 0; y < mapSize; y++)
            for (int x = 0; x < mapSize; x++)
            {
                float normalizedHeight = (noiseMap[x, y] + 1) / maxPossibleHeight;
                noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
            }

        Data.Parameters usedParameters = new Data.Parameters(seed, mapSize, octaves, scale, persistance, lacunarity, redistribution, mapOffset);
        Data newData = new Data(usedParameters, noiseMap);

        return newData;
    }

    static public Data GenerateFalloffMap(Data inFalloffNoiseData)
    {
        for (int y = 0; y < inFalloffNoiseData.parameters.size; y++)
            for (int x = 0; x < inFalloffNoiseData.parameters.size; x++)
            {
                float vertexDistanceFromCenter = Mathf.Pow(inFalloffNoiseData.parameters.offset.x * inFalloffNoiseData.parameters.size + x, 2) + Mathf.Pow(inFalloffNoiseData.parameters.offset.y * inFalloffNoiseData.parameters.size - y, 2);
                float normalizedDistance = Mathf.InverseLerp(0, 10000 * 4, vertexDistanceFromCenter);

                inFalloffNoiseData.noise[x,y] += normalizedDistance;
            }

        return inFalloffNoiseData;
    }

    static public Data ApplyFalloffMap(Data noiseData, Data falloffData)
    {
        for (int y = 0; y < noiseData.parameters.size; y++)
            for (int x = 0; x < noiseData.parameters.size; x++)
            {
                noiseData.noise[x,y] -= falloffData.noise[x, y];
            }

        return noiseData;
    }
}


