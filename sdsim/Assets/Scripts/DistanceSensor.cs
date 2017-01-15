using UnityEngine;
using System.Collections;

public class DistanceSensor : MonoBehaviour 
{

	public float maxRange = 10f;
	public float updateRatePerSecond = 1f;//once per second.
	float senseTimer = 0f;
	// Use this for initialization

	public float lastDistSensed = 0.0f;
	public string lastObjectSensed = "none";
	public Transform sensorIndicator;
	Vector3 sensorIndicatorStart;

	public delegate void OnNewBarrierCB();

	public OnNewBarrierCB onDetectCB;

	void Awake()
	{
		sensorIndicatorStart = sensorIndicator.localPosition;
		lastDistSensed = maxRange;
	}

	// Update is called once per frame
	void Update () 
	{
		senseTimer += Time.deltaTime;

		if(senseTimer > (1f / updateRatePerSecond))
		{
			senseTimer = 0.0f;
			Sense();
		}
	}

	void Sense()
	{
		Vector3 origin = transform.position;
		origin += transform.forward * 0.1f;

		RaycastHit hitInfo;
		
		if(Physics.Raycast(origin, transform.forward * -1f, out hitInfo, maxRange))
		{
			lastObjectSensed = hitInfo.collider.gameObject.tag;
			lastDistSensed = hitInfo.distance;

			BarrierPiece b = hitInfo.collider.gameObject.GetComponent<BarrierPiece>();

			if(b != null)
			{
				if(!b.hasBeenHit && onDetectCB != null)
					onDetectCB.Invoke();

				b.OnSensorDetected();
			}

			sensorIndicator.position = hitInfo.point;
		}
		else
		{
			sensorIndicator.localPosition = sensorIndicatorStart;
			lastObjectSensed = "none";
			lastDistSensed = maxRange;
			
		}
	}

}
