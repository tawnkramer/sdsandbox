using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tk;

public class CarSpawner : MonoBehaviour {

	public GameObject carPrefab;
	public Transform startTm;
    public bool EnableTrainingManager = false;

	public delegate void OnNewCar(GameObject carObj);

	public OnNewCar OnNewCarCB;	

	private string nnIPAddress = "";
	private string nnPort = "";

	public Camera splitScreenCamLeft;
	public Camera splitScreenCamRight;
	public GameObject splitScreenPanel;

	private List<GameObject> cars;

	public void CheckCommandLineConnectArgs()
	{
		string[] args = System.Environment.GetCommandLineArgs ();
		for (int i = 0; i < args.Length; i++) {
			if (args [i] == "--host") {
				nnIPAddress = args [i + 1];
			}
			else if (args [i] == "--port") {
				nnPort = args [i + 1];				
			}
		}
	}

	void Start()
	{
		cars = new List<GameObject>();
		//Spawn the first car auto-magically.
		CheckCommandLineConnectArgs();
		Spawn(Vector3.zero, nnIPAddress, nnPort);
	}

	static public GameObject getChildGameObject(GameObject fromGameObject, string withName) {
         //Author: Isaac Dart, June-13.
         Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
         foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;

        return null;
     }

	public void Spawn (Vector3 offset, string host="", string port="") 
	{
        //Create a car object, and also hook up all the connections
        //to various places in game that need to hook into the car.
		GameObject go = GameObject.Instantiate(carPrefab) as GameObject;

		cars.Add(go);

		if(cars.Count > 2)
		{
			//just stack more cars after the second. Not pretty.
			offset = offset + Vector3.forward * (-5f * (cars.Count - 2));
		}

		go.transform.rotation = startTm.rotation;
		go.transform.position = startTm.position + offset;		
        go.GetComponent<Car>().SavePosRot();

		GameObject TcpClientObj = getChildGameObject(go, "TCPClient");

		Camera cam = Camera.main;

		//Detect that we have the second car. Doesn't really handle more than 2 right now.
		if(cars.Count > 1)
		{
			if(splitScreenCamLeft != null)
			{
				splitScreenCamLeft.gameObject.SetActive(true);
				cam = splitScreenCamLeft;
			}

			if(splitScreenCamRight != null)
			{
				splitScreenCamRight.gameObject.SetActive(true);

				//would be better to render both to textures, but too much work.
				Transform camFollowRt = getChildGameObject(cars[0], "CameraFollowTm").transform;

				CameraFollow rtCameraFollow = splitScreenCamRight.transform.GetComponent<CameraFollow>();

				rtCameraFollow.target = camFollowRt;
			}

			if(splitScreenPanel)
				splitScreenPanel.SetActive(true);
		}

		if(TcpClientObj != null && host != "" && port != "")
		{
			//without this it will not connect.
			TcpClientObj.SetActive(true);

			//now set the connection settings. The jsonclient will attempt to connect
			//later in the update loop.
			JsonTcpClient jsonClient = TcpClientObj.GetComponent<JsonTcpClient>();
			if(jsonClient != null)
			{
				jsonClient.nnIPAddress = host;
				jsonClient.nnPort = int.Parse(port);
			}
		}

        if (OnNewCarCB != null)
			OnNewCarCB.Invoke(go);

        ///////////////////////////////////////////////
        //Search scene to find these.
        CameraFollow cameraFollow = cam.transform.GetComponent<CameraFollow>();
        MenuHandler menuHandler = GameObject.FindObjectOfType<MenuHandler>();
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        GameObject panelMenu = getChildGameObject(canvas.gameObject, "Panel Menu");
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

            if (GlobalState.bAutoHideSceneMenu && panelMenu != null)
            {
                panelMenu.SetActive(false);
            }

        }
		else
		{
			Debug.LogError("need menu handler");
		}        
	}
	
}
