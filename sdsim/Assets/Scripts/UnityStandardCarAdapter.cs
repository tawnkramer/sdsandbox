using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityStandardCarAdapter : MonoBehaviour, ICar {

	public UnityStandardAssets.Vehicles.Car.CarController unityCar;
	public float MaximumSteerAngle = 25.0f; //has to be kept in sync with the car, as that's a private var.
	float steering = 0.0f;
	float throttle = 0.0f;
	float footBrake = 0.0f;
	float handBrake = 0.0f;
	Vector3 vel = Vector3.zero;
	Vector3 accel = Vector3.zero;
	public string activity_prefix = "lane_keeping";

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
		
		unityCar.Move(steering / MaximumSteerAngle, throttle, footBrake, handBrake);
	}

	//get the image prefix to label the current activity of the car when logging.
	public string GetActivityPrefix()
	{
		return activity_prefix;
	}

	public void SetActivityPrefix(string prefix)
	{
		activity_prefix = prefix;
	}

}
