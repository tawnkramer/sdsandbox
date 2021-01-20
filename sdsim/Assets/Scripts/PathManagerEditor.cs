using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(PathManager))]
public class PathManagerEditor : Editor
{
    public string savepath = "Assets\\generated_mesh.asset";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PathManager pathManager = (PathManager)target;
        savepath = GUILayout.TextField(savepath);

        if (GUILayout.Button("Save Mesh"))
        {
            pathManager.SaveRoadMesh(savepath);
        }
    }
}