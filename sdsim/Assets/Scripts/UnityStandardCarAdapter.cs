using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class UnityStandardCarAdapter : MonoBehaviour, ICar {

	public UnityStandardAssets.Vehicles.Car.CarController unityCar;
	public float MaximumSteerAngle = 25.0f; //has to be kept in sync with the car, as that's a private var.
	float steering = 0.0f;
	float throttle = 0.0f;
	float footBrake = 0.0f;
	float handBrake = 0.0f;
	Vector3 vel = Vector3.zero;
	Vector3 accel = Vector3.zero;
	public string activity = "keep_lane";

	Rigidbody rb;

	public Vector3 startPos;
	public Quaternion startRot;

	void Awake()
	{
		rb = unityCar.GetComponent<Rigidbody>();
		SavePosRot();
	}

	//all inputs require 0-1 input except steering which is in degrees, where 0 is center.
	public void RequestThrottle(float val) { throttle = val; }

	public void RequestSteering(float val) { steering = val; }

	public void RequestFootBrake(float val) { footBrake = val; }

	public void RequestHandBrake(float val) { handBrake = val; }


	//query last input given.
	public float GetSteering() { return steering; }

	public float GetThrottle() { return throttle; }

	public float GetFootBrake() { return footBrake; }

	public float GetHandBrake() { return handBrake; }


	//query state.
	public Transform GetTransform() { return this.transform; }

	public Vector3 GetVelocity()
	{
		return rb.velocity;
	}

	public Vector3 GetAccel() { return accel; }


	//Save and restore State
	public void SavePosRot() 
	{ 
		startPos = transform.position;
		startRot = transform.rotation;
	}

	public void RestorePosRot()
	{
		Set(startPos, startRot);
	}

	public void Set(Vector3 pos, Quaternion rot)
	{
		rb.position = pos;
		rb.rotation = rot;

		//just setting it once doesn't seem to work. Try setting it multiple times..
		StartCoroutine(KeepSetting(pos, rot, 10));
	}

	IEnumerator KeepSetting(Vector3 pos, Quaternion rot, int numIter)
	{
		while(numIter > 0)
		{
			rb.position = pos;
			rb.rotation = rot;
			transform.position = pos;
			transform.rotation = rot;
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;

			numIter--;
			yield return new WaitForFixedUpdate();
		}
	}

	private void FixedUpdate()
	{
		accel = rb.velocity - vel;
		vel = rb.velocity;

		unityCar.Move(steering / MaximumSteerAngle, throttle, footBrake, handBrake);
	}

	public string GetActivity()
	{
		return activity;
	}

	public void SetActivity(string act)
	{
		activity = act;
	}
}
