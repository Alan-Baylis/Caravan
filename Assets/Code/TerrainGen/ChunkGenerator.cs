using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class ChunkGenerator : MonoBehaviour 
{
    /* Member Variables */
    private World _world;

    private Queue<Action> _noiseDataThreadInfoQueue = new Queue<Action>();
    public Queue<Action> noiseDataThreadInfoQueue
    {
        get { return _noiseDataThreadInfoQueue; }

        set { _noiseDataThreadInfoQueue = value; }
    }

    private Queue<Action> _meshDataThreadInfoQueue = new Queue<Action>();
    public Queue<Action> meshDataThreadInfoQueue
    {
        get { return _meshDataThreadInfoQueue; }

        set { _meshDataThreadInfoQueue = value; }
    }

    private Queue<Action> _textureDataThreadInfoQueue = new Queue<Action>();
    public Queue<Action> textureDataThreadInfoQueue
    {
        get { return _textureDataThreadInfoQueue; }

        set { _textureDataThreadInfoQueue = value; }
    }


    /* Start, Update */
    void Start()
    {
        _world = GetComponent<World>();
    }

    void Update()
    {
        ProcessQueues();
    }


    /* Internal Methods */
    private void ProcessQueues()
    {
        if (_noiseDataThreadInfoQueue.Count > 0)
            for (int i = 0; i < _noiseDataThreadInfoQueue.Count; i++)
                _noiseDataThreadInfoQueue.Dequeue()();

        if (_meshDataThreadInfoQueue.Count > 0)
            for (int i = 0; i < _meshDataThreadInfoQueue.Count; i++)
                _meshDataThreadInfoQueue.Dequeue()();

        if (textureDataThreadInfoQueue.Count > 0)
            for (int i = 0; i < textureDataThreadInfoQueue.Count; i++)
                _textureDataThreadInfoQueue.Dequeue()();
    }

    private GameObject GenerateGO(Vector2 inChunkCoords)
    {
        GameObject newGO = new GameObject(inChunkCoords.x + "," + inChunkCoords.y);
        newGO.AddComponent<MeshFilter>();
        newGO.AddComponent<MeshRenderer>();
        newGO.transform.SetParent(transform);
        newGO.transform.position = new Vector3(inChunkCoords.x * _world.worldGenData.chunkSize, 0, -inChunkCoords.y * _world.worldGenData.chunkSize);
        newGO.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0.25f);
        return newGO;
    }


    // Request methods
    private void RequestNoiseData(Vector2 inChunkCoords, World.WorldGenData inWorldGenData, Chunk inChunk)
    {
        NoiseData noiseData = new NoiseData(inChunkCoords, inWorldGenData, this);
        Action callbackMethod = new Action(() => OnNoiseDataReceived(noiseData, inChunk));

        ThreadStart threadStart = delegate { noiseData.Generate(callbackMethod); };

        new Thread(threadStart).Start();
    }

    private void RequestMeshData(NoiseData inNoiseData, Chunk inChunk)
    {
        MeshData meshData = new MeshData(inNoiseData);
        Action callbackMethod = new Action(() => OnMeshDataReceived(meshData, inChunk));

        ThreadStart threadStart = delegate { meshData.Generate(callbackMethod); };

        new Thread(threadStart).Start();
    }

    private void RequestTexture(NoiseData inNoiseData, Chunk inChunk)
    {
        TextureData textureData = new TextureData(inNoiseData);
        Action callbackMethod = new Action(() => OnTextureReceived(textureData, inChunk));

        ThreadStart threadStart = delegate { textureData.Generate(callbackMethod); };

        new Thread(threadStart).Start();
    }


    // Callback methods
    private void OnNoiseDataReceived(NoiseData inNoiseData, Chunk inChunk)
    {
        RequestMeshData(inNoiseData, inChunk);

        RequestTexture(inNoiseData, inChunk);

        inChunk.noiseData = inNoiseData;
    }

    private void OnMeshDataReceived(MeshData inMeshData, Chunk inChunk)
    {
        if (inChunk.gameObject == null)
            return;

        Mesh newMesh = new Mesh();
        newMesh.name = "Terrain Mesh";
        newMesh.vertices = inMeshData.vertexCoords;
        newMesh.normals = inMeshData.normals;
        newMesh.uv = inMeshData.UVCoords;
        newMesh.triangles = inMeshData.triVertIDs;

        newMesh.RecalculateNormals();

        inChunk.gameObject.GetComponent<MeshFilter>().mesh = newMesh;
    }

    private void OnTextureReceived(TextureData inTextureData, Chunk inChunk)
    {
        if (inChunk.gameObject == null)
            return;

        int chunkSize = inTextureData.noiseData.worldGenData.chunkSize;

        Texture2D texture = new Texture2D(chunkSize, chunkSize);
        texture.filterMode = FilterMode.Trilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(inTextureData.colorMap);
        texture.Apply();

        inChunk.gameObject.GetComponent<MeshRenderer>().material.mainTexture = texture;
    }
    

    /* External Methods */
    public Chunk GenerateChunk(Vector2 inChunkCoords)
    {
        Chunk newChunk = new Chunk(inChunkCoords, GenerateGO(inChunkCoords));

        RequestNoiseData(inChunkCoords, _world.worldGenData, newChunk);

        return newChunk;
    }

    public void RegenerateChunk(Chunk inChunkToRegenerate)
    {
        RequestNoiseData(inChunkToRegenerate.coords, _world.worldGenData, inChunkToRegenerate);
    }
}

public class NoiseData
{
    public readonly Vector2 chunkCoords;
    public readonly World.WorldGenData worldGenData;
    public readonly ChunkGenerator chunkGenerator;

    private Noise.Data _tempMap;
    public  Noise.Data tempMap
    {
        get { return _tempMap; }
    }

    private Noise.Data _humidMap;
    public  Noise.Data humidMap
    {
        get { return _humidMap; }
    }

    private Noise.Data _heightMap;
    public  Noise.Data heightMap
    {
        get { return _heightMap; }
    }

    private Noise.Data _falloffMap;
    public Noise.Data falloffMap
    {
        get { return _falloffMap; }
    }

    // Constructor
    public NoiseData(Vector2 inChunkCoords, World.WorldGenData inWorldGenData, ChunkGenerator inChunkGenerator)
    {
        chunkCoords = inChunkCoords;
        worldGenData = inWorldGenData;
        chunkGenerator = inChunkGenerator;
    }
       
    // External methods
    public void Generate(Action callback)
    {
        _tempMap = Noise.GenerateNoiseData
        (
            worldGenData.tempSeed,
            worldGenData.chunkSize,
            chunkCoords,
            worldGenData.tempOctaves,
            worldGenData.tempScale,
            worldGenData.tempPersistance,
            worldGenData.tempLacunarity,
            worldGenData.tempRedistribution,
            worldGenData.heightBaseFloorOffset,
            worldGenData.heightFalloffDistanceMultiplier,
            worldGenData.heightFalloffDropOffSpeed,
            worldGenData.heightFallOffEdgeSlope,
            worldGenData.heightFalloffMultiplierCurve
        );

        _humidMap = Noise.GenerateNoiseData
        (
            worldGenData.humidSeed,
            worldGenData.chunkSize,
            chunkCoords,
            worldGenData.humidOctaves,
            worldGenData.humidScale,
            worldGenData.humidPersistance,
            worldGenData.humidLacunarity,
            worldGenData.humidRedistribution,
            worldGenData.heightBaseFloorOffset,
            worldGenData.heightFalloffDistanceMultiplier,
            worldGenData.heightFalloffDropOffSpeed,
            worldGenData.heightFallOffEdgeSlope,
            worldGenData.heightFalloffMultiplierCurve
        );

        _heightMap = Noise.GenerateNoiseData
        (
            worldGenData.heightSeed,
            worldGenData.chunkSize,
            chunkCoords,
            worldGenData.heightOctaves,
            worldGenData.heightScale,
            worldGenData.heightPersistance,
            worldGenData.heightLacunarity,
            worldGenData.heightRedistribution,
            worldGenData.heightBaseFloorOffset,
            worldGenData.heightFalloffDistanceMultiplier,
            worldGenData.heightFalloffDropOffSpeed,
            worldGenData.heightFallOffEdgeSlope,
            worldGenData.heightFalloffMultiplierCurve
        );

        _falloffMap = Noise.GenerateNoiseData
        (
            worldGenData.falloffSeed,
            worldGenData.chunkSize,
            chunkCoords,
            worldGenData.falloffOctaves,
            worldGenData.falloffScale,
            worldGenData.falloffPersistance,
            worldGenData.falloffLacunarity,
            worldGenData.falloffRedistribution,
            worldGenData.heightBaseFloorOffset,
            worldGenData.heightFalloffDistanceMultiplier,
            worldGenData.heightFalloffDropOffSpeed,
            worldGenData.heightFallOffEdgeSlope,
            worldGenData.heightFalloffMultiplierCurve
        );

        lock (chunkGenerator.noiseDataThreadInfoQueue)
            chunkGenerator.noiseDataThreadInfoQueue.Enqueue(callback);
    }
}

public class MeshData
{
    // Member variables
    public readonly NoiseData noiseData;
    public readonly int meshSize;
    public readonly int tileCount;
    public readonly int triangleCount;
    public readonly int vertexSize;
    public readonly int vertexCount;

    private Vector3[] _vertexCoords;
    public Vector3[] vertexCoords
    {
        get { return _vertexCoords; }
    }

    private Vector3[] _normals;
    public Vector3[] normals
    {
        get { return _normals; }
    }

    private Vector2[] _UVCoords;
    public Vector2[] UVCoords
    {
        get { return _UVCoords; }
    }

    private int[] _triVertIDs;
    public int[] triVertIDs
    {
        get { return _triVertIDs; }
    }

    // Constructor
    public MeshData(NoiseData inNoiseData)
    {
        noiseData = inNoiseData;

        meshSize = inNoiseData.worldGenData.chunkSize;

        tileCount = meshSize * meshSize;
        triangleCount = tileCount * 2;

        vertexSize = meshSize + 1;
        vertexCount = vertexSize * vertexSize;

        _vertexCoords = new Vector3[vertexCount];
        _normals = new Vector3[vertexCount];
        _UVCoords = new Vector2[vertexCount];

        _triVertIDs = new int[triangleCount * 3];
    }

    // External methods
    public void Generate(Action callback)
    {
        AnimationCurve heightMultiplierCurve = new AnimationCurve(noiseData.worldGenData.heightMultiplierCurve.keys);
        float meshHeightMultiplier = noiseData.worldGenData.meshHeightMultiplier;
        float[,] heightMap = noiseData.heightMap.noise;
        Vector3 upVector = Vector3.up;

        for (int y = 0; y < vertexSize; y++)
            for (int x = 0; x < vertexSize; x++)
            {
                _vertexCoords[y * vertexSize + x] = new Vector3(x, heightMultiplierCurve.Evaluate(heightMap[x, y]) * meshHeightMultiplier, y);
                _normals[y * vertexSize + x] = upVector;
                _UVCoords[y * vertexSize + x] = new Vector2((float)x / meshSize, (float)y / meshSize);
            }

		bool diagonal = false;
        for (int y = 0; y < meshSize; y++)
        {
            for (int x = 0; x < meshSize; x++)
            {
                int currentTileID = y * meshSize + x;

                int triVertOffset = y * vertexSize + x;

                int triangleOffset = currentTileID * 6;

                if (diagonal)
                {
                    _triVertIDs[triangleOffset + 0] = triVertOffset + 0;
                    _triVertIDs[triangleOffset + 1] = triVertOffset + vertexSize + 0;
                    _triVertIDs[triangleOffset + 2] = triVertOffset + vertexSize + 1;

                    _triVertIDs[triangleOffset + 3] = triVertOffset + 0;
                    _triVertIDs[triangleOffset + 4] = triVertOffset + vertexSize + 1;
                    _triVertIDs[triangleOffset + 5] = triVertOffset + 1;
                }

                else
                {
                    _triVertIDs[triangleOffset + 0] = triVertOffset + 0;
                    _triVertIDs[triangleOffset + 1] = triVertOffset + vertexSize + 0;
                    _triVertIDs[triangleOffset + 2] = triVertOffset + 1;

                    _triVertIDs[triangleOffset + 3] = triVertOffset + 1;
                    _triVertIDs[triangleOffset + 4] = triVertOffset + vertexSize + 0;
                    _triVertIDs[triangleOffset + 5] = triVertOffset + vertexSize + 1;
                }

                diagonal = !diagonal;

            }

            diagonal = !diagonal;
        }

        lock (noiseData.chunkGenerator.meshDataThreadInfoQueue)
            noiseData.chunkGenerator.meshDataThreadInfoQueue.Enqueue(callback);
    }
}

public class TextureData
{
    // Member variables
    public readonly NoiseData noiseData;

    private Color[] _colorMap;
    public Color[] colorMap
    {
        get { return _colorMap; }
    }

    // Constructor
    public TextureData(NoiseData inNoiseData)
    {
        noiseData = inNoiseData;
    }

    // External methods
    public void Generate(Action callback)
    {
        int textureSize = noiseData.worldGenData.chunkSize;
        Gradient tempTerrainGradient = noiseData.worldGenData.tempTerrainGradient;
        float[,] heightMap = noiseData.heightMap.noise;

        // Calculate colormap
        _colorMap = new Color[textureSize * textureSize];
        for (int y = 0; y < textureSize; y++)
            for (int x = 0; x < textureSize; x++)
                _colorMap[y * textureSize + x] = tempTerrainGradient.Evaluate(heightMap[x, y]);

        lock (noiseData.chunkGenerator.textureDataThreadInfoQueue)
            noiseData.chunkGenerator.textureDataThreadInfoQueue.Enqueue(callback);
    }
}