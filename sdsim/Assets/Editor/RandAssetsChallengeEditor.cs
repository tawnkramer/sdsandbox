using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(RandAssetsChallenge))]
public class RandAssetsChallengeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RandAssetsChallenge challenge = (RandAssetsChallenge)target;

        if (GUILayout.Button("Reset Challenge"))
        {
            challenge.ResetChallenge();
        }
    }
}