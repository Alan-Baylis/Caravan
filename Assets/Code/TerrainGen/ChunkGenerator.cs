using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class ChunkGenerator : MonoBehaviour 
{
    /* Member Variables */
    private World _world;

    private BiomeManager _biomeManager;
    public BiomeManager biomeManager
    {
        get { return _biomeManager; }
    }


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
        _biomeManager = GetComponent<BiomeManager>();
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
    private void RequestNoiseData(Vector2 inChunkCoords, Chunk inChunk)
    {
        NoiseData noiseData = new NoiseData();
        Action callbackMethod = new Action(() => OnNoiseDataReceived(noiseData, inChunk));

        ThreadStart threadStart = delegate { noiseData.Generate(callbackMethod, inChunkCoords, _world.worldGenData, this); };

        new Thread(threadStart).Start();
    }

    private void RequestMeshData(NoiseData inNoiseData, Chunk inChunk)
    {
        MeshData meshData = new MeshData(_world.worldGenData.chunkSize);
        Action callbackMethod = new Action(() => OnMeshDataReceived(meshData, inChunk));

        ThreadStart threadStart = delegate { meshData.Generate(callbackMethod, inNoiseData, _world.worldGenData, this); };

        new Thread(threadStart).Start();
    }

    private void RequestTexture(NoiseData inNoiseData, Chunk inChunk)
    {
        TextureData textureData = new TextureData();
        Action callbackMethod = new Action(() => OnTextureReceived(textureData, inChunk));

        ThreadStart threadStart = delegate { textureData.Generate(callbackMethod, inNoiseData, _world.worldGenData, this); };

        new Thread(threadStart).Start();
    }


    // Callback methods
    private void OnNoiseDataReceived(NoiseData inNoiseData, Chunk inChunk)
    {
        inChunk.noiseData = inNoiseData;

        RequestMeshData(inNoiseData, inChunk);

        RequestTexture(inNoiseData, inChunk);
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

        int chunkSize = _world.worldGenData.chunkSize;

        Texture2D texture = new Texture2D(chunkSize, chunkSize);
        texture.filterMode = FilterMode.Trilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        int colorMapLength = inTextureData.colorMap.Length;
        
        texture.SetPixels(inTextureData.colorMap);
        texture.Apply();

        inChunk.gameObject.GetComponent<MeshRenderer>().material.mainTexture = texture;
    }


    /* External Methods */
    public Chunk GenerateChunk(Vector2 inChunkCoords)
    {
        Chunk newChunk = new Chunk(inChunkCoords, GenerateGO(inChunkCoords), _world);

        RequestNoiseData(inChunkCoords, newChunk);

        return newChunk;
    }

    public void RegenerateChunk(Chunk inChunkToRegenerate)
    {
        RequestNoiseData(inChunkToRegenerate.coords, inChunkToRegenerate);
    }
}

public class NoiseData
{
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


    public NoiseData(){}


    public void Generate(Action callback, Vector2 inChunkCoords, World.WorldGenData inWorldGenData, ChunkGenerator inChunkGenerator)
    {
        _tempMap = Noise.GenerateNoiseData
        (
            inWorldGenData.tempSeed,
            inWorldGenData.chunkSize,
            inChunkCoords,
            inWorldGenData.tempOctaves,
            inWorldGenData.tempScale,
            inWorldGenData.tempPersistance,
            inWorldGenData.tempLacunarity,
            inWorldGenData.tempRedistribution
        );

        _humidMap = Noise.GenerateNoiseData
        (
            inWorldGenData.humidSeed,
            inWorldGenData.chunkSize,
            inChunkCoords,
            inWorldGenData.humidOctaves,
            inWorldGenData.humidScale,
            inWorldGenData.humidPersistance,
            inWorldGenData.humidLacunarity,
            inWorldGenData.humidRedistribution
        );

        _heightMap = Noise.GenerateNoiseData
        (
            inWorldGenData.heightSeed,
            inWorldGenData.chunkSize,
            inChunkCoords,
            inWorldGenData.heightOctaves,
            inWorldGenData.heightScale,
            inWorldGenData.heightPersistance,
            inWorldGenData.heightLacunarity,
            inWorldGenData.heightRedistribution
        );

        _falloffMap = Noise.GenerateNoiseData
        (
            inWorldGenData.falloffSeed,
            inWorldGenData.chunkSize,
            inChunkCoords,
            inWorldGenData.falloffOctaves,
            inWorldGenData.falloffScale,
            inWorldGenData.falloffPersistance,
            inWorldGenData.falloffLacunarity,
            inWorldGenData.falloffRedistribution
        );

        _falloffMap = Noise.GenerateFalloffMap(_falloffMap);

        _heightMap = Noise.ApplyFalloffMap(_heightMap, _falloffMap);

        lock (inChunkGenerator.noiseDataThreadInfoQueue)
            inChunkGenerator.noiseDataThreadInfoQueue.Enqueue(callback);
    }
}

public class MeshData
{
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

    
    public MeshData(int inMeshSize)
    {
        meshSize = inMeshSize;

        tileCount = meshSize * meshSize;
        triangleCount = tileCount * 2;

        vertexSize = meshSize + 1;
        vertexCount = vertexSize * vertexSize;

        _vertexCoords = new Vector3[vertexCount];
        _normals = new Vector3[vertexCount];
        _UVCoords = new Vector2[vertexCount];

        _triVertIDs = new int[triangleCount * 3];
    }


    public void Generate(Action callback, NoiseData inNoiseData, World.WorldGenData inWorldGenData, ChunkGenerator inChunkGenerator)
    {
        AnimationCurve heightMultiplierCurve = new AnimationCurve(inWorldGenData.heightMultiplierCurve.keys);
        float meshHeightMultiplier = inWorldGenData.meshHeightMultiplier;
        float[,] heightMap = inNoiseData.heightMap.noise;
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

        lock (inChunkGenerator.meshDataThreadInfoQueue)
            inChunkGenerator.meshDataThreadInfoQueue.Enqueue(callback);
    }
}

public class TextureData
{
    private Color[] _colorMap;
    public Color[] colorMap
    {
        get { return _colorMap; }
    }


    public TextureData(){}


    public void Generate(Action callback, NoiseData inNoiseData, World.WorldGenData inWorldGenData, ChunkGenerator inChunkGenerator)
    {
        int textureSize = inWorldGenData.chunkSize;
        float[,] heightMap = inNoiseData.heightMap.noise;
        BiomeManager biomeManager = inChunkGenerator.biomeManager;

        // Calculate colormap
        _colorMap = new Color[textureSize * textureSize];
        for (int y = 0; y < textureSize; y++)
            for (int x = 0; x < textureSize; x++)
                _colorMap[y * textureSize + x] = biomeManager.GetBiome(heightMap[x, y]).color;

        lock (inChunkGenerator.textureDataThreadInfoQueue)
            inChunkGenerator.textureDataThreadInfoQueue.Enqueue(callback);
    }
}
