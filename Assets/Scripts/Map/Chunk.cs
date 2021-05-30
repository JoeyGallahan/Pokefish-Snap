using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    private GameObject meshObject;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshFilter meshFilter;

    private ChunkManager chunkManager;
    public float[,] noise { get; private set; }

    private bool isWaterChunk;

    public Chunk(Vector2 coord, Transform parent, Material mat, bool isWater = false)
    {
        Vector3 chunkPos = Vector3.zero;
        chunkPos.x = coord.x * ChunkManager.CHUNK_WIDTH;
        chunkPos.z = coord.y * ChunkManager.CHUNK_WIDTH;

        isWaterChunk = isWater;

        meshObject = new GameObject();
        meshObject.transform.parent = parent;
        if (isWater)
        {
            meshObject.name = "Water Chunk " + (int)coord.x + "," + (int)coord.y;
            chunkPos.y = ChunkManager.CHUNK_HEIGHT * 2.0f;
        }
        else
        {
            meshObject.name = "Chunk " + (int)coord.x + "," + (int)coord.y;
        }
        meshObject.transform.position = chunkPos;
        meshObject.transform.localScale = Vector3.one;

        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = mat;
        meshFilter = meshObject.AddComponent<MeshFilter>();

        chunkManager = GameObject.FindGameObjectWithTag("ChunkManager").GetComponent<ChunkManager>();
        Hide();

        chunkManager.RequestChunkData(OnChunkDataReceived, isWater);
    }

    private void OnChunkDataReceived(ChunkData data)
    {
        noise = data.heightMap;
        chunkManager.RequestMeshData(data, OnMeshDataReceived);
    }

    private void OnMeshDataReceived(MeshData data)
    {
        meshFilter.mesh = data.CreateMesh();
    }

    public bool IsVisible()
    {
        return meshObject.activeInHierarchy;
    }

    public void Show()
    {
        meshObject.SetActive(true);
    }

    public void Hide()
    {
        meshObject.SetActive(false);
    }

    public void UpdateChunk()
    {
        chunkManager.RequestChunkData(OnChunkDataReceived, isWaterChunk);
    }
}