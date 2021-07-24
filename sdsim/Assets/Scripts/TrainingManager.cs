﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingManager : MonoBehaviour
{

    public PIDController controller;
    public GameObject carObj;
    public ICar car;
    public Logger logger;
    public RoadBuilder roadBuilder;

    public Camera overheadCamera;

    public PathManager pathManager;
    public CarSpawner carSpawner;

    public int numTrainingRuns = 1;
    int iRun = 0;


    void LinkObj()
    {
        car = carObj.GetComponent<ICar>();
        if (car == null)
            Debug.LogError("TrainingManager needs car object");

        roadBuilder = GameObject.FindObjectOfType<RoadBuilder>();
        pathManager = GameObject.FindObjectOfType<PathManager>();
		carSpawner = GameObject.FindObjectOfType<CarSpawner>();
    }

    // Use this for initialization
    void Start()
    {
        LinkObj();
        controller.endOfPathCB += new PIDController.OnEndOfPathCB(OnPathDone);
    }

    public void SetRoadStyle(int style)
    {
        iRun = style;
    }

    void SwapRoadToNewTextureVariation()
    {
        if (roadBuilder == null)
            return;

        roadBuilder.SetNewRoadVariation(iRun);
    }

    void StartNewRun()
    {
        car.RestorePosRot();
        roadBuilder.DestroyRoad();
        SwapRoadToNewTextureVariation();
        pathManager.InitCarPath();
        controller.StartDriving();
        RepositionOverheadCamera();
    }

    public void RepositionOverheadCamera()
    {
        if (carSpawner == null)
            return;

        if (GlobalState.overheadCamera)
        {
            GameObject OHCamGo = carSpawner.cameras[0];
            OverHeadCamera overheadCamera = OHCamGo.GetComponent<OverHeadCamera>();
            overheadCamera.Init();
        }
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

        if (iRun >= numTrainingRuns)
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

        if (iRun >= numTrainingRuns)
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
        if (car == null)
            return;

        //watch the car and if we fall off the road, reset things.
        if (car.GetTransform().position.y < -1.0f)
        {
            OnPathDone();
        }

        if (logger.frameCounter + 1 % 1000 == 0)
        {
            //swap road texture left to right. or Y
            roadBuilder.NegateYTiling();
        }
    }

}
