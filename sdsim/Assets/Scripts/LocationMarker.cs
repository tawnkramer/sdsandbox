using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationMarker : MonoBehaviour {

	public int id;

	public static int GetNearestLocMarker(Vector3 pos)
	{
		int closest = -1;
		float dist = float.PositiveInfinity;

		var markers = GameObject.FindObjectsOfType<LocationMarker>();

		foreach(var marker in markers)
		{
			float d = (marker.transform.position - pos).magnitude;
			if(d < dist)
			{
				closest = marker.id;
				dist = d;
			}
		}

		return closest;
	}
}

