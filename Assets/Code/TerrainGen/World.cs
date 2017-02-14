using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

[CustomEditor(typeof(World))]
public class WorldEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        World worldScript = (World)target;

        if (GUILayout.Button("Generate World"))
        {
            if (Application.isPlaying)
                worldScript.UpdateChunks();

            else
                Debug.Log("Cannot generate terrain in edit mode");
        }
    }
}


public class World : MonoBehaviour
{
    /* Member variables */
    
    [SerializeField]
    private WorldGenData _worldGenData;
    public WorldGenData worldGenData
    {
        get { return _worldGenData; }
    }

    [System.Serializable]
    public struct WorldGenData
    {
        [Header("World Settings")]
        [Range(16, 124)]
        public int chunkSize;                           // Chunk size in quads
        public int renderDistance;                      // Render distance in chunks. Should be an odd number.
        public Gradient tempTerrainGradient;            // Gradient used in order to color the terrain
        public float meshHeightMultiplier;              // Number used to determine how much the height map affects the terrain mesh
        public AnimationCurve heightMultiplierCurve;    // Curve used to determine how much the height map affects the terrain mesh at different values

        [Header("Temparature Map Settings")]
        public int tempSeed;                            // Number which is the "ID" of the generation. Same seed will always result in the same generation
        public int tempScale;                           // Determines how "zoomed in" the noise is
        public int tempOctaves;                         // Determines how detailed the noise is. The more octaves, the more details
        public float tempPersistance;                   // TODO: Write comment
        public float tempLacunarity;                    // TODO: Write comment
        public float tempRedistribution;                // Raises the overall height of the noise
                                                        // TODO: Add value that raises the minimum cap of noise values
        [Header("Humidity Map Settings")]
        public int humidSeed;
        public int humidScale;
        public int humidOctaves;
        public float humidPersistance;
        public float humidLacunarity;
        public float humidRedistribution;

        [Header("Height Map Settings")]
        public int heightSeed;
        public int heightScale;
        public int heightOctaves;
        public float heightPersistance;
        public float heightLacunarity;
        public float heightRedistribution;

        [Header("Falloff Map Settings")]
        public int falloffSeed;
        public int falloffScale;
        public int falloffOctaves;
        public float falloffPersistance;
        public float falloffLacunarity;
        public float falloffRedistribution;
    }

    private ChunkGenerator _chunkGenerator;

    private Dictionary<Vector2, Chunk> _worldChunks = new Dictionary<Vector2, Chunk>();
    public Dictionary<Vector2, Chunk> worldChunks
    {
        get { return _worldChunks; }
    }

    private Vector2 _currentCameraChunkCoord = Vector2.one;
    public Vector2 currentCameraChunkCoord
    {
        get { return _currentCameraChunkCoord; }

        private set
        {
            if (_currentCameraChunkCoord != value)
                OnCameraChunkEnter(value);

            _currentCameraChunkCoord = value;
        }
    }

    private List<Vector2> _chunksToRemoveCoordsQueue = new List<Vector2>();
    private List<Vector2> _chunksToGenerateCoordsQueue = new List<Vector2>();

    private Transform _cameraControllerTransform;
    private bool _isGeneratingChunks = false;


    [SerializeField] private GameObject _townPrefab;


    /* Start, Update */
    void Start()
    {
        _chunkGenerator = GetComponent<ChunkGenerator>();
        _cameraControllerTransform = Camera.main.transform.parent;
    }

    void Update()
    {
        currentCameraChunkCoord = GetChunkCoord(_cameraControllerTransform.position);

        ProcessQueues();
    }


    /* Internal methods */
    private IEnumerator GenerateChunks()
    {
        _isGeneratingChunks = true;


        while(_chunksToGenerateCoordsQueue.Count > 0)
        {
            _worldChunks.Add(_chunksToGenerateCoordsQueue[0], _chunkGenerator.GenerateChunk(_chunksToGenerateCoordsQueue[0]));

            _chunksToGenerateCoordsQueue.RemoveAt(0);

            yield return null;
        }

        yield return null;
        _isGeneratingChunks = false;
    }

    private void ProcessQueues()
    {
        if (!_isGeneratingChunks)
            StartCoroutine("GenerateChunks");

        if (_chunksToRemoveCoordsQueue.Count > 0)
            DeleteChunks();
    }

    private void DeleteChunks()
    {
        while (_chunksToRemoveCoordsQueue.Count > 0)
        {
            Destroy(_worldChunks[_chunksToRemoveCoordsQueue[0]].gameObject);
            _worldChunks.Remove(_chunksToRemoveCoordsQueue[0]);

            _chunksToRemoveCoordsQueue.RemoveAt(0);
        }
    }

    private Vector2 GetChunkCoord(Vector3 inWorldPosition)
    {
        Vector2 chunkCoords = new Vector2(inWorldPosition.x / _worldGenData.chunkSize, -(inWorldPosition.z / _worldGenData.chunkSize));

        Vector2 roundedChunkCoords = new Vector2(Mathf.Floor(chunkCoords.x), Mathf.Ceil(chunkCoords.y));

        return roundedChunkCoords;
    }

    private void OnCameraChunkEnter(Vector2 inToChunk)
    {
        Vector2[] visibleChunkCoords = new Vector2[_worldGenData.renderDistance * _worldGenData.renderDistance];

        int chunksToSide = (_worldGenData.renderDistance - 1) / 2;


        // Calculate the coordinates of all the chunks that are inside of render distance
        for (int y = 0; y < _worldGenData.renderDistance; y++)
            for (int x = 0; x < _worldGenData.renderDistance; x++)
                visibleChunkCoords[y * _worldGenData.renderDistance + x] = new Vector2(x - chunksToSide + inToChunk.x, 
                                                                                       y - chunksToSide + inToChunk.y);

        // Add all the visible chunk coords to the delete queue
        _chunksToRemoveCoordsQueue.Clear();
        foreach (KeyValuePair<Vector2, Chunk> entry in _worldChunks)
            _chunksToRemoveCoordsQueue.Add(entry.Key);

        // Queue up coords to generate chunks on
        _chunksToGenerateCoordsQueue.Clear();
        for (int i = 0; i < visibleChunkCoords.Length; i++)
        {
            if (!_worldChunks.ContainsKey(visibleChunkCoords[i]))
                _chunksToGenerateCoordsQueue.Add(visibleChunkCoords[i]);

            else if (_chunksToRemoveCoordsQueue.Contains(visibleChunkCoords[i]))
                _chunksToRemoveCoordsQueue.Remove(visibleChunkCoords[i]);
        }
    }


    /* External methods */
    public void UpdateChunks()
    {
        foreach (Vector2 chunkCoord in _worldChunks.Keys)
            _chunkGenerator.RegenerateChunk(_worldChunks[chunkCoord]);
    }

    public GameObject GenerateTown(Vector3 inWorldPosition)
    {
        GameObject newTown = Instantiate(_townPrefab);

        newTown.transform.position = inWorldPosition;

        return newTown;
    }
}

// TODO: Assign every vertice a biome, have biome specific terrain gen values
// TODO: Erosion n shiet
// TODO: Replace the lists with actual Queues? 
// TODO: Rewrite the way chunks to delete and generate is determined
// TODO: Implement BIOMES ALREADY AAH
// TODO: See if the regenerate Texture / Mesh methods still work or can be repaired if they don't

/* TODO LIST */
/*
 *  - Every quad is represented by the values of its top left vertex. Assign every vertex a specific biome.
 *  - Have biomes use specific terrain gen values
 *  - Implement thermal erosion
 *  - Implement hydraulic eriosion
 *  - Rewrite how the delete and generation queues are populated
 */