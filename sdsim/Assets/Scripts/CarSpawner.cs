using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tk;
using System;

public class CarSpawner : MonoBehaviour {

	public GameObject carPrefab;
	public Transform startTm;
    public bool EnableTrainingManager = false;

	public delegate void OnNewCar(GameObject carObj);
	public OnNewCar OnNewCarCB;	


	public Camera splitScreenCamLeft;
	public Camera splitScreenCamRight;
	public GameObject splitScreenPanel;

	public List<GameObject> cars = new List<GameObject>();

	static public GameObject getChildGameObject(GameObject fromGameObject, string withName) {
         //Author: Isaac Dart, June-13.
         Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
         foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;

        Debug.LogError("couldn't find: " + withName);
         return null;
    }

    // Find the car for the given JsonTcpClient and remove it from the scene.
    public bool RemoveCar(tk.JsonTcpClient client)
    {
        GameObject toRemove = null;

        foreach(GameObject go in cars)
        {
            GameObject TcpClientObj = getChildGameObject(go, "TCPClient");

            if(TcpClientObj)
            {
                tk.TcpCarHandler handler = TcpClientObj.GetComponent<tk.TcpCarHandler>();
                
                if(handler != null && handler.GetClient() == client)
                {
                    toRemove = go;
                }
            }
        }

        if(toRemove != null)
        {
            RemoveLapTimer(toRemove);
            cars.Remove(toRemove);
            GameObject.Destroy(toRemove);

            if (cars.Count < 2)
                DeactivateSplitScreen();            

            return true;
        }


        Debug.LogError("failed to remove car");
        return false;
    }

    void RemoveLapTimer(GameObject go)
    {
        //Remove race status, if possible.
        RaceManager raceMan = GameObject.FindObjectOfType<RaceManager>();
        if(raceMan != null)
        {
            GameObject to = getChildGameObject(go, "LapTimer");
            
            if(to != null)
                raceMan.RemoveLapTimer(to.GetComponent<LapTimer>());
        }
    }

    public void RemoveAllCars()
    {
        foreach(GameObject car in cars)
        {
            RemoveLapTimer(car);
            GameObject.Destroy(car);
        }

        cars.Clear();
        DeactivateSplitScreen();
        RemoveUiReferences();
    }

    

    public Camera ActivateSplitScreen()
    {
        Camera cam = Camera.main;

        if (splitScreenCamLeft != null)
        {
            splitScreenCamLeft.gameObject.SetActive(true);
            cam = splitScreenCamLeft;
        }

        if (splitScreenCamRight != null)
        {
            splitScreenCamRight.gameObject.SetActive(true);

            //would be better to render both to textures, but too much work.
            Transform camFollowRt = getChildGameObject(cars[0], "CameraFollowTm").transform;

            CameraFollow rtCameraFollow = splitScreenCamRight.transform.GetComponent<CameraFollow>();

            rtCameraFollow.target = camFollowRt;
        }

        if (splitScreenPanel)
            splitScreenPanel.SetActive(true);

        return cam;
    }

    public void DeactivateSplitScreen()
    {
        Camera cam = Camera.main;

        if (splitScreenCamLeft != null)
        {
            splitScreenCamLeft.gameObject.SetActive(false);
            cam = splitScreenCamLeft;
        }

        if (splitScreenCamRight != null)
        {
            splitScreenCamRight.gameObject.SetActive(false);
        }

        if (splitScreenPanel)
            splitScreenPanel.SetActive(false);

        if(cars.Count == 1)
        {
            Transform camFollow = getChildGameObject(cars[0], "CameraFollowTm").transform;

            if(Camera.main != null)
            {
                CameraFollow mainCamFollow = Camera.main.transform.GetComponent<CameraFollow>();

                mainCamFollow.target = camFollow;
            }
        }
    }

    public void CarTextFacecamera(GameObject car, Transform target)
    {
        GameObject carNameObj = getChildGameObject(car, "CarName");

        if(!carNameObj)
            return;

        FaceTarget ft = carNameObj.GetComponent<FaceTarget>();

        if(!ft)
            return;

        ft.target = target;
    
    }

    public bool IsOccupied(Vector3 pos)
    {
        int carCount = cars.Count - 1;

        for(int iCar = 0; iCar < carCount; iCar++)
        {
            if(Vector3.Distance(cars[iCar].transform.position, pos) < 2.0f)
                return true;
        }

        return false;
    }

    public Vector3 GetCarStartPos(int iCar, bool bAvoid)
    {
        Vector3 offset = Vector3.zero;

        //just stack more cars after the second. Not pretty.
        int iRow = iCar / 2;
        offset = Vector3.forward * (-5f * iRow);

        if ((iCar + 1) % 2 == 0)
            offset += Vector3.left * 4.5f;

        Vector3 startPos = startTm.position + offset;

        while(bAvoid && IsOccupied(startPos))
        {
            startPos += Vector3.forward * (-5f * iRow++);
        }

        return startPos;
    }

    public GameObject Spawn (tk.JsonTcpClient client) 
	{
        if(carPrefab == null)
        {
            Debug.LogError("No carPrefab set in CarSpawner!");
            return null;
        }

        //Create a car object, and also hook up all the connections
        //to various places in game that need to hook into the car.
		GameObject go = GameObject.Instantiate(carPrefab) as GameObject;

        if (go == null)
        {
            Debug.LogError("CarSpawner failed to instantiate prefab!");
            return null;
        }

        cars.Add(go);
        
        Vector3 startPos = GetCarStartPos(cars.Count - 1, true);
		go.transform.rotation = startTm.rotation;
		go.transform.position = startPos;
        go.GetComponent<Car>().SavePosRot();

		GameObject TcpClientObj = getChildGameObject(go, "TCPClient");

        Camera cam = Camera.main;

        bool bRaceActive = false;

        RaceManager raceMan = GameObject.FindObjectOfType<RaceManager>();

        if(raceMan)
            bRaceActive = raceMan.bRaceActive;

		//Detect that we have the second car. Doesn't really handle more than 2 right now.
		if(cars.Count > 1 && !bRaceActive)
		{
            cam = ActivateSplitScreen();
		}

       // CarTextFacecamera(go, cam.transform);

		if(TcpClientObj != null)
		{
			//without this it will not connect.
			TcpClientObj.SetActive(true);

			//now set the connection settings.
			TcpCarHandler carHandler = TcpClientObj.GetComponent<TcpCarHandler>();

            if (carHandler != null)
                carHandler.Init(client);
		}

        if (OnNewCarCB != null)
			OnNewCarCB.Invoke(go);

        ///////////////////////////////////////////////
        //Search scene to find these.
        CameraFollow cameraFollow = null;
        if(cam != null)
            cameraFollow = cam.transform.GetComponent<CameraFollow>();
        MenuHandler menuHandler = GameObject.FindObjectOfType<MenuHandler>();
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        GameObject panelMenu = getChildGameObject(canvas.gameObject, "Panel Menu");
        PID_UI pid_ui = null;
        GameObject pidPanel = getChildGameObject(canvas.gameObject, "PIDPanel");
        ///////////////////////////////////////////////

        if (pidPanel)
            pid_ui = pidPanel.GetComponent<PID_UI>();

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

            if (GlobalState.bAutoHideSceneMenu && panelMenu != null)
            {
                panelMenu.SetActive(false);
            }
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

        //Add race status, if possible.
        if(raceMan != null)
        {
            GameObject to = getChildGameObject(go, "LapTimer");

            if(to != null)
                raceMan.AddLapTimer(to.GetComponent<LapTimer>(), client);
        }

        return go;
    }

    internal void EnsureOneCar()
    {
        if (cars.Count == 0)
            Spawn(null);
    }

    public void RemoveUiReferences()
    {
        Camera cam = Camera.main;

        ///////////////////////////////////////////////
        //Search scene to find these.
        CameraFollow cameraFollow = null;
        if(cam != null)
            cameraFollow = cam.transform.GetComponent<CameraFollow>();
        MenuHandler menuHandler = GameObject.FindObjectOfType<MenuHandler>();
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        GameObject panelMenu = getChildGameObject(canvas.gameObject, "Panel Menu");
        PID_UI pid_ui = null;
        GameObject pidPanel = getChildGameObject(canvas.gameObject, "PIDPanel");
        ///////////////////////////////////////////////

        if (pidPanel)
            pid_ui = pidPanel.GetComponent<PID_UI>();

        //set camera target follow tm
        if (cameraFollow != null)
			cameraFollow.target = null;

        //Set menu handler hooks
		if(menuHandler != null)
		{
			menuHandler.PIDContoller = null;
			menuHandler.Logger = null;
			menuHandler.NetworkSteering = null;
			menuHandler.carJSControl  = null;
			menuHandler.trainingManager  = null;
        }

        //Set the PID ui hooks
		if (pid_ui != null)
		{
			pid_ui.pid = null;
			pid_ui.logger = null;
		}

    }
	
}
