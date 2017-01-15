using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPusher : MonoBehaviour {

	public Car car;
	public float amountToPushSideways = 1.0f;
	public float amountToRotate = 10.0f;
	public float durationTillNextPush = 3.0f;
	public bool doPush = true;
	public bool doRotate = true;

	float timer = 0.0f;

	// Use this for initialization
	void Start () {
		timer = durationTillNextPush - 1.0f;
	}

	void PushCar()
	{
		Vector3 sideVec = car.gameObject.transform.right.normalized;
		float randScale = Random.Range( -1f * amountToPushSideways, amountToPushSideways);
		car.transform.position = car.transform.position + (sideVec * randScale);
	}

	void RotateCar()
	{
		float randRot = Random.Range( -1f * amountToRotate, amountToRotate);
		Quaternion rotY = Quaternion.Euler(0.0f, randRot, 0.0f);
		car.transform.rotation = car.transform.rotation * rotY;
	}
	
	// Update is called once per frame
	void Update () 
	{
		timer += Time.deltaTime;

		if(timer > durationTillNextPush)
		{
			timer = 0.0f;

			if(doPush)
				PushCar();

			if(doRotate)
				RotateCar();
		}		
	}
}
