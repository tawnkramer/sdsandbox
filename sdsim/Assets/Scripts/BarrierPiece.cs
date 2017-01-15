using UnityEngine;
using System.Collections;

public class BarrierPiece : MonoBehaviour {

	MeshRenderer mr;

	public bool hasBeenHit = false;

	// Use this for initialization
	void Start () 
	{
		mr = GetComponent<MeshRenderer>();
		mr.enabled = false;
		hasBeenHit = false;
	}

	public void OnSensorDetected()
	{
		mr.enabled = true;
		hasBeenHit = true;
	}
}
