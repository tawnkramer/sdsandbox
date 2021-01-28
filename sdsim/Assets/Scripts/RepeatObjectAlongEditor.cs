using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(RepeatObjectAlong))]
public class RepeatObjectAlongEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RepeatObjectAlong repeatObjectAlong = (RepeatObjectAlong)target;

        if (GUILayout.Button("Generate Mesh (only at runtime)"))
        {
            repeatObjectAlong.Generate();
        }

        if (GUILayout.Button("Save Mesh"))
        {
            repeatObjectAlong.SaveMesh();
        }
    }
}