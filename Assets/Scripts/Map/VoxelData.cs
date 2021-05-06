using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData
{
    public static readonly int TextureAtlasSizeInBlocks = 4;
    public static float NormalizedBlockTextureSize
    {
        get { return 1.0f / (float)TextureAtlasSizeInBlocks; }
    }

    public static readonly int BACK = 0;
    public static readonly int FRONT = 1;
    public static readonly int TOP = 2;
    public static readonly int BOT = 3;
    public static readonly int LEFT = 4;
    public static readonly int RIGHT = 5;

    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        new Vector3(0.0f,0.0f,0.0f),  //0
        new Vector3(1.0f,0.0f,0.0f),  //1
        new Vector3(1.0f,1.0f,0.0f),  //2
        new Vector3(0.0f,1.0f,0.0f),  //3
        new Vector3(0.0f,0.0f,1.0f),  //4
        new Vector3(1.0f,0.0f,1.0f),  //5
        new Vector3(1.0f,1.0f,1.0f),  //6
        new Vector3(0.0f,1.0f,1.0f)   //7
    };

    public static readonly int[,] voxelTris = new int[6, 4]
    {
        {0,3,1,2}, //Back face
        {5,6,4,7}, //Front face
        {3,7,2,6}, //Top face
        {1,5,0,4}, //Bot face
        {4,7,0,3}, //Left face
        {1,2,5,6}  //Right face
    };

    public static readonly Vector3[] voxelFaceChecks = new Vector3[6]
    {
        new Vector3(0.0f,0.0f,-1.0f), //Back face
        new Vector3(0.0f,0.0f,1.0f), //Front face
        new Vector3(0.0f,1.0f,0.0f), //Top face
        new Vector3(0.0f,-1.0f,0.0f), //Bot face
        new Vector3(-1.0f,0.0f,0.0f), //Left face
        new Vector3(1.0f,0.0f,0.0f)  //Right face
    };

    public static readonly Vector2[] voxelUVs = new Vector2[4]
    {
        new Vector2(0.0f,0.0f),
        new Vector2(0.0f,1.0f),
        new Vector2(1.0f,0.0f),
        new Vector2(1.0f,1.0f)
    };
}
