using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(LightChallenge))]
public class LightChallengeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        LightChallenge challenge = (LightChallenge)target;

        if (GUILayout.Button("Reset Challenge"))
        {
            challenge.ResetChallenge();
        }
    }
}