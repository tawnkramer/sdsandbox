using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

public class MenuHandler : MonoBehaviour {

	public GameObject PIDContoller;
	public GameObject Logger;
	public GameObject NetworkSteering;
	public GameObject menuPanel;
	public UnityStandardCarAdapter carAdapter;

	public void OnGenerateTrainingData()
	{
		if(PIDContoller != null)
			PIDContoller.SetActive(true);

		if(carAdapter != null)
			carAdapter.userInputs = true;
	
		Logger.SetActive(true);
		menuPanel.SetActive(false);
	}

	public void OnUseNNNetworkSteering()
	{
		if(carAdapter != null)
			carAdapter.userInputs = false;
		
		NetworkSteering.SetActive(true);
		menuPanel.SetActive(false);
	}

	public void OnJustDrive()
	{
		if(PIDContoller != null)
			PIDContoller.SetActive(true);

		if(carAdapter != null)
			carAdapter.userInputs = true;

		menuPanel.SetActive(false);
	}

}
