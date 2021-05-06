using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChunkManager))]
public class NoiseDisplay : Editor
{
    public override void OnInspectorGUI()
    {
        ChunkManager chunkMan = (ChunkManager)target;

        if (DrawDefaultInspector())
        {
            if (chunkMan.autoUpdate)
            {
                chunkMan.DrawMapInEditor();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            chunkMan.DrawMapInEditor();
        }
    }
}
