using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

public class track_maker : MonoBehaviour {

	public GameObject hayPrefab;

	public int numStraightSeg = 0;
	public float offset = 1.0f;
	public float curveYDeg = 5.0f;
	public int numCurve = 0;

	public Transform startTm;
	public Transform root;
	public bool Rotate180Y = false;

	#if UNITY_EDITOR
	[MenuItem("CONTEXT/track_maker/CreateTrackInEditor")]
    static void CreateTrackInEditor(MenuCommand command)
    {
        track_maker go = (track_maker)command.context;
        go.CreateTrack();
    }
	#endif

	void Start () 
	{
		CreateTrack();
	}

    void RegUndoObj(GameObject go)
    {
        #if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(go, "track object");
        #endif
    }

	// Use this for initialization
	public void CreateTrack() 
	{		
		for(int i = 0; i < numStraightSeg; i++)
		{
			GameObject go = GameObject.Instantiate(hayPrefab) as GameObject;

			go.transform.parent = root.transform;

			go.transform.localScale = hayPrefab.transform.localScale;
			go.transform.rotation = startTm.rotation;
			go.transform.position = startTm.position;

			if(Rotate180Y)
				go.transform.Rotate(0, 180, 0);

			startTm.Translate(offset + hayPrefab.transform.localScale.x, 0, 0);	
		}

		for(int i = 0; i < numCurve; i++)
		{
			GameObject go = GameObject.Instantiate(hayPrefab) as GameObject;

			RegUndoObj(go);

			go.transform.parent = root.transform;

			go.transform.localScale = hayPrefab.transform.localScale;
			go.transform.rotation = startTm.rotation;
			go.transform.position = startTm.position;

			if(Rotate180Y)
				go.transform.Rotate(0, 180, 0);

			startTm.Rotate(0, curveYDeg, 0);	
			startTm.Translate(offset + hayPrefab.transform.localScale.x, 0, 0);	
		}

		for(int i = 0; i < numStraightSeg; i++)
		{
			GameObject go = GameObject.Instantiate(hayPrefab) as GameObject;

			RegUndoObj(go);

			go.transform.parent = root.transform;

			go.transform.localScale = hayPrefab.transform.localScale;
			go.transform.rotation = startTm.rotation;
			go.transform.position = startTm.position;

			if(Rotate180Y)
				go.transform.Rotate(0, 180, 0);

			startTm.Translate(offset + hayPrefab.transform.localScale.x, 0, 0);	
		}
	}	
	
}
