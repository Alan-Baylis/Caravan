using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class Chunk
{
    // Member variabless
    private World _world;

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

    private NoiseData _noiseData;
    public NoiseData noiseData
    {
        set
        {
            _noiseData = value;
            GenerateTowns();
        }
    }


    // Constructor
    public Chunk(Vector2 inChunkCoords, GameObject inGameObject, World inWorld)
    {
        _gameObject = inGameObject;
        _coords = inChunkCoords;
        _world = inWorld;

    }


    // External
    public float GetTileHeight(int xCoord, int yCoord)
    {
        return _noiseData.heightMap.noise[xCoord, yCoord]; // WARNING: Do mind this is temporary since biomes will be implemented
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
