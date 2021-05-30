using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateMesh(float[,] noiseMap, byte[,,] textureMap, List<ChunkManager.TerrainType> terrainTypes, bool isWater = false)
    {
        MeshData meshData = new MeshData();
        for (int x = 0; x < ChunkManager.CHUNK_WIDTH; x++)
        {
            for (int z = 0; z < ChunkManager.CHUNK_WIDTH; z++)
            {
                int height = GetHeightOfStack(new Vector3(x, 0, z), noiseMap, isWater);
                for (int y = 0; y < height; y++)
                {
                    Vector3 pos = new Vector3(x, y, z);

                    //Create faces in the order of the faces in voxel data
                    for (int f = 0; f < 6; f++)
                    {
                        if (IsFaceVisible(pos, Vector2.zero, f, noiseMap, isWater))//Should this face be rendered?
                        {
                            byte blockID = textureMap[(int)pos.x, (int)pos.y, (int)pos.z];
                            ChunkManager.TerrainType terrain = terrainTypes[blockID]; //Get the type of terrain this block is at

                            //Create Vertices for the face
                            for (int i = 0; i < 4; i++) //There are 4 vertices for each face because that's what a square is
                            {
                                meshData.vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[f, i]]);

                                //Shading
                                //int shadeStacks = IsInShade(pos);
                                float lightLevel = ChunkManager.shadowLightLevel;
                                if (f == VoxelData.TOP)
                                {
                                    lightLevel *= 1.25f;
                                }

                                meshData.colors.Add(new Color(0.0f, 0.0f, 0.0f, lightLevel));
                            }
                            meshData.AddFace();

                            meshData.AddTexture(terrain.GetTextureID(f));
                        }
                    }
                }
            }
        }

        return meshData;
    }

    //Gets the height of the tallest block in a given x,z position of a chunk
    public static int GetHeightOfStack(Vector3 pos, float[,] heightMap, bool isWater = false)
    {
        int height;

        if (isWater)
        {
            height = Mathf.FloorToInt(ChunkManager.WATER_CHUNK_HEIGHT * heightMap[(int)pos.x, (int)pos.z]); //De-Normalize the value from the noisemap
            if (height >= ChunkManager.WATER_CHUNK_HEIGHT)
            {
                height = ChunkManager.WATER_CHUNK_HEIGHT - 1;
            }
        }
        else
        {
            height = Mathf.FloorToInt(ChunkManager.CHUNK_HEIGHT * heightMap[(int)pos.x, (int)pos.z]); //De-Normalize the value from the noisemap

            if (height >= ChunkManager.CHUNK_HEIGHT)
            {
                height = ChunkManager.CHUNK_HEIGHT - 1;
            }
        }

        if (height == 0)
        {
            height = 1;
        }


        return height;
    }

    //Determines if a specific face of a given block should be rendered.
    //If there is no block obstructing this face, it should be visible. If there is a block on the side of the face, we shouldn't render it.
    public static bool IsFaceVisible(Vector3 pos, Vector2 coord, int face, float[,] noise, bool isWater = false)
    {
        if (pos.y <= 1 && face == VoxelData.BOT)//Bottom face should never show at the moment on ground blocks. Will need to change if I add caves or arches later.
        {
            return isWater;
        }
        else if (face != VoxelData.TOP)
        {
            if (pos.y == 0.0f) //If this is the very bottom block, there's always going to be a block next to it, so there's no need to render any of the sides.
            {
                return false;
            }

            //Handles drawing faces between chunks
            Vector2 otherChunkNeighbor = pos;
            bool checkChunk = false;

            if (face == VoxelData.LEFT && pos.x == 0) //If this is the left face on the Left-most side of a chunk
            {
                checkChunk = true;
                otherChunkNeighbor.x = ChunkManager.CHUNK_WIDTH + 1;
            }
            else if (face == VoxelData.RIGHT && pos.x >= ChunkManager.CHUNK_WIDTH - 1) //If this is the left face on the Right-most side of a chunk
            {
                checkChunk = true;
                otherChunkNeighbor.x = ChunkManager.CHUNK_WIDTH;
            }
            else if (face == VoxelData.FRONT && pos.z >= ChunkManager.CHUNK_WIDTH - 1) //If this is the left face on the Front-most side of a chunk
            {
                checkChunk = true;
                otherChunkNeighbor.y = ChunkManager.CHUNK_WIDTH;
            }
            else if (face == VoxelData.BACK && pos.z == 0) //If this is the left face on the Back-most side of a chunk
            {
                checkChunk = true;
                otherChunkNeighbor.y = ChunkManager.CHUNK_WIDTH + 1;
            }

            //If we need to look at a neighboring chunk
            if (checkChunk)
            {
                //int heightOfNeighbor = GetHeightOfStack(otherChunkNeighbor, noise); //Get the height of the neighboring block we need
                return true;
            }

            //If it's not on an edge, check the block on whatever side your block face is to see if there's anything there
            if (pos.y > GetHeightOfStack(pos + VoxelData.voxelFaceChecks[face], noise, isWater) - 1)
            {
                return true;
            }
        }
        else if (pos.y >= GetHeightOfStack(pos, noise, isWater) - 1) //If this is the top face and it's at the top of this stack of blocks
        {
            return true;
        }
        return false;
    }

}
