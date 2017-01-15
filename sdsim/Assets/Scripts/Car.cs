using UnityEngine;
using System.Collections;


public class Car : MonoBehaviour {

	public WheelCollider[] wheelColliders;
	public Transform[] wheelMeshes;

	public float maxTorque = 50f;
	public float maxSpeed = 10f;

	public Transform centrOfMass;

	public float requestTorque = 0f;
	public float requestBrake = 0f;
	public float requestSteering = 0f;

	public Vector3 acceleration = Vector3.zero;
	public Vector3 prevVel = Vector3.zero;

	public Vector3 startPos;
	public Quaternion startRot;
	Vector3 startPosOffset = new Vector3(0, 1, 0);

	public float length = 1.7f;

	Rigidbody rb;

	//for logging
	public float lastSteer = 0.0f;
	public float lastAccel = 0.0f;

	// Use this for initialization
	void Awake () 
	{
		rb = GetComponent<Rigidbody>();

		if(rb && centrOfMass)
		{
			rb.centerOfMass = centrOfMass.localPosition;
		}

		requestTorque = 0f;
		requestSteering = 0f;

		SetStart();
	}

	public void SetStart()
	{
		startPos = transform.position;
		startRot = transform.rotation;
	}

	public void ResetToStart()
	{
		Set(startPos + startPosOffset, startRot);
	}

	public void RequestThrottle(float val)
	{
		requestTorque = val;
		requestBrake = 0f;
	}

	public void RequestSteering(float val)
	{
		requestSteering = val;

		//steering limits of car.
		if(requestSteering > 45f)
			requestSteering = 45f;
		else if(requestSteering < -45f)
			requestSteering = -45f;
	}

	public void Set(Vector3 pos, Quaternion rot)
	{
		rb.position = pos;
		rb.rotation = rot;

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

			numIter--;
			yield return new WaitForFixedUpdate();
		}
	}

	public Vector3 Velocity()
	{
		return rb.velocity;
	}

	public Vector3 Accel()
	{
		return acceleration;
	}

	public float GetOrient ()
	{
		Vector3 dir = transform.forward;
		return Mathf.Atan2( dir.z, dir.x);
	}

	public bool IsStill()
	{
		return rb.IsSleeping();
	}

	public void Brake()
	{
		requestBrake = 1.0f;
	}
	
	// Update is called once per frame
	void Update () {
	
		UpdateWheelPositions();
	}


	void FixedUpdate()
	{
		float accel = Input.GetAxis ("Vertical");
		float steer = Input.GetAxis("Horizontal") * 15.0f;
		float brake = 0.0f;
	
		lastSteer = steer;
		lastAccel = accel;

		float throttle = accel * maxTorque;

		if(accel == 0.0f)
		{
			throttle = requestTorque * maxTorque;
			requestTorque = 0.0f;
		}
		else
		{
			requestTorque = accel; //so we can log it.
		}

		if(steer == 0.0f)
		{
			steer = requestSteering;
		}
		else
		{
			requestSteering = steer; //so that we can log it.
		}

		float steerAngle = steer;

		if(brake == 0.0f)
		{
			brake = requestBrake;
			requestBrake = 0.0f;
		}

		//front two tires.
		wheelColliders[2].steerAngle = steerAngle;
		wheelColliders[3].steerAngle = steerAngle;

		//four wheel drive at the moment
		foreach(WheelCollider wc in wheelColliders)
		{
			if(rb.velocity.magnitude < maxSpeed)
			{
				wc.motorTorque = throttle;
			}
			else
			{
				wc.motorTorque = 0.0f;
			}

			wc.brakeTorque = 400f * brake;
		}

		acceleration = rb.velocity - prevVel;
	}

	void FlipUpright()
	{
		Quaternion rot = Quaternion.Euler(180f, 0f, 0f);
		this.transform.rotation = transform.rotation * rot;
		transform.position = transform.position + Vector3.up * 2;
	}

	void UpdateWheelPositions()
	{
		Quaternion rot;
		Vector3 pos;

		for(int i = 0; i < wheelColliders.Length; i++)
		{
			WheelCollider wc = wheelColliders[i];
			Transform tm = wheelMeshes[i];

			wc.GetWorldPose(out pos, out rot);

			tm.position = pos;
			tm.rotation = rot;
		}
	}
}
