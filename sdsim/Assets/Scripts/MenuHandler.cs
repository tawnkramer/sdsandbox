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

        //auto link
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        menuPanel = getChildGameObject(canvas.gameObject, "Panel Menu");
        stopPanel = getChildGameObject(canvas.gameObject, "StopPanel");
        exitPanel = getChildGameObject(canvas.gameObject, "ExitPanel");
        PIDControls = getChildGameObject(canvas.gameObject, "PIDPanel");

        menuPanel.SetActive(true);
        stopPanel.SetActive(false);
        exitPanel.SetActive(true);
    }
    static public GameObject getChildGameObject(GameObject fromGameObject, string withName)
    {
        //Author: Isaac Dart, June-13.
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;

        Debug.LogError("Failed to find: " + withName);
        return null;
    }

    public void OnPidGenerateTrainingData()
	{
        if(Logger != null)
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
		if(Logger != null)
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
		
		if(NetworkSteering != null)    
            NetworkSteering.SetActive(true);

		menuPanel.SetActive(false);
        stopPanel.SetActive(true);
        exitPanel.SetActive(false);

        CarSpawner spawner = GameObject.FindObjectOfType<CarSpawner>();

        if (spawner)
            spawner.RemoveAllCars();
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

        if(Logger != null)
		    Logger.SetActive(false);

        if(NetworkSteering != null)    
            NetworkSteering.SetActive(false);

        menuPanel.SetActive(true);
        stopPanel.SetActive(false);
        exitPanel.SetActive(true);

        CarSpawner spawner = GameObject.FindObjectOfType<CarSpawner>();

        if (spawner)
            spawner.EnsureOneCar();
    }

}
