using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PIDController : MonoBehaviour {

	public Car car;
	public PathManager pm;

	float errA, errB;
	public float Kp = 10.0f;
	public float Kd = 10.0f;
	public float Ki = 0.1f;

	//Ks is the proportion of the current vel that
	//we use to sample ahead of the vehicles actual position.
	public float Kv = 1.0f; 

	//Ks is the proportion of the current err that
	//we use to change throtlle.
	public float Kt = 1.0f; 

	float diffErr = 0f;
	public float prevErr = 0f;
	public float steeringReq = 0.0f;
	public float throttleVal = 0.3f;
	public float totalError = 0f;
	public float absTotalError = 0f;
	public float totalAcc = 0f;
	public float totalOscilation = 0f;
	public float AccelErrFactor = 0.1f;
	public float OscilErrFactor = 10f;

	public delegate void OnEndOfPathCB();

	public OnEndOfPathCB endOfPathCB;

	public Transform samplePosMarker;

	bool isDriving = false;
	public bool waitForStill = true;

	public bool startOnWake = false;

	public bool brakeOnEnd = true;

	public bool doDrive = true;

	public Text pid_steering;

	void Start()
	{
		if(startOnWake)
			StartDriving();
	}

	public void StartDriving()
	{
		if(!pm.isActiveAndEnabled || pm.path == null)
			return;

		steeringReq = 0f;
		prevErr = 0f;
		totalError = 0f;
		totalAcc = 0f;
		totalOscilation = 0f;
		absTotalError = 0f;


		pm.path.ResetActiveSpan();
		isDriving = true;
		waitForStill = false;//true;

		if(!waitForStill && doDrive)
			car.RequestThrottle(throttleVal);

		car.SetStart();
	}
		
	// Update is called once per frame
	void Update () 
	{
		if(!pm.isActiveAndEnabled)
			return;

		if(!isDriving)
			return;

		if(waitForStill)
		{
			car.Brake();

			if(car.Accel().magnitude < 0.001f)
			{
				waitForStill = false;

				if(doDrive)
					car.RequestThrottle(throttleVal);
			}
			else
			{
				//don't continue until we've settled.
				return;
			}
		}

		float err = 0.0f;

		float velMag = car.Velocity().magnitude;

		Vector3 samplePos = car.transform.position + (car.transform.forward * velMag * Kv);

		samplePosMarker.position = samplePos;

		if(!pm.path.GetCrossTrackErr(samplePos, ref err))
		{
			if(brakeOnEnd)
			{
				car.Brake();

				if(car.Accel().magnitude < 0.0001f)
				{
					isDriving = false;

					if(endOfPathCB != null)
						endOfPathCB.Invoke();
				}
			}
			else
			{
				isDriving = false;
				
				if(endOfPathCB != null)
					endOfPathCB.Invoke();
			}

			return;
		}

		diffErr = err - prevErr;

		steeringReq = (-Kp * err) - (Kd * diffErr) - (Ki * totalError);

		if(doDrive)
			car.RequestSteering(steeringReq);

		if(doDrive)
			car.RequestThrottle(throttleVal);
		
		if(pid_steering != null)
			pid_steering.text = string.Format("PID: {0}", steeringReq);

		//accumulate total error
		totalError += err;

		//save err for next iteration.
		prevErr = err;

		float carPosErr = 0.0f;

		//accumulate error at car, not steering decision point.
		pm.path.GetCrossTrackErr(car.transform.position, ref carPosErr);


		//now get a measure of overall fitness.
		//we don't with this to cancel out when it oscilates.
		absTotalError += Mathf.Abs(carPosErr) + 
		                 AccelErrFactor * car.Accel().magnitude;

	}
}
