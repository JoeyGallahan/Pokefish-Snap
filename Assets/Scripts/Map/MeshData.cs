using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector2> uvs = new List<Vector2>();
    public int vertexIndex = 0;
    public List<Color> colors = new List<Color>();

    public void AddFace()
    {
        //Triangle 1
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);

        //Triangle 2 electric boogaloo
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 3);

        vertexIndex += 4;
    }

    //Puts all our data together into one mesh that can be rendered
    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        //Initialize a mesh with all the vertices, triangles, and UVs we just made
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();

        //Make it actually exist and tie it to our mesh filter
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    public void AddTexture(int textureID)
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
}
