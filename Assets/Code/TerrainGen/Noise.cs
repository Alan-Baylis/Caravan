using UnityEngine;
using System.Collections;

public class Noise
{
    public static float[,] GenerateNoiseMap(int seed, int mapSize, Vector2 mapOffset, int octaves, float scale, float persistance, float lacunarity, float redistribution, float baseFloorOffset, float falloffDistanceMultiplier, float falloffDropOffSpeed, float falloffEdgeSlope, AnimationCurve falloffMultiplierCurve)
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

        if (scale <= 0)
            scale = 0.0001f;

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

                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }

        // Normalize noise map (-1 to 1) -> (0 to 1)
        for (int y = 0; y < mapSize; y++)
            for (int x = 0; x < mapSize; x++)
            {
                float normalizedHeight = (noiseMap[x, y] + 1) / maxPossibleHeight;
                noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
            }
        
        // Apply redistribution
        for (int y = 0; y < mapSize; y++)
            for (int x = 0; x < mapSize; x++)
                noiseMap[x, y] = Mathf.Pow(noiseMap[x, y], redistribution);

        AnimationCurve falloffMultiplier = new AnimationCurve(falloffMultiplierCurve.keys);
        for (int y = 0; y < mapSize; y++)
            for (int x = 0; x < mapSize; x++)
            {
                float vertexDistanceFromCenter = Mathf.Pow(mapOffset.x * mapSize + x, 2) + Mathf.Pow(mapOffset.y * mapSize - y, 2);
                float normalizedDistance = Mathf.InverseLerp(0, 10000 * falloffDistanceMultiplier, vertexDistanceFromCenter);

                normalizedDistance = falloffMultiplier.Evaluate(normalizedDistance);

                noiseMap[x, y] = (noiseMap[x, y] + baseFloorOffset) * (1 - falloffEdgeSlope * Mathf.Pow(normalizedDistance, falloffDropOffSpeed));
            }

        return noiseMap;
    }
}
