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
    MeshCollider col;
    Mesh mesh;

    private ChunkManager chunkManager;
    public float[,] noise;

    private byte[,,] textureMap = new byte[ChunkManager.CHUNK_WIDTH, ChunkManager.CHUNK_HEIGHT, ChunkManager.CHUNK_WIDTH];

    private bool isWaterChunk;

    public void Init(Vector2 chunkCoord, float[,] heightMap, bool waterChunk = false)
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        chunkManager = GameObject.FindGameObjectWithTag("ChunkManager").GetComponent<ChunkManager>();
        if (!isWaterChunk)
        col = gameObject.AddComponent<MeshCollider>();
        mesh = new Mesh();

        coord = chunkCoord;
        noise = heightMap;
        isWaterChunk = waterChunk;
        col.convex = true;
        col.isTrigger = true;
    }

    public void CreateChunk()
    {
        for (int x = 0; x < ChunkManager.CHUNK_WIDTH; x++)
        {
            for (int z = 0; z < ChunkManager.CHUNK_WIDTH; z++)
            {
                int height = GetHeightOfStack(new Vector3(x, 0, z));
                for (int y = 0; y < height; y++)
                {
                    CreateBlock(new Vector3(x, y, z));
                }
            }
        }
    }

    public void UpdateChunk(float[,] heightMap)
    {
        if (heightMap != noise)
        {
            noise = heightMap;
            ClearMeshData();
            CreateChunk();
        }
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

                CreateVertices(pos, f);
                CreateTriangles();

                AddTexture(terrain.GetTextureID(f));

                vertexIndex += 4;
            }
        }
    }

    //Make vertices into a beautiful face that becomes 1 side of a block
    private void CreateVertices(Vector3 pos, int face)
    {
        for (int i = 0; i < 4; i++) //There are 4 vertices for each face because that's what a square is
        {
            vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[face, i]]);

            //Shading

            //int shadeStacks = IsInShade(pos);
            float lightLevel = ChunkManager.shadowLightLevel;
            if (face == VoxelData.TOP)
            {
                lightLevel *= 1.25f;
            }

            colors.Add(new Color(0.0f, 0.0f, 0.0f, lightLevel));
        }
    }

    //Make Triangles that form a square
    private void CreateTriangles()
    {
        //Triangle 1
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);

        //Triangle 2 electric boogaloo
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 3);
    }

    //Puts all our data together into one mesh that can be rendered
    public void CreateMesh()
    {
        if (isWaterChunk)
        {
            mesh.MarkDynamic();
        }

        //Initialize a mesh with all the vertices, triangles, and UVs we just made
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();

        //Make it actually exist and tie it to our mesh filter
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;

        //Set up our mesh collider so our player and other gameobjects can collide with it
        if (!isWaterChunk)
            col.sharedMesh = mesh;
    }

    //Clear out all the mesh data
    private void ClearMeshData()
    {
        vertexIndex = 0;
        
        mesh.triangles = null;
        mesh.vertices = null;
        mesh.uv = null;
        mesh.colors = null;

        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        colors.Clear();
    }

    //Gets the height of the tallest block in a given x,z position of achunk
    private int GetHeightOfStack(Vector3 pos, float[,] heightMap = null)
    {
        if (heightMap == null)
        {
            heightMap = noise;
        }

        int height;
        if (isWaterChunk)
        {
            height = Mathf.FloorToInt(ChunkManager.WATER_CHUNK_HEIGHT * heightMap[(int)pos.x, (int)pos.z]); //De-Normalize the value from the noisemap
        }
        else
        {
            height = Mathf.FloorToInt(ChunkManager.CHUNK_HEIGHT * heightMap[(int)pos.x, (int)pos.z]); //De-Normalize the value from the noisemap
        }
        
        if (height == 0)
        {
            height = 1;
        }

        return height;
    }

    //Determines if a specific face of a given block should be rendered.
    //If there is no block obstructing this face, it should be visible. If there is a block on the side of the face, we shouldn't render it.
    private bool IsFaceVisible(Vector3 pos, int face)
    {
        if (face == VoxelData.BOT) //Bottom face should never show at the moment. Will need to change if I add caves or arches later.
        {
            return false;
        }
        else if (face != VoxelData.TOP)
        {
            if (pos.y == 0.0f) //If this is the very bottom block, there's always going to be a block next to it, so there's no need to render any of the sides.
            {
                return false;
            }

            //Handles drawing faces between chunks
            Vector3 coordV3 = new Vector3(coord.x, 0, coord.y);
            Vector3 otherChunkNeighbor = pos;
            bool checkChunk = false;

            if (face == VoxelData.LEFT && pos.x == 0) //If this is the left face on the Left-most side of a chunk
            {
                checkChunk = true;
                otherChunkNeighbor.x = ChunkManager.CHUNK_WIDTH - 1;
            }
            else if (face == VoxelData.RIGHT && pos.x >= ChunkManager.CHUNK_WIDTH - 1) //If this is the left face on the Right-most side of a chunk
            {
                checkChunk = true;
                otherChunkNeighbor.x = 0.0f;
            }
            else if (face == VoxelData.FRONT && pos.z >= ChunkManager.CHUNK_WIDTH - 1) //If this is the left face on the Front-most side of a chunk
            {
                checkChunk = true;
                otherChunkNeighbor.z = 0.0f;
            }
            else if (face == VoxelData.BACK && pos.z == 0) //If this is the left face on the Back-most side of a chunk
            {
                checkChunk = true;
                otherChunkNeighbor.z = ChunkManager.CHUNK_WIDTH - 1;
            }

            //If we need to look at a neighboring chunk
            if (checkChunk) 
            {
                int heightOfNeighbor = GetHeightOfStack(otherChunkNeighbor, chunkManager.GetChunkHeightMap(coordV3, VoxelData.voxelFaceChecks[face], isWaterChunk)); //Get the height of the neighboring block we need

                if (heightOfNeighbor-1 < pos.y )
                {
                    return true;
                }
                return false;
            }

            //If it's not on an edge, check the block on whatever side your block face is to see if there's anything there
            if (pos.y > GetHeightOfStack(pos + VoxelData.voxelFaceChecks[face]) - 1)
            {
                return true;
            }
        }
        else if (pos.y >= GetHeightOfStack(pos) - 1) //If this is the top face and it's at the top of this stack of blocks
        {
            return true;
        }

        return false;
    }

    private int IsInShade(Vector3 pos)
    {
        int shadeStacks = 0;
        int heightOfPos = GetHeightOfStack(pos);

        if (pos.y == heightOfPos - 1 && pos.y < ChunkManager.CHUNK_HEIGHT - 1) //We only care about the block at the top of its stack and if it's at the absolute top of the chunk, there are no shadows
        {
            
            int[] ignoreSide = new int[2] //Any sides we want to ignore. If we're on the edge of a chunk there's no point in checking outside the chunk
            {
                -1,-1
            };

            //Handles drawing faces between chunks
            Vector3 coordV3 = new Vector3(coord.x, 0, coord.y);
            Vector3 otherChunkNeighbor = pos;
            bool checkChunk = false;

            if (pos.x == 0)
            {
                checkChunk = true;
                otherChunkNeighbor.x = ChunkManager.CHUNK_WIDTH - 1;
                ignoreSide[0] = VoxelData.LEFT;
            }
            else if (pos.x >= ChunkManager.CHUNK_WIDTH - 1)
            {
                checkChunk = true;
                otherChunkNeighbor.x = 0.0f;
                ignoreSide[0] = VoxelData.RIGHT;
            }
            if (pos.z >= ChunkManager.CHUNK_WIDTH - 1)
            {
                checkChunk = true;
                otherChunkNeighbor.z = 0.0f;
                ignoreSide[1] = VoxelData.FRONT;
            }
            else if (pos.z == 0)
            {
                checkChunk = true;
                otherChunkNeighbor.z = ChunkManager.CHUNK_WIDTH - 1;
                ignoreSide[1] = VoxelData.BACK;
            }

            /*
            //If we need to look at a neighboring chunk
            if (checkChunk)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (ignoreSide[i] != -1)
                    {
                        int heightOfNeighbor = GetHeightOfStack(otherChunkNeighbor, chunkManager.GetChunkHeightMap(coordV3, VoxelData.voxelFaceChecks[ignoreSide[i]], isWaterChunk)); //Get the height of the neighboring block we need

                        if (heightOfNeighbor - 1 > pos.y)
                        {
                            shadeStacks++;
                        }
                    }
                }
            }
            */
            //Debug.Log("Pos: " + pos + "ig1:" + ignoreSide[0] + " ig2: " + ignoreSide[1]);
            //If there is a block on any side 1 block up, this block is in shade
            for (int i = 0; i < VoxelData.voxelFaceChecks.Length; i++)
            {
                if (i != ignoreSide[0] && i != ignoreSide[1] && GetHeightOfStack(pos + VoxelData.voxelFaceChecks[i])-1 > pos.y)
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
                for (int y = 0; y < GetHeightOfStack(new Vector3(x, 0.0f, z)); y++)
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

        //Add the coordinates of the texture we want to our UV list
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
