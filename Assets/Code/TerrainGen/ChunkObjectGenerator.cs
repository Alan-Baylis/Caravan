using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkObjectGenerator : MonoBehaviour
{
    // Member variables
    [SerializeField] private GameObject[] _chunkObjects;

    [System.Serializable]
    private struct ChunkObjectGenTries
    {
        [Space(5)]
        public int pineMin; 
        public int pineMax;

        [Space(5)]
        public int townMin;
        public int townMax;
    }
    [SerializeField] private ChunkObjectGenTries _chunkObjectGenTries;

    private World _world;

    // Start, Update
    void Start()
    {
        _world = GetComponent<World>();
    }

    // External
    public void GenerateChunkObjects(Chunk inChunk)
    {
        Random.InitState(inChunk.coords.GetHashCode());

        int chunkSize = _world.worldGenData.chunkSize;
        AnimationCurve meshHeightMultiplierCurve = _world.worldGenData.heightMultiplierCurve;
        float meshHeightMultiplier = _world.worldGenData.meshHeightMultiplier;

        GenerateTowns(inChunk, chunkSize, meshHeightMultiplierCurve, meshHeightMultiplier);
        GenerateTrees(inChunk, chunkSize, meshHeightMultiplierCurve, meshHeightMultiplier);
    }

    // Internal
    private GameObject[] GenerateTrees(Chunk inChunk, int inChunkSize, AnimationCurve inHeightMultiplierCurve, float inMeshHeightMultiplier)
    {
        // Calculate gen tries. If 0, return empty array
        int treeGenerationTries = Random.Range(_chunkObjectGenTries.pineMin, _chunkObjectGenTries.pineMax);
        if (treeGenerationTries <= 0 || !inChunk.gameObject)
            return new GameObject[0];

        // Try to generate GenTries amount of trees, and add them to generatedTrees
        List<GameObject> generatedTrees = new List<GameObject>();
        for (int i = 0; i < treeGenerationTries; i++)
        {
            int newTreePosX = Random.Range(0, inChunkSize);
            int newTreePosY = Random.Range(0, inChunkSize);

            float targetTileNoiseHeight = inChunk.GetTileHeight(newTreePosX, newTreePosY);
            if (_world.biomeManager.GetBiome(targetTileNoiseHeight).biomeType != BiomeType.Forest)
                continue;

            float targetTileWSHeight = targetTileNoiseHeight * inHeightMultiplierCurve.Evaluate(targetTileNoiseHeight) * inMeshHeightMultiplier;

            Vector3 newTreeWSPos = new Vector3(newTreePosX + (inChunkSize * inChunk.coords.x), targetTileWSHeight, newTreePosY - (inChunkSize * inChunk.coords.y));

            GameObject newTree = Instantiate(_chunkObjects[(int)ChunkObject.Pine]);
            newTree.transform.position = newTreeWSPos + new Vector3(0, 1, 0);
            newTree.transform.Rotate(Vector3.up, Random.Range(0, 180));
            newTree.isStatic = true;

            generatedTrees.Add(newTree);
        }


        // If more than 0 trees managed to get generated, child them to an object 
        if (generatedTrees.Count > 0)
        {
            GameObject treesParentGO = new GameObject("Trees");
            treesParentGO.transform.SetParent(inChunk.gameObject.transform);

            for (int i = 0; i < generatedTrees.Count; i++)
            {
                generatedTrees[i].transform.SetParent(treesParentGO.transform);
            }

            MeshFilter[] treeMeshFilters = treesParentGO.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] treeCombines = new CombineInstance[treeMeshFilters.Length];

            for (int i = 0; i < treeMeshFilters.Length; i++)
            {
                treeCombines[i].mesh = treeMeshFilters[i].sharedMesh;
                treeCombines[i].transform = treeMeshFilters[i].transform.localToWorldMatrix;
                treeMeshFilters[i].gameObject.SetActive(false);
            }

            MeshFilter treesParentMeshFilter = treesParentGO.AddComponent<MeshFilter>();
            treesParentMeshFilter.mesh = new Mesh();
            treesParentMeshFilter.mesh.CombineMeshes(treeCombines);
            treesParentMeshFilter.gameObject.SetActive(true);

            MeshRenderer treesParentMeshRenderer = treesParentGO.AddComponent<MeshRenderer>();
            treesParentMeshRenderer.sharedMaterial = generatedTrees[0].GetComponent<MeshRenderer>().sharedMaterial;
        }

        return generatedTrees.ToArray();
    }

    private GameObject[] GenerateTowns(Chunk inChunk, int inChunkSize, AnimationCurve inHeightMultiplierCurve, float inMeshHeightMultiplier)
    {
        // Calculate gen tries. If 0, return empty array
        int townGenerationTries = Random.Range(_chunkObjectGenTries.townMin, _chunkObjectGenTries.townMax);
        if (townGenerationTries <= 0 || !inChunk.gameObject)
            return new GameObject[0];

        List<GameObject> generatedTowns = new List<GameObject>();
        for (int i = 0; i < townGenerationTries; i++)
        {
            int newTownPosX = Random.Range(0, inChunkSize);
            int newTownPosY = Random.Range(0, inChunkSize);

            float targetTileNoiseHeight = inChunk.GetTileHeight(newTownPosX, newTownPosY);
            float targetTileWSHeight = targetTileNoiseHeight * inHeightMultiplierCurve.Evaluate(targetTileNoiseHeight) * inMeshHeightMultiplier;

            Vector3 newTownWSPos = new Vector3(newTownPosX + (inChunkSize * inChunk.coords.x), targetTileWSHeight, newTownPosY - (inChunkSize * inChunk.coords.y));

            switch (_world.biomeManager.GetBiome(targetTileNoiseHeight).biomeType)
            {
                case BiomeType.Forest:
                case BiomeType.Plains:
                case BiomeType.Hills:
                    GameObject newTown = Instantiate(_chunkObjects[(int)ChunkObject.Town]);
                    newTown.transform.position = newTownWSPos + new Vector3(0, 4, 0);
                    newTown.transform.Rotate(Vector3.up, Random.Range(0, 180));
                    generatedTowns.Add(newTown);
                    break;
            }
        }

        if (generatedTowns.Count > 0)
        {
            GameObject townsParentGO = new GameObject("Towns");
            townsParentGO.transform.SetParent(inChunk.gameObject.transform, true);

            for (int i = 0; i < generatedTowns.Count; i++)
                generatedTowns[i].transform.SetParent(townsParentGO.transform);
        }

        return generatedTowns.ToArray();
    }

    public enum ChunkObject
    {
        Pine,
        Town
    }
}








