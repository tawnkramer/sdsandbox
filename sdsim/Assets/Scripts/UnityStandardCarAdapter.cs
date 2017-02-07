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

	public bool userInputs = false;

	Rigidbody rb;

	void Awake()
	{
		rb = unityCar.GetComponent<Rigidbody>();
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

	public Vector3 GetVelocity() { return vel; }

	public Vector3 GetAccel() { return accel; }


	//Save and restore State
	public void SavePosRot() 
	{ 
		//todo
	}

	public void RestorePosRot()
	{
		//todo
	}

	private void FixedUpdate()
	{
		accel = rb.velocity - vel;
		vel = rb.velocity;

		if(userInputs)
		{
			// pass the input to the car!
			float h = CrossPlatformInputManager.GetAxis("Horizontal");
			float v = CrossPlatformInputManager.GetAxis("Vertical");
			float handbrake = CrossPlatformInputManager.GetAxis("Jump");
			RequestSteering(h * MaximumSteerAngle);
			RequestThrottle(v);
			RequestFootBrake(v);
			RequestHandBrake(handbrake);
		}

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
