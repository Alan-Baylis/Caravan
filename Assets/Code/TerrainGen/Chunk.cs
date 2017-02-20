using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Chunk
{
    // Member variabless
    private World _world;
    public World world
    {
        get { return _world; }
    }

    private GameObject _gameObject;
    public GameObject gameObject
    {
        get { return _gameObject; }
    }

    private Vector2 _coords;
    public Vector2 coords
    {
        get { return _coords; }
    }

    [System.Serializable]
    public struct ChunkData
    {
        public NoiseData noiseData;
        public MeshData meshData;
        public TextureData textureData;

        public bool IsComplete()
        {
            if (noiseData != null && meshData != null && textureData != null)
                return true;

            return false;
        }
    }
    private ChunkData _chunkData;
    public ChunkData chunkData
    {
        get { return _chunkData; }
        set { _chunkData = value; }
    }


    // Constructor
    public Chunk(Vector2 inChunkCoords, GameObject inGameObject, World inWorld)
    {
        _gameObject = inGameObject;
        _coords = inChunkCoords;
        _world = inWorld;
    }

    public Chunk(Vector2 inChunkCoords, GameObject inGameObject, World inWorld, ChunkData inChunkData)
    {
        _gameObject = inGameObject;
        _coords = inChunkCoords;
        _world = inWorld;
        _chunkData = inChunkData;
    }

    // External
    public float GetTileHeight(int xCoord, int yCoord)
    {
        return _chunkData.noiseData.heightMap.noise[xCoord, yCoord]; // WARNING: Do mind this is temporary since biomes will be implemented
    }

    // Internal
    private void GenerateTowns()
    {
        int townGenerationTries = Random.Range(0, 5);  // TODO - In order to make worlds generate identically every time, a global seed value is probably needed. So yeah, implement global seed value
        int chunkSize = _world.worldGenData.chunkSize;
        
        for (int i = 0; i < townGenerationTries; i++)
        {
            Vector2 newTownPosition = new Vector2(Random.Range(0, chunkSize), Random.Range(0, chunkSize));
            float targetTileHeight = GetTileHeight((int)newTownPosition.x, (int)newTownPosition.y);

            Vector3 newTownWorldSpacePosition = new Vector3(newTownPosition.x + (chunkSize * coords.x), (targetTileHeight * _world.worldGenData.heightMultiplierCurve.Evaluate(targetTileHeight) * _world.worldGenData.meshHeightMultiplier) + 4, newTownPosition.y - (chunkSize * coords.y));

            if (targetTileHeight > 0.2f && targetTileHeight < 0.4f)
                _world.GenerateTown(newTownWorldSpacePosition);
        }
    }
}

public enum Biome
{
    Water_Deep,
    Water_Shallow,
    Beach,
    Forest,
    Plains,
    Hills,
    Mountain,
    Mountain_Snowy
}
