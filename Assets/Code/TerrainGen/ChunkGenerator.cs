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
    private void RequestNoiseData(Vector2 inChunkCoords, Chunk inChunk)
    {
        NoiseData noiseData = new NoiseData();
        Action callbackMethod = new Action(() => OnNoiseDataReceived(noiseData, inChunk));

        ThreadStart threadStart = delegate { noiseData.Generate(callbackMethod, inChunkCoords, _world.worldGenData, this); };

        new Thread(threadStart).Start();
    }

    private void RequestMeshData(NoiseData inNoiseData, Chunk inChunk)
    {
        MeshData meshData = new MeshData(inChunk.world.worldGenData.chunkSize);
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
        Chunk.ChunkData newChunkData = inChunk.chunkData;
        newChunkData.noiseData = inNoiseData;
        inChunk.chunkData = newChunkData;

        RequestMeshData(inNoiseData, inChunk);

        RequestTexture(inNoiseData, inChunk);
    }

    private void OnMeshDataReceived(MeshData inMeshData, Chunk inChunk)
    {
        if (inChunk.gameObject == null)
            return;

        Mesh newMesh = new Mesh();
        newMesh.name = "Terrain Mesh";

        List<Vector3> newVertices = new List<Vector3>();
        List<Vector3> newNormals = new List<Vector3>();
        List<Vector2> newUVs = new List<Vector2>();

        for (int i = 0; i < inMeshData.vertexCoords.Length; i++) newVertices.Add(inMeshData.vertexCoords[i]);
        for (int i = 0; i < inMeshData.normals.Length; i++) newNormals.Add(inMeshData.normals[i]);
        for (int i = 0; i < inMeshData.UVCoords.Length; i++) newUVs.Add(inMeshData.UVCoords[i]);

        newMesh.SetVertices(newVertices);
        newMesh.SetNormals(newNormals);
        newMesh.uv = newUVs.ToArray();

        newMesh.triangles = inMeshData.triVertIDs;

        newMesh.RecalculateNormals();

        inChunk.gameObject.GetComponent<MeshFilter>().mesh = newMesh;

        Chunk.ChunkData newChunkData = inChunk.chunkData;
        newChunkData.meshData = inMeshData;
        inChunk.chunkData = newChunkData;
    }

    private void OnTextureReceived(TextureData inTextureData, Chunk inChunk)
    {
        if (inChunk.gameObject == null)
            return;

        int chunkSize = inChunk.world.worldGenData.chunkSize;

        Texture2D texture = new Texture2D(chunkSize, chunkSize);
        texture.filterMode = FilterMode.Trilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color[] colorMap = new Color[inTextureData.colorMap.Length];
        for (int i = 0; i < inTextureData.colorMap.Length; i++) colorMap[i] = inTextureData.colorMap[i];
        texture.SetPixels(colorMap);
        texture.Apply();

        inChunk.gameObject.GetComponent<MeshRenderer>().material.mainTexture = texture;

        Chunk.ChunkData newChunkData = inChunk.chunkData;
        newChunkData.textureData = inTextureData;
        inChunk.chunkData = newChunkData;
    }
    

    /* External Methods */
    public Chunk GenerateChunk(Vector2 inChunkCoords)
    {
        Chunk newChunk;
        if (File.Exists(Application.dataPath + @"\..\Chunks\" + inChunkCoords.x.ToString() + "." + inChunkCoords.y.ToString() + ".dat"))
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            byte[] chunkBytesReadFromDisk = File.ReadAllBytes(Application.dataPath + @"\..\Chunks\" + inChunkCoords.x.ToString() + "." + inChunkCoords.y.ToString() + ".dat");

            memoryStream.Write(chunkBytesReadFromDisk, 0, chunkBytesReadFromDisk.Length);
            memoryStream.Position = 0;

            Chunk.ChunkData loadedChunkData = (Chunk.ChunkData)binaryFormatter.Deserialize(memoryStream);

            newChunk = new Chunk(inChunkCoords, GenerateGO(inChunkCoords), _world, loadedChunkData);

            OnNoiseDataReceived(loadedChunkData.noiseData, newChunk);
            OnMeshDataReceived(loadedChunkData.meshData, newChunk);
            OnTextureReceived(loadedChunkData.textureData, newChunk);
        }

        else
        {
            newChunk = new Chunk(inChunkCoords, GenerateGO(inChunkCoords), _world);

            RequestNoiseData(inChunkCoords, newChunk);
        }

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

    // Constructor
    public NoiseData(){}
       
    // External methods
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

    public void WriteToStream(MemoryStream stream)
    {
        BinaryWriter writer = new BinaryWriter(stream);

        int tempMapSize = _tempMap.parameters.size;
        for (int y = 0; y < tempMapSize; y++)
            for (int x = 0; x < tempMapSize; x++)
                writer.Write(_tempMap.noise[x, y]);

        int humidMapSize = _humidMap.parameters.size;
        for (int y = 0; y < humidMapSize; y++)
            for (int x = 0; x < humidMapSize; x++)
                writer.Write(_humidMap.noise[x, y]);            

        int heightMapSize = _heightMap.parameters.size;
        for (int y = 0; y < heightMapSize; y++)
            for (int x = 0; x < heightMapSize; x++)
                writer.Write(_heightMap.noise[x, y]);

        int falloffMapSize = _falloffMap.parameters.size;
        for (int y = 0; y < falloffMapSize; y++)
            for (int x = 0; x < falloffMapSize; x++)
                writer.Write(_falloffMap.noise[x, y]);


        writer.Write(_tempMap.parameters.seed);
        writer.Write(_tempMap.parameters.size);
        writer.Write(_tempMap.parameters.octaves);
        writer.Write(_tempMap.parameters.scale);
        writer.Write(_tempMap.parameters.persistance);
        writer.Write(_tempMap.parameters.lacunarity);
        writer.Write(_tempMap.parameters.redistribution);
        writer.Write(_tempMap.parameters.offsetX);
        writer.Write(_tempMap.parameters.offsetY);

        writer.Write(_humidMap.parameters.seed);
        writer.Write(_humidMap.parameters.size);
        writer.Write(_humidMap.parameters.octaves);
        writer.Write(_humidMap.parameters.scale);
        writer.Write(_humidMap.parameters.persistance);
        writer.Write(_humidMap.parameters.lacunarity);
        writer.Write(_humidMap.parameters.redistribution);
        writer.Write(_humidMap.parameters.offsetX);
        writer.Write(_humidMap.parameters.offsetY);

        writer.Write(_heightMap.parameters.seed);
        writer.Write(_heightMap.parameters.size);
        writer.Write(_heightMap.parameters.octaves);
        writer.Write(_heightMap.parameters.scale);
        writer.Write(_heightMap.parameters.persistance);
        writer.Write(_heightMap.parameters.lacunarity);
        writer.Write(_heightMap.parameters.redistribution);
        writer.Write(_heightMap.parameters.offsetX);
        writer.Write(_heightMap.parameters.offsetY);

        writer.Write(_falloffMap.parameters.seed);
        writer.Write(_falloffMap.parameters.size);
        writer.Write(_falloffMap.parameters.octaves);
        writer.Write(_falloffMap.parameters.scale);
        writer.Write(_falloffMap.parameters.persistance);
        writer.Write(_falloffMap.parameters.lacunarity);
        writer.Write(_falloffMap.parameters.redistribution);
        writer.Write(_falloffMap.parameters.offsetX);
        writer.Write(_falloffMap.parameters.offsetY);
    }
}

public class MeshData
{
    // Member variables
    public readonly int meshSize;
    public readonly int tileCount;
    public readonly int triangleCount;
    public readonly int vertexSize;
    public readonly int vertexCount;

    private SAbleVector3[] _vertexCoords;
    public SAbleVector3[] vertexCoords
    {
        get { return _vertexCoords; }
    }

    private SAbleVector3[] _normals;
    public SAbleVector3[] normals
    {
        get { return _normals; }
    }

    private SAbleVector2[] _UVCoords;
    public SAbleVector2[] UVCoords
    {
        get { return _UVCoords; }
    }

    private int[] _triVertIDs;
    public int[] triVertIDs
    {
        get { return _triVertIDs; }
    }

    // Constructor
    public MeshData(int inMeshSize)
    {
        meshSize = inMeshSize;

        tileCount = meshSize * meshSize;
        triangleCount = tileCount * 2;

        vertexSize = meshSize + 1;
        vertexCount = vertexSize * vertexSize;

        _vertexCoords = new SAbleVector3[vertexCount];
        _normals = new SAbleVector3[vertexCount];
        _UVCoords = new SAbleVector2[vertexCount];

        _triVertIDs = new int[triangleCount * 3];
    }

    // External methods
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

    public void WriteToStream(Stream stream)
    {
        BinaryWriter writer = new BinaryWriter(stream);


        writer.Write(meshSize);
        writer.Write(tileCount);
        writer.Write(triangleCount);
        writer.Write(vertexSize);
        writer.Write(vertexCount);


        int vertexCoordsLength = _vertexCoords.Length;
        for (int i = 0; i < vertexCoordsLength; i++) writer.Write(_vertexCoords[i].x);
        for (int i = 0; i < vertexCoordsLength; i++) writer.Write(_vertexCoords[i].y);
        for (int i = 0; i < vertexCoordsLength; i++) writer.Write(_vertexCoords[i].z);

        int normalsLength = _normals.Length;
        for (int i = 0; i < normalsLength; i++) writer.Write(_normals[i].x);
        for (int i = 0; i < normalsLength; i++) writer.Write(_normals[i].y);
        for (int i = 0; i < normalsLength; i++) writer.Write(_normals[i].z);

        int UVCoordsLength = _UVCoords.Length;
        for (int i = 0; i < UVCoordsLength; i++) writer.Write(_UVCoords[i].x);
        for (int i = 0; i < UVCoordsLength; i++) writer.Write(_UVCoords[i].y);

        int triVertIDsLength = _triVertIDs.Length;
        for (int i = 0; i < triVertIDsLength; i++) writer.Write(_triVertIDs[i]);
    }
}

public class TextureData
{
    // Member variables
    private SAbleColor[] _colorMap;
    public SAbleColor[] colorMap
    {
        get { return _colorMap; }
    }

    // Constructor
    public TextureData(){}

    // External methods
    public void Generate(Action callback, NoiseData inNoiseData, World.WorldGenData inWorldGenData, ChunkGenerator inChunkGenerator)
    {
        int textureSize = inWorldGenData.chunkSize;
        Gradient tempTerrainGradient = inWorldGenData.tempTerrainGradient;
        float[,] heightMap = inNoiseData.heightMap.noise;

        // Calculate colormap
        _colorMap = new SAbleColor[textureSize * textureSize];
        for (int y = 0; y < textureSize; y++)
            for (int x = 0; x < textureSize; x++)
                _colorMap[y * textureSize + x] = tempTerrainGradient.Evaluate(heightMap[x, y]);

        lock (inChunkGenerator.textureDataThreadInfoQueue)
            inChunkGenerator.textureDataThreadInfoQueue.Enqueue(callback);
    }

    public void WriteToStream(MemoryStream stream)
    {
        BinaryWriter writer = new BinaryWriter(stream);

        int colorMapLength = _colorMap.Length;

        for (int i = 0; i < colorMapLength; i++) writer.Write(_colorMap[i].r);
        for (int i = 0; i < colorMapLength; i++) writer.Write(_colorMap[i].g);
        for (int i = 0; i < colorMapLength; i++) writer.Write(_colorMap[i].b);
        for (int i = 0; i < colorMapLength; i++) writer.Write(_colorMap[i].a);
    }
}



// TODO: Make NoiseData.chunkCoords serializable
// TODO: Make World.WorldGenData serializable
// TODO: Remove NoiseData.chunkGenerator and pass it through the generate method instead
// TODO: Remove NoiseData member variable from MeshData and TextureData since it can be passed through the generate method
