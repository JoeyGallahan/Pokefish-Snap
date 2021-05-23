using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
 * Taken and modified from https://youtu.be/rQG9aUWarwE
 */

[CustomEditor (typeof(FieldOfView))]
public class FOVEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfView fov = (FieldOfView)target;
        Handles.color = Color.green;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360.0f, fov.viewRadius);
        Handles.color = Color.blue;
        Handles.DrawWireArc(fov.transform.position, Vector3.forward, Vector3.up, 360.0f, fov.viewRadius);
        Handles.color = Color.red;
        Handles.DrawWireArc(fov.transform.position, Vector3.left, Vector3.up, 360.0f, fov.viewRadius);

        Vector3 viewAngleA = fov.DirFromAngle(-fov.viewAngle / 2, false);
        Vector3 viewAngleB = fov.DirFromAngle(fov.viewAngle / 2, false);

        Handles.color = Color.white;
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + (Vector3.up * fov.viewRadius) + viewAngleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + (Vector3.up * fov.viewRadius) + viewAngleB * fov.viewRadius);

        Handles.color = Color.yellow;
        foreach(GameObject visibleTarget in fov.visibleTargets)
        {
            Handles.DrawLine(fov.transform.position, visibleTarget.transform.position);
        }

        Handles.color = Color.black;
        foreach (Vector3 visibleObstacle in fov.visibleObstacles)
        {
            Handles.DrawLine(fov.transform.position, visibleObstacle);
        }
    }

}