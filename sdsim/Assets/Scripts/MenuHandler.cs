using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

public class MenuHandler : MonoBehaviour {

	public GameObject PIDContoller;
	public GameObject Logger;
	public GameObject NetworkSteering;
	public GameObject menuPanel;
	public GameObject stopPanel;
    public GameObject carJSControl;

    public TrainingManager trainingManager;

    public void Awake()
    {
        //keep it processing even when not in focus.
        Application.runInBackground = true;

        //Set desired frame rate as high as possible.
        Application.targetFrameRate = 60;

        stopPanel.SetActive(false);
    }

	public void OnPidGenerateTrainingData()
	{
		if(PIDContoller != null)
			PIDContoller.SetActive(true);

		if(carJSControl != null)
			carJSControl.SetActive(false);
	
		Logger.SetActive(true);
		menuPanel.SetActive(false);
        stopPanel.SetActive(true);
    }

	public void OnManualGenerateTrainingData()
	{
		if(PIDContoller != null)
			PIDContoller.SetActive(false);

		if(carJSControl != null)
			carJSControl.SetActive(true);
	
		Logger.SetActive(true);
		menuPanel.SetActive(false);
        stopPanel.SetActive(true);
    }

	public void OnUseNNNetworkSteering()
	{
		if(carJSControl != null)
			carJSControl.SetActive(false);
		
		NetworkSteering.SetActive(true);
		menuPanel.SetActive(false);
        stopPanel.SetActive(true);
    }

	public void OnPidDrive()
	{
		if(PIDContoller != null)
			PIDContoller.SetActive(true);

		if(carJSControl != null)
			carJSControl.SetActive(false);

		menuPanel.SetActive(false);
        stopPanel.SetActive(true);
    }

	public void OnManualDrive()
	{
		if(PIDContoller != null)
			PIDContoller.SetActive(false);

		if(carJSControl != null)
			carJSControl.SetActive(true);

		menuPanel.SetActive(false);
        stopPanel.SetActive(true);
    }

    public void OnNextTrack()
	{
		if(trainingManager != null)
			trainingManager.OnMenuNextTrack();
    }

    public void OnRegenTrack()
	{
		if(trainingManager != null)
			trainingManager.OnMenuRegenTrack();
    }

    public void OnStop()
    {
        if (PIDContoller != null)
            PIDContoller.SetActive(false);

        if (carJSControl != null)
            carJSControl.SetActive(false);

        Logger.SetActive(false);
        NetworkSteering.SetActive(false);


        menuPanel.SetActive(true);
        stopPanel.SetActive(false);
    }

}
