using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ChunkManager : MonoBehaviour
{
    //Chunks
    public const int CHUNK_WIDTH = 32;
    public const int CHUNK_HEIGHT = 32;
    public const int WATER_CHUNK_HEIGHT = 1;
    private Dictionary<Vector2, Chunk> chunks = new Dictionary<Vector2, Chunk>();
    private Dictionary<Vector2, Chunk> waterChunks = new Dictionary<Vector2, Chunk>();
    [SerializeField] private GameObject chunkPrefab;
    public List<TerrainType> terrainTypes;
    public Vector3 spawn;
    [SerializeField] private Material terrainMaterial;
    [SerializeField] private Material waterMaterial;

    //Chunk Visibility
    List<Chunk> chunksVisibleLastFrame = new List<Chunk>();
    public static Vector2 viewerPosition;
    public static readonly int ViewDistanceInChunks = 12;
    [SerializeField] private Transform viewer;
    private Vector2 playerChunkCoords;

    //Noise
    [SerializeField] private float noiseScale;
    [SerializeField] private int octaves;
    [SerializeField] [Range(0, 1)] private float persistance;
    [SerializeField] private float lacunarity;
    [SerializeField] private int seed;
    [SerializeField] private Vector2 offset;
    [SerializeField] private AnimationCurve heightCurve;
    private float curTime = 0.0f;

    Queue<MapThreadInfo<ChunkData>> chunkThreadQueue = new Queue<MapThreadInfo<ChunkData>>();
    Queue<MapThreadInfo<MeshData>> meshThreadQueue = new Queue<MapThreadInfo<MeshData>>();

    public AnimationCurve HeightCurve
    {
        get => heightCurve;
    }

    //Other
    [Range(0.95f,0)]
    public float globalLightLevel;
    public static readonly float shadowLightLevel = 0.1f;
    public static Vector2 WaterSpeed = new Vector2(0.5f,0.5f);

    private void Start()
    {
        Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);

        UpdateVisibleChunks(true);
    }
    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        curTime = Time.time;

        if (chunkThreadQueue.Count > 0)
        {
            for (int i = 0; i < chunkThreadQueue.Count; i++)
            {
                MapThreadInfo<ChunkData> threadInfo = chunkThreadQueue.Dequeue();
                threadInfo.callback(threadInfo.param);
            }
        }

        if (meshThreadQueue.Count > 0)
        {
            for (int i = 0; i < meshThreadQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshThreadQueue.Dequeue();
                threadInfo.callback(threadInfo.param);
            }
        }

        UpdateVisibleChunks();
        //UpdateWaterChunks();
    }

    public void RequestChunkData(Action<ChunkData> callback, bool isWater = false)
    {
        ThreadStart threadStart = delegate
        {
            ChunkDataThread(callback, isWater);
        };
        new Thread(threadStart).Start();
    }

    private void ChunkDataThread(Action<ChunkData> callback, bool isWater = false)
    {
        ChunkData chunkData = GenerateChunkData(isWater);
        PopulateTextureMap(chunkData, isWater);

        lock(chunkThreadQueue)
        {
            chunkThreadQueue.Enqueue(new MapThreadInfo<ChunkData>(callback, chunkData));
        }
    }

    public void RequestMeshData(ChunkData chunkData, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(chunkData, callback);
        };

        new Thread(threadStart).Start();
    }

    private void MeshDataThread(ChunkData chunkData, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateMesh(chunkData.heightMap, chunkData.textureMap, terrainTypes, chunkData.isWater);
        lock (meshThreadQueue)
        {
            meshThreadQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T param;

        public MapThreadInfo(Action<T> callbackT, T parameter)
        {
            callback = callbackT;
            param = parameter;
        }

    }

    private void UpdateWaterChunks()
    {
        for (int i = 0; i < chunksVisibleLastFrame.Count; i++)
        {
            if (waterChunks.ContainsValue(chunksVisibleLastFrame[i]))
            {
                chunksVisibleLastFrame[i].UpdateChunk();
            }
        }
    }

    private ChunkData GenerateChunkData(bool isWater = false)
    {
        float[,] noise;

        if (isWater)
        {
            noise = Noise.GenerateWaterMap(CHUNK_WIDTH, CHUNK_WIDTH, seed, noiseScale, offset, curTime);
        }
        else
        {
            noise = Noise.GenerateNoiseMap(CHUNK_WIDTH, CHUNK_WIDTH, seed, noiseScale, octaves, persistance, lacunarity, offset, heightCurve);
        }

        return new ChunkData(noise, isWater);
    }

    private void UpdateVisibleChunks(bool initialLoad = false)
    {
        //Get the index of the current chunk the viewer is on
        int curChunkIndexX = Mathf.RoundToInt(viewerPosition.x / CHUNK_WIDTH);
        int curChunkIndexY = Mathf.RoundToInt(viewerPosition.y / CHUNK_WIDTH);

        Vector2 curChunk = new Vector2(curChunkIndexX, curChunkIndexY);

        if (curChunk != playerChunkCoords || initialLoad)
        {
            playerChunkCoords = curChunk;

            //Disable all chunks that were visible last frame (they'll be reenabled later if they should be viewed this frame)
            for (int i = 0; i < chunksVisibleLastFrame.Count; i++)
            {
                chunksVisibleLastFrame[i].Hide();
            }

            chunksVisibleLastFrame.Clear();

            List<Vector2> newChunks = new List<Vector2>();
            //Go through and each possible chunk around the current one
            for (int y = -ViewDistanceInChunks; y < ViewDistanceInChunks; y++)
            {
                for (int x = -ViewDistanceInChunks; x < ViewDistanceInChunks; x++)
                {
                    Vector2 viewedChunkIndex = new Vector2(curChunkIndexX + x, curChunkIndexY + y);

                    if (chunks.ContainsKey(viewedChunkIndex)) //If there is a chunk at this index
                    {
                        chunks[viewedChunkIndex].Show();
                        waterChunks[viewedChunkIndex].Show();
                        chunksVisibleLastFrame.Add(chunks[viewedChunkIndex]); //Add it to our list of currently viewable chunks
                        chunksVisibleLastFrame.Add(waterChunks[viewedChunkIndex]); //Add it to our list of currently viewable chunks
                    }
                    else //If there isn't a chunk at this index
                    {
                        newChunks.Add(viewedChunkIndex);
                    }
                }
            }

            for (int i = 0; i < newChunks.Count; i++)
            {
                offset = newChunks[i] * CHUNK_WIDTH;

                chunks.Add(newChunks[i], new Chunk(newChunks[i], transform, terrainMaterial));
                chunksVisibleLastFrame.Add(chunks[newChunks[i]]);
                chunksVisibleLastFrame[chunksVisibleLastFrame.Count - 1].Show();

                waterChunks.Add(newChunks[i], new Chunk(newChunks[i], transform, waterMaterial, true));
                chunksVisibleLastFrame.Add(waterChunks[newChunks[i]]);
                chunksVisibleLastFrame[chunksVisibleLastFrame.Count - 1].Show();
            }
        }
    }

    public byte GetTerrainType(int height, bool isWater = false)
    {
        float fHeight;

        if (isWater)
        {
            /*
            if (height >= WATER_CHUNK_HEIGHT - 2)
            {
                return 6;
            }
            */
            return 5;
        }

        fHeight = (float)height / (float)(CHUNK_HEIGHT - 1);

        for (int i = 0; i < terrainTypes.Count; i++)
        {
            if (fHeight >= terrainTypes[i].height)
            {
                return (byte)i;
            }
        }

        return 0;
    }

    [System.Serializable]
    public class TerrainType
    {
        public string name;
        public float height;
        public Color color;
        public float transparency = 1;

        [Header("Texture Values")]
        public int backFaceTexture;
        public int frontFaceTexture;
        public int topFaceTexture;
        public int botFaceTexture;
        public int leftFaceTexture;
        public int rightFaceTexture;

        public int GetTextureID(int faceIndex)
        {
            switch (faceIndex)
            {
                case 0:
                    return backFaceTexture;                    
                case 1:
                    return frontFaceTexture;                    
                case 2:
                    return topFaceTexture;                    
                case 3:
                    return botFaceTexture;                    
                case 4:
                    return leftFaceTexture;                    
                case 5:
                    return rightFaceTexture;                    
                default: Debug.Log("How'd you do that? Theres no faceIndex there.");
                    return -1;          
            }
        }
    }

    public void PopulateTextureMap(ChunkData data, bool isWater = false)
    {
        for (int x = 0; x < CHUNK_WIDTH; x++)
        {
            for (int z = 0; z < CHUNK_WIDTH; z++)
            {
                for (int y = 0; y < CHUNK_HEIGHT; y++)
                {
                    data.textureMap[x, y, z] = GetTerrainType(y, isWater);
                }
            }
        }
    }

}

public struct ChunkData
{
    public readonly float[,] heightMap;
    public readonly byte[,,] textureMap;
    public readonly bool isWater;

    public ChunkData(float[,] noise, bool water)
    {
        heightMap = noise;
        isWater = water;
        textureMap = new byte[ChunkManager.CHUNK_WIDTH, ChunkManager.CHUNK_HEIGHT, ChunkManager.CHUNK_WIDTH];

        for (int x = 0; x < ChunkManager.CHUNK_WIDTH; x++)
        {
            for (int z = 0; z < ChunkManager.CHUNK_WIDTH; z++)
            {
                for (int y = 0; y < ChunkManager.CHUNK_HEIGHT; y++)
                {
                    textureMap[x, y, z] = 0;
                }
            }
        }
    }
}