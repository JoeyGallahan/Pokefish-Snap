using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshFilter meshFilter;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private int vertexIndex = 0;
    private Vector2 coord;
    List<Color> colors = new List<Color>();

    private ChunkManager chunkManager;
    public float[,] noise;

    private byte[,,] textureMap = new byte[ChunkManager.CHUNK_WIDTH, ChunkManager.CHUNK_HEIGHT, ChunkManager.CHUNK_WIDTH];

    private bool isWaterChunk;

    public void Init(Vector2 chunkCoord, float[,] heightMap, bool waterChunk = false)
    {
        coord = chunkCoord;
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        chunkManager = GameObject.FindGameObjectWithTag("ChunkManager").GetComponent<ChunkManager>();
        noise = heightMap;
        isWaterChunk = waterChunk;

        CreateChunk();
    }

    public void CreateChunk()
    {
        for (int x = 0; x < ChunkManager.CHUNK_WIDTH; x++)
        {
            for (int z = 0; z < ChunkManager.CHUNK_WIDTH; z++)
            {
                int height = GetHeightOfBlock(new Vector3(x, 0.0f, z));

                for (int y = 0; y < height; y++)
                {
                    CreateBlock(new Vector3(x, y, z));
                }
            }
        }
        CreateMesh();
    }

    public void UpdateChunk(float[,] heightMap)
    {
        noise = heightMap;
        ClearMeshData();
        CreateChunk();
    }

    private void CreateBlock(Vector3 pos)
    {
        textureMap[(int)pos.x, (int)pos.y, (int)pos.z] = chunkManager.GetTerrainType((int)pos.y, isWaterChunk);
        for (int f = 0; f < 6; f++) //faces in the order of the faces in voxel data
        {
            if (IsFaceVisible(pos,f))
            {
                byte blockID = textureMap[(int)pos.x, (int)pos.y, (int)pos.z];
                ChunkManager.TerrainType terrain = chunkManager.terrainTypes[blockID];
                //Make vertices
                for (int i = 0; i < 4; i++)
                {
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[f, i]]);

                    float lightLevel = ChunkManager.shadowLightLevel;
                    colors.Add(new Color(0.0f, 0.0f, 0.0f, lightLevel));
                    /* Shading
                    int shadeStacks = IsInShade(pos);
                    float lightLevel = ChunkManager.shadowLightLevel * shadeStacks;
                    if (f == VoxelData.TOP)
                    {
                        lightLevel *= 1.25f;
                    }

                    colors.Add(new Color(0.0f, 0.0f, 0.0f, lightLevel));
                    */
                }


                AddTexture(terrain.GetTextureID(f));

                //Make triangles
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);

                vertexIndex += 4;
            }
        }
    }

    private void CreateMesh()
    {
        //Initialize a mesh with all the vertices, triangles, and UVs we just made
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();

        //Make it actually exist and tie it to our mesh filter
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;

        //Add a mesh collider so our player and other gameobjects can collide with it
        MeshCollider col = gameObject.AddComponent<MeshCollider>();
        col.sharedMesh = mesh;
    }

    private void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        colors.Clear();
    }

    private int GetHeightOfBlock(Vector3 pos, float[,] heightMap = null)
    {
        if (heightMap == null)
        {
            heightMap = noise;
        }

        int height;

        if (isWaterChunk)
        {
            height = Mathf.FloorToInt(ChunkManager.WATER_CHUNK_HEIGHT * heightMap[(int)pos.x, (int)pos.z]);
        }
        else
        {
            height = Mathf.FloorToInt(ChunkManager.CHUNK_HEIGHT * heightMap[(int)pos.x, (int)pos.z]);
        }
        if (height == 0)
        {
            height = 1;
        }

        return height;
    }

    private bool IsFaceVisible(Vector3 pos, int face)
    {
        if (face == VoxelData.BOT) //Bottom face should never show at the moment except for on the water surface. Will need to change if I add caves or arches later.
        {
            return false;
        }
        else if (face != VoxelData.TOP)
        {
            if (pos.y == 0.0f)
            {
                return false;
            }

            if (face == VoxelData.LEFT && pos.x <= 0)
            {
                return true;
            }
            else if (face == VoxelData.RIGHT && pos.x >= ChunkManager.CHUNK_WIDTH - 1)
            {
                return true;
            }
            else if (face == VoxelData.FRONT && pos.z >= ChunkManager.CHUNK_WIDTH - 1)
            {
                return true;
            }
            else if (face == VoxelData.BACK && pos.z <= 0)
            {
                return true;
            }
            else if (pos.y == 0)
            {
                return false;
            }
            else if (pos.y > GetHeightOfBlock(pos + VoxelData.voxelFaceChecks[face]) - 1)
            {
                return true;
            }
        }
        else if (pos.y >= GetHeightOfBlock(pos) - 1) //If this is the top face and it's at the top of this stack of blocks
        {
            return true;
        }

        return false;
    }

    private int IsInShade(Vector3 pos)
    {
        int shadeStacks = 0;
        int heightOfPos = GetHeightOfBlock(pos);

        if (pos.y == heightOfPos - 1 && pos.y < ChunkManager.CHUNK_HEIGHT - 1) //We only care about the block at the top of its stack and if it's at the absolute top of the chunk, there are no shadows
        {
            Vector3 otherChunkNeighbor = pos;
            int[] ignoreSide = new int[2] //Any sides we want to ignore. If we're on the edge of a chunk there's no point in checking outside the chunk
            {
                -1,-1
            };
            float[,] heightMap = null;

            if (pos.x == 0) //Left-most side
            {
                heightMap = chunkManager.GetChunk(coord, VoxelData.voxelFaceChecks[VoxelData.LEFT]);
                otherChunkNeighbor.x = ChunkManager.CHUNK_WIDTH - 1;
                ignoreSide[0] = VoxelData.LEFT;
            }
            else if (pos.x >= ChunkManager.CHUNK_WIDTH - 1) //Right-most side
            {
                heightMap = chunkManager.GetChunk(coord, VoxelData.voxelFaceChecks[VoxelData.RIGHT]);
                otherChunkNeighbor.x = 0.0f;
                ignoreSide[0] = VoxelData.RIGHT;
            }
            if (pos.z >= ChunkManager.CHUNK_WIDTH - 1) //Front-most side
            {
                heightMap = chunkManager.GetChunk(coord, VoxelData.voxelFaceChecks[VoxelData.FRONT]);
                otherChunkNeighbor.z = 0.0f;
                ignoreSide[1] = VoxelData.FRONT;

            }
            else if (pos.z == 0) //Back-most side
            {
                heightMap = chunkManager.GetChunk(coord, VoxelData.voxelFaceChecks[VoxelData.BACK]);
                otherChunkNeighbor.z = ChunkManager.CHUNK_WIDTH - 1;
                ignoreSide[1] = VoxelData.BACK;
            }

            if (heightMap != null) //If we need to look at a neighboring chunk
            {
                int heightOfNeighbor = GetHeightOfBlock(otherChunkNeighbor, heightMap); //Get the height of the neighboring block we need
                if (heightOfNeighbor > heightOfPos)
                {
                    shadeStacks++;
                }
            }

            //If there is a block on any side 1 block up, this block is in shade
            for (int i = 0; i < VoxelData.voxelFaceChecks.Length; i++)
            {
                if (i != ignoreSide[0] && i != ignoreSide[1] && GetHeightOfBlock(pos + VoxelData.voxelFaceChecks[i])-1 > pos.y)
                {
                    shadeStacks++;
                }
            }       
        }        

        return shadeStacks;
    }

    private void PopulateTextureMap()
    {
        for (int x = 0; x < ChunkManager.CHUNK_WIDTH; x++)
        {
            for (int z = 0; z < ChunkManager.CHUNK_WIDTH; z++)
            {
                for (int y = 0; y < GetHeightOfBlock(new Vector3(x, 0.0f, z)); y++)
                {
                    textureMap[x, y, z] = 0;
                }
            }
        }
    }

    private void AddTexture(int textureID)
    {
        //Basically get the coordinates (on the atlas) of the texture we want
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        //Normalize our values
        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1.0f - y - VoxelData.NormalizedBlockTextureSize; //So that our textures start at the top left going to the bottom right. Unity starts bottom left going to top right.

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

    public bool IsVisible()
    {
        return gameObject.activeInHierarchy;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
