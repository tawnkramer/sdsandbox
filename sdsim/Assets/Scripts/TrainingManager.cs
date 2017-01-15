using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingManager : MonoBehaviour {

	public PIDController controller;
	public Car car;
	public Logger logger;

	public int numTrainingRuns = 1;
	int iRun = 0;

	// Use this for initialization
	void Start () 
	{
		controller.endOfPathCB += new PIDController.OnEndOfPathCB(OnPathDone);
	}

	void StartNewRun()
	{
		car.ResetToStart();
		controller.pm.DestroyRoad();
		controller.pm.InitNewRoad();
		controller.StartDriving();
	}

	void OnLastRunCompleted()
	{
		car.Brake();
		logger.Shutdown();
	}

	void OnPathDone()
	{
		iRun += 1;

		if(iRun >= numTrainingRuns)
		{
			OnLastRunCompleted();
		}
		else
		{
			StartNewRun();
		}
	}

	void Update()
	{
		//watch the car and if we fall off the road, reset things.
		if(car.transform.position.y < -1.0f)
		{
			OnPathDone();
		}
	}

}
