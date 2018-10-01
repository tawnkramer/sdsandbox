using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour {

	public GameObject carPrefab;
	public Transform startTm;

	public CameraFollow cameraFollow;
	public MenuHandler menuHandler;
	public PID_UI pid_ui;
	public delegate void OnNewCar(GameObject carObj);

	public OnNewCar OnNewCarCB;	

	void Start()
	{
		Spawn();
	}

	static public GameObject getChildGameObject(GameObject fromGameObject, string withName) {
         //Author: Isaac Dart, June-13.
         Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
         foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
         return null;
     }

	public void Spawn () 
	{
		GameObject go = GameObject.Instantiate(carPrefab) as GameObject;

		go.transform.rotation = startTm.rotation;
		go.transform.position = startTm.position;

		if(OnNewCarCB != null)
			OnNewCarCB.Invoke(go);

		if(cameraFollow != null)
			cameraFollow.target = getChildGameObject(go, "CameraFollowTm").transform;

		if(menuHandler != null)
		{
			menuHandler.PIDContoller = getChildGameObject(go, "PIDController");
			menuHandler.Logger = getChildGameObject(go, "Logger");
			menuHandler.NetworkSteering = getChildGameObject(go, "TCPClient");
			menuHandler.carJSControl = getChildGameObject(go, "JoyStickCarContoller");
			menuHandler.trainingManager = getChildGameObject(go, "TrainingManager").GetComponent<TrainingManager>();

			if(GlobalState.bAutoConnectToWebSocket && menuHandler.NetworkSteering  != null)
			{
				menuHandler.NetworkSteering.SetActive(true);
			}
		}
		else
		{
			Debug.LogError("need menu handler");
		}

		if (pid_ui != null)
		{
			pid_ui.pid = getChildGameObject(go, "PIDController").GetComponent<PIDController>();
			pid_ui.logger = getChildGameObject(go, "Logger").GetComponent<Logger>();
		}
		else
		{
			Debug.LogError("failed tp find PID_UI");
		}

	}
	
}
