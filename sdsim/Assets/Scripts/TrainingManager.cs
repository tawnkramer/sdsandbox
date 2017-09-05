using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingManager : MonoBehaviour {

	public PIDController controller;
	public GameObject carObj;
	public ICar car;
	public Logger logger;
	public RoadBuilder roadBuilder;

    public Camera overheadCamera;

    public PathManager pathManager;

	public int numTrainingRuns = 1;
	int iRun = 0;

	void Awake()
	{
		car = carObj.GetComponent<ICar>();
	}

	// Use this for initialization
	void Start () 
	{
		controller.endOfPathCB += new PIDController.OnEndOfPathCB(OnPathDone);

        RepositionOverheadCamera();
	}

	void SwapRoadToNewTextureVariation()
	{
		if(roadBuilder == null)
			return;

		roadBuilder.SetNewRoadVariation(iRun);
	}

	void StartNewRun()
	{
		car.RestorePosRot();
		controller.pm.DestroyRoad();
		SwapRoadToNewTextureVariation();
		controller.pm.InitNewRoad();
		controller.StartDriving();
        RepositionOverheadCamera();
	}

    public void RepositionOverheadCamera()
    {
        if(overheadCamera == null)
            return;

        Vector3 pathStart = pathManager.GetPathStart();
        Vector3 pathEnd = pathManager.GetPathEnd();
        Vector3 avg = (pathStart + pathEnd) / 2.0f;
        avg.y = overheadCamera.transform.position.y;
        overheadCamera.transform.position = avg;
    }


	void OnLastRunCompleted()
	{
		car.RequestFootBrake(1.0f);
		controller.StopDriving();
		logger.Shutdown();
	}

    public void OnMenuNextTrack()
    {
        iRun += 1;

		if(iRun >= numTrainingRuns)
            iRun = 0;

        StartNewRun();
        car.RequestFootBrake(1);
    }

    public void OnMenuRegenTrack()
    {
        StartNewRun();
        car.RequestFootBrake(1);
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
		if(car.GetTransform().position.y < -1.0f)
		{
			OnPathDone();
		}

		if(logger.frameCounter + 1 % 1000 == 0)
		{
			//swap road texture left to right. or Y
			roadBuilder.NegateYTiling();
		}
	}

}
