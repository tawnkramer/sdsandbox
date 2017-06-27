using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

public class MenuHandler : MonoBehaviour {

	public GameObject PIDContoller;
	public GameObject Logger;
	public GameObject NetworkSteering;
	public GameObject menuPanel;
	public GameObject carJSControl;

	public void OnPidGenerateTrainingData()
	{
		if(PIDContoller != null)
			PIDContoller.SetActive(true);

		if(carJSControl != null)
			carJSControl.SetActive(false);
	
		Logger.SetActive(true);
		menuPanel.SetActive(false);
	}

	public void OnManualGenerateTrainingData()
	{
		if(PIDContoller != null)
			PIDContoller.SetActive(false);

		if(carJSControl != null)
			carJSControl.SetActive(true);
	
		Logger.SetActive(true);
		menuPanel.SetActive(false);
	}

	public void OnUseNNNetworkSteering()
	{
		if(carJSControl != null)
			carJSControl.SetActive(false);
		
		NetworkSteering.SetActive(true);
		menuPanel.SetActive(false);
	}

	public void OnPidDrive()
	{
		if(PIDContoller != null)
			PIDContoller.SetActive(true);

		if(carJSControl != null)
			carJSControl.SetActive(false);

		menuPanel.SetActive(false);
	}

	public void OnManualDrive()
	{
		if(PIDContoller != null)
			PIDContoller.SetActive(false);

		if(carJSControl != null)
			carJSControl.SetActive(true);

		menuPanel.SetActive(false);
	}

}
