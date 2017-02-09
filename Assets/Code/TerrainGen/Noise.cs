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

    /// <summary>
    /// Returns a Noise.Data containing a noise map generated from the parameter values, and the paremeters used to generate the data
    /// </summary>
    /// <param name="inSeed"></param>
    /// <param name="inSize"></param>
    /// <param name="inOffset"></param>
    /// <param name="inOctaves"></param>
    /// <param name="inScale"></param>
    /// <param name="inPersistance"></param>
    /// <param name="inLacunarity"></param>
    /// <param name="inRedistribution"></param>
    /// <returns></returns>
    static public Data GenerateNoiseData(int inSeed, int inSize, Vector2 inOffset, int inOctaves, float inScale, float inPersistance, float inLacunarity, float inRedistribution)
    {
        float maxPossibleHeight = 0;
        float amplitude = 1;

        System.Random rng = new System.Random(inSeed);
        Vector2[] octaveOffsets = new Vector2[inOctaves];
        for (int i = 0; i < inOctaves; i++)
        {
            float octaveOffsetX = rng.Next(-100000, 100000) + (inOffset.x * inSize);
            float octaveOffsetY = rng.Next(-100000, 100000) - (inOffset.y * inSize);
            octaveOffsets[i] = new Vector2(octaveOffsetX, octaveOffsetY);

            maxPossibleHeight += amplitude;
            amplitude *= inPersistance;
        }

        inSize += 1;

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float[,] noiseMap = new float[inSize, inSize];

        float halfSize = inSize / 2f;

        for (int y = 0; y < inSize; y++)
            for (int x = 0; x < inSize; x++)
            {
                amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < inOctaves; i++)
                {
                    float sampleX = (x - halfSize + octaveOffsets[i].x) / inScale * frequency;
                    float sampleY = (y - halfSize + octaveOffsets[i].y) / inScale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= inPersistance;
                    frequency *= inLacunarity;
                }

                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }

        // Normalize noise map (-1 to 1) -> (0 to 1)
        for (int y = 0; y < inSize; y++)
            for (int x = 0; x < inSize; x++)
            {
                noiseMap[x, y] += inRedistribution;

                float normalizedHeight = (noiseMap[x, y] + 1) / maxPossibleHeight;
                noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
            }

        

        Data.Parameters usedParameters = new Data.Parameters(inSeed, inSize, inOctaves, inScale, inPersistance, inLacunarity, inRedistribution, inOffset);
        Data newData = new Data(usedParameters, noiseMap);

        return newData;
    }


    /// <summary>
    /// Generates a falloff map by linerarily decreasing the values of the noise map passed through the parameter depending on distance from center
    /// </summary>
    /// <param name="inFalloffNoiseData"></param>
    /// <returns></returns>
    static public Data GenerateFalloffMap(Data inFalloffNoiseData)
    {
        for (int y = 0; y < inFalloffNoiseData.parameters.size; y++)
            for (int x = 0; x < inFalloffNoiseData.parameters.size; x++)
            {
                float vertexDistanceFromCenter = Mathf.Pow(inFalloffNoiseData.parameters.offset.x * inFalloffNoiseData.parameters.size + x, 2) + Mathf.Pow(inFalloffNoiseData.parameters.offset.y * inFalloffNoiseData.parameters.size - y, 2);
                float normalizedDistance = Mathf.InverseLerp(0, 10000 * 100, vertexDistanceFromCenter);

                inFalloffNoiseData.noise[x,y] += normalizedDistance;
            }

        return inFalloffNoiseData;
    }


    /// <summary>
    /// Applies falloff map to the noise map passed through the parameter using the falloff map passed through the parameter
    /// </summary>
    /// <param name="noiseData"></param>
    /// <param name="falloffData"></param>
    /// <returns></returns>
    static public Data ApplyFalloffMap(Data noiseData, Data falloffData)
    {
        for (int y = 0; y < noiseData.parameters.size; y++)
            for (int x = 0; x < noiseData.parameters.size; x++)
                noiseData.noise[x,y] -= falloffData.noise[x, y];

        return noiseData;
    }
}
