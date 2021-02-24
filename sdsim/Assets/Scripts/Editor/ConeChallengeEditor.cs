using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(ConeChallenge))]
public class ConeChallengeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ConeChallenge challenge = (ConeChallenge)target;

        if (GUILayout.Button("Reset Challenge"))
        {
            challenge.ResetChallenge();
        }
    }
}