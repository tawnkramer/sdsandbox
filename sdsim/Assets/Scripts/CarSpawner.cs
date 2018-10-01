using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour {

	public GameObject carPrefab;
	public Transform startTm;
    public bool EnableTrainingManager = false;

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

        Debug.LogError("couldn't find: " + withName);
         return null;
     }

	public void Spawn () 
	{
        //Create a car object, and also hook up all the connections
        //to various places in game that need to hook into the car.
		GameObject go = GameObject.Instantiate(carPrefab) as GameObject;

		go.transform.rotation = startTm.rotation;
		go.transform.position = startTm.position;
        go.GetComponent<Car>().SavePosRot();


        if (OnNewCarCB != null)
			OnNewCarCB.Invoke(go);

        ///////////////////////////////////////////////
        //Search scene to find these.
        CameraFollow cameraFollow = GameObject.FindObjectOfType<CameraFollow>();
        MenuHandler menuHandler = GameObject.FindObjectOfType<MenuHandler>();
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        PID_UI pid_ui = getChildGameObject(canvas.gameObject, "PIDPanel").GetComponent<PID_UI>();
        ///////////////////////////////////////////////


        //set camera target follow tm
        if (cameraFollow != null)
			cameraFollow.target = getChildGameObject(go, "CameraFollowTm").transform;

        //Set menu handler hooks
		if(menuHandler != null)
		{
			menuHandler.PIDContoller = getChildGameObject(go, "PIDController");
			menuHandler.Logger = getChildGameObject(go, "Logger");
			menuHandler.NetworkSteering = getChildGameObject(go, "TCPClient");
			menuHandler.carJSControl = getChildGameObject(go, "JoyStickCarContoller");
			menuHandler.trainingManager = getChildGameObject(go, "TrainingManager").GetComponent<TrainingManager>();
            menuHandler.trainingManager.carObj = go;

            if (EnableTrainingManager)
            {
                menuHandler.trainingManager.gameObject.SetActive(true);

                getChildGameObject(go, "OverheadViewSphere").SetActive(true);
            }

            if (GlobalState.bAutoConnectToWebSocket && menuHandler.NetworkSteering  != null)
			{
				menuHandler.NetworkSteering.SetActive(true);
			}

		}
		else
		{
			Debug.LogError("need menu handler");
		}

        //Set the PID ui hooks
		if (pid_ui != null)
		{
			pid_ui.pid = getChildGameObject(go, "PIDController").GetComponent<PIDController>();
			pid_ui.logger = getChildGameObject(go, "Logger").GetComponent<Logger>();
		}
		else
		{
			Debug.LogError("failed to find PID_UI");
		}
	}
	
}
