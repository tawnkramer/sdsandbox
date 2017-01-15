using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHandler : MonoBehaviour {

	public GameObject PIDContoller;
	public GameObject Logger;
	public GameObject NetworkSteering;
	public GameObject menuPanel;

	public void OnGenerateTrainingData()
	{
		PIDContoller.SetActive(true);
		Logger.SetActive(true);
		menuPanel.SetActive(false);
	}

	public void OnUseNNNetworkSteering()
	{
		NetworkSteering.SetActive(true);
		menuPanel.SetActive(false);
	}

	public void OnJustDrive()
	{
		PIDContoller.SetActive(true);
		menuPanel.SetActive(false);
	}
}
