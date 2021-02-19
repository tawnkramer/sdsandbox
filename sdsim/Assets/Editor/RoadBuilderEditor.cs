using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(RoadBuilder))]
public class RoadBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoadBuilder roadBuilder = (RoadBuilder)target;

        if (GUILayout.Button("Save Mesh") && roadBuilder.createdRoad != null)
        {
            MeshFilter mf = roadBuilder.createdRoad.GetComponent<MeshFilter>();
            AssetDatabase.CreateAsset(mf.mesh, roadBuilder.savePath);
        }
    }
}