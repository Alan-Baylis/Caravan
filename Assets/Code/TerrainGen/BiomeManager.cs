using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeManager : MonoBehaviour
{
    public Biome[] _biomes;

    public Biome GetBiome(float height /*, float temperature, float humidity */)
    {
        if (height <= 0.01f) return _biomes[(int)BiomeType.Water_Deep];
        if (height <= 0.056f) return _biomes[(int)BiomeType.Water_Shallow];

        if (height > 0.771f) return _biomes[(int)BiomeType.Mountains_Snowy];
        if (height > 0.412f) return _biomes[(int)BiomeType.Mountains];
        if (height > 0.353f) return _biomes[(int)BiomeType.Hills];
        if (height > 0.200f) return _biomes[(int)BiomeType.Plains];
        if (height > 0.082f)  return _biomes[(int)BiomeType.Forest];
        if (height > 0.056f)  return _biomes[(int)BiomeType.Beach];

        else
        {
            Debug.Log("ERROR: " + height + " failed to generate a biome. Check the BiomeManager.GetBiome()!");
            return _biomes[(int)BiomeType.Debug];
        }

    }
}

[System.Serializable]
public class Biome
{
    public BiomeType biomeType;
    public Color color;
}

public enum BiomeType
{
    Debug,
    Water_Deep,
    Water_Shallow,
    Beach,
    Forest,
    Plains,
    Hills,
    Mountains,
    Mountains_Snowy
}

