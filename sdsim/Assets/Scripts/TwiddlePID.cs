using UnityEngine;
using System.Collections;

public class TwiddlePID : MonoBehaviour 
{
	public Twiddle twiddleEngine;
	public PIDController controller;
	public float[] initial_params;
	public float maxTimePerRun = 100f;

	class PIDPRoc : IProcedure
	{
		bool isDone;
		PIDController cont;
		Vector3 camPos;
		Quaternion camRot;

		float maxTimePerRun = 10f;
		float curTime = 0f;

		bool testTimedOut = false;
		float maxErr = 0f;

		public PIDPRoc(PIDController c, float mt)
		{
			isDone = false;
			cont = c;
			cont.endOfPathCB += OnPathDone;
			maxTimePerRun = mt;

			camPos = Camera.main.transform.position;
			camRot = Camera.main.transform.rotation;
		}

		public void StartTest (float[] param, float _maxErr)
		{
			isDone = false;
			curTime = 0f;
			testTimedOut = false;
			maxErr = _maxErr;

			cont.Kv = param[0];
			cont.Kp = param[1];
			cont.Kd = param[2];
			cont.Ki = param[3];
			cont.Kt = param[4];

			Camera.main.transform.position = camPos;
			Camera.main.transform.rotation = camRot;

			cont.car.RestorePosRot();
			cont.StartDriving();
		}
		
		public bool IsDone ()
		{
			return isDone || testTimedOut;
		}
		
		public float GetErr ()
		{
			float timeOutErr = testTimedOut ? 1000000f : 0f;

			return Mathf.Abs(cont.absTotalError) + timeOutErr;
		}

		public void OnPathDone()
		{
			isDone = true;
		}

		public void Update()
		{
			curTime += Time.deltaTime;

			if(curTime > maxTimePerRun && !isDone)
				testTimedOut = true;

			if(!isDone && GetErr() - 1f > maxErr)
				testTimedOut = true;
		}

	}

	PIDPRoc proc;
	
	void Start () 
	{
		StartTest();
	}

	void StartTest()
	{
		if (initial_params.Length != 5)
		{
			Debug.LogError("Must have three initial params");
			return;
		}

		proc = new PIDPRoc(controller, maxTimePerRun);

		twiddleEngine.StartTest(proc, initial_params);
	}

	void Update()
	{
		if(proc != null)
			proc.Update();
	}

}
