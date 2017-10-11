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
	public GameObject exitPanel;
    public GameObject carJSControl;
	public GameObject PIDControls;

    public TrainingManager trainingManager;

    public void Awake()
    {
        //keep it processing even when not in focus.
        Application.runInBackground = true;

        //Set desired frame rate as high as possible.
        Application.targetFrameRate = 60;

		menuPanel.SetActive(true);
        stopPanel.SetActive(false);
        exitPanel.SetActive(true);
    }

	public void OnPidGenerateTrainingData()
	{
		Logger.SetActive(true);
        
		if(PIDContoller != null)
			PIDContoller.SetActive(true);

		if(carJSControl != null)
			carJSControl.SetActive(false);

		if(PIDControls != null)
			PIDControls.SetActive(true);
	
		menuPanel.SetActive(false);
        stopPanel.SetActive(true);
        exitPanel.SetActive(false);
    }

	public void OnManualGenerateTrainingData()
	{
		Logger.SetActive(true);
        
		if(PIDContoller != null)
			PIDContoller.SetActive(false);

		if(carJSControl != null)
			carJSControl.SetActive(true);

		if(PIDControls != null)
			PIDControls.SetActive(false);
	
		menuPanel.SetActive(false);
        stopPanel.SetActive(true);
        exitPanel.SetActive(false);
    }

	public void OnUseNNNetworkSteering()
	{
		if(carJSControl != null)
			carJSControl.SetActive(false);

		if(PIDControls != null)
			PIDControls.SetActive(false);
		
		NetworkSteering.SetActive(true);
		menuPanel.SetActive(false);
        stopPanel.SetActive(true);
        exitPanel.SetActive(false);
    }

	public void OnPidDrive()
	{
		if(PIDContoller != null)
			PIDContoller.SetActive(true);

		if(carJSControl != null)
			carJSControl.SetActive(false);

		if(PIDControls != null)
			PIDControls.SetActive(true);

		menuPanel.SetActive(false);
        stopPanel.SetActive(true);
        exitPanel.SetActive(false);
    }

	public void OnManualDrive()
	{
		if(PIDContoller != null)
			PIDContoller.SetActive(false);

		if(carJSControl != null)
			carJSControl.SetActive(true);

		if(PIDControls != null)
			PIDControls.SetActive(false);

		menuPanel.SetActive(false);
        stopPanel.SetActive(true);
        exitPanel.SetActive(false);
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

		if(PIDControls != null)
			PIDControls.SetActive(false);

        Logger.SetActive(false);
        NetworkSteering.SetActive(false);


        menuPanel.SetActive(true);
        stopPanel.SetActive(false);
        exitPanel.SetActive(true);
    }

}
