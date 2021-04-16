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
            MeshFilter mf = repeatObjectAlong.GetComponent<MeshFilter>();
            Mesh mesh = mf.sharedMesh;
            if (mesh == null)
            {
                Debug.LogWarning("Mesh is null, creating a new one");
                mesh = new Mesh();
            }
            AssetDatabase.CreateAsset(mesh, repeatObjectAlong.savePath);
        }
    }
}