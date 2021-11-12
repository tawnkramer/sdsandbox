﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tk;
using System;

public class CarSpawner : MonoBehaviour
{

    public PathManager pathManager;
    public GameObject carPrefab;
    public Transform[] startsTm; // list containing multiple starting points
    public bool EnableTrainingManager = false;

    public delegate void OnNewCar(GameObject carObj);
    public OnNewCar OnNewCarCB;

    public int numCarRows = 2;
    public float distCarCols = 4.5f;
    public float distCarRows = 5f;

    public GameObject mainCamera;
    public GameObject splitScreenCamPrefab;
    public GameObject splitScreenOHCamPrefab;
    public int SplitScreenWidth = 2;
    public RaceCameras raceCameras;

    public GameObject racerStatusPrefab;
    public RectTransform raceStatusPanel;
    int raceStatusWidth = 380;
    int raceStatusHeight = 100;
    int n_columns = 2; // number of columns in the RaceStatus panel

    public List<GameObject> cars = new List<GameObject>();
    public List<GameObject> cameras = new List<GameObject>();

    static public GameObject getChildGameObject(GameObject fromGameObject, string withName)
    {
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

        foreach (GameObject go in cars)
        {
            GameObject TcpClientObj = getChildGameObject(go, "TCPClient");

            if (TcpClientObj != null)
            {
                tk.TcpCarHandler handler = TcpClientObj.GetComponent<tk.TcpCarHandler>();

                if (handler != null && handler.GetClient() == client)
                {
                    toRemove = go;
                }
            }
        }

        if (toRemove != null)
        {
            int iSplitScreenCam = cars.IndexOf(toRemove);
            if (GlobalState.overheadCamera) { iSplitScreenCam += 1; }

            if (raceCameras != null)
            {
                int carID = toRemove.GetInstanceID() - 4;
                if (raceCameras.carProgress.ContainsKey(carID))
                {
                    raceCameras.carProgress.Remove(carID);
                }
            }

            RemoveTimer(toRemove);
            cars.Remove(toRemove);

            if (cameras.Count > iSplitScreenCam)
            {
                GameObject SplitScreenCamGo = cameras[iSplitScreenCam];
                RemoveSplitScreenCam(SplitScreenCamGo);
            }
            GameObject.Destroy(toRemove);

            Debug.Log("Removed car");
            return true;
        }
        else
        {
            Debug.LogError("failed to remove car");
            return false;

        }
    }

    public void RemoveGhostCars()
    {
        foreach (GameObject car in cars)
        {
            tk.TcpCarHandler tcpCarHandler = car.GetComponentInChildren<tk.TcpCarHandler>();
            if (tcpCarHandler != null && tcpCarHandler.IsGhostCar())
            {
                tcpCarHandler.Boot();
            }
        }
    }

    public void RemoveAllCars()
    {
        // Remove each car one by one
        foreach (GameObject car in cars)
        {
            int i = cars.IndexOf(car);

            if (raceCameras != null)
            {
                int carID = car.GetInstanceID() - 4;
                if (raceCameras.carProgress.ContainsKey(carID))
                {
                    raceCameras.carProgress.Remove(carID);
                }
            }

            RemoveTimer(car);
            cars.Remove(car);

            if (cameras.Count > i)
            {
                GameObject SplitScreenCamGo = cameras[i + 1];
                RemoveSplitScreenCam(SplitScreenCamGo);
            }
            GameObject.Destroy(car);
        }
        RemoveUiReferences();
    }

    void UpdateRaceStatusPannel()
    {
        int n_children = raceStatusPanel.transform.childCount;
        int row = n_children;
        if (row > n_columns)
            row = n_columns;

        int col = (n_children / n_columns) + (n_children % n_columns);
        float width = row * raceStatusWidth;
        float height = col * raceStatusHeight;
        raceStatusPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        raceStatusPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        raceStatusPanel.anchoredPosition = new Vector3(8.0f, -1 * height, 0.0f);
    }

    public void AddTimer(Timer t, tk.JsonTcpClient client)
    {
        if (racerStatusPrefab == null)
            return;

        GameObject go = Instantiate(racerStatusPrefab) as GameObject;
        RaceStatus rs = go.GetComponent<RaceStatus>();
        rs.Init(t, client);
        go.transform.SetParent(raceStatusPanel.transform);
        go.transform.GetComponent<RectTransform>().localScale = raceStatusPanel.transform.localScale;

        UpdateRaceStatusPannel(); // update the UI with the new child count
        Debug.Log("Added timer");

    }

    public void RemoveTimer(GameObject go)
    {
        Timer timer = getChildGameObject(go, "Timer").GetComponent<Timer>();

        if (timer != null && raceStatusPanel != null)
        {
            int count = raceStatusPanel.transform.childCount;
            for (int i = 0; i < count; i++)
            {
                Transform child = raceStatusPanel.transform.GetChild(i);
                RaceStatus rs = child.GetComponent<RaceStatus>();
                if (rs.timer == timer)
                {
                    child.transform.SetParent(null); // detach from parent
                    Destroy(child.gameObject); // destroy child
                    UpdateRaceStatusPannel(); // update the UI with the new child count
                    Debug.Log("removed timer");
                    return;
                }
            }
            Debug.LogError("failed to find timer while removing it");
            return;
        }
        Debug.LogError("failed to remove timer");
    }

    public void AddSplitScreenCam(int index)
    {
        if (cameras.Count < GlobalState.maxSplitScreen && !GlobalState.raceCameras)
        {
            if (index == 0 && GlobalState.overheadCamera)
            {
                GameObject splitScreenOHCamGo = Instantiate(splitScreenOHCamPrefab);
                OverHeadCamera OHCam = splitScreenOHCamGo.GetComponent<OverHeadCamera>();
                OHCam.pathManager = pathManager;
                OHCam.Init();

                cameras.Add(splitScreenOHCamGo);
            }
            else
            {
                GameObject splitScreenCamGo = Instantiate(splitScreenCamPrefab);
                cameras.Add(splitScreenCamGo);
            }
        }
    }

    public void RemoveSplitScreenCam(GameObject splitScreenCamGo)
    {
        GameObject.Destroy(splitScreenCamGo);
        cameras.Remove(splitScreenCamGo);
        UpdateSplitScreenCams();
        Debug.Log("removed split screen camera");
    }

    public void UpdateSplitScreenCams()
    {

        if (GlobalState.raceCameras)
        {
            if (mainCamera != null) { mainCamera.SetActive(false); }
            return;
        }

        int num_cameras = cars.Count;
        if (GlobalState.overheadCamera) { num_cameras += 1; }
        if (num_cameras > GlobalState.maxSplitScreen) { num_cameras = GlobalState.maxSplitScreen; }

        // check if the number of cameras match the number of cars
        if ((cameras.Count != num_cameras))
        {
            // remove all cameras in there
            foreach (GameObject splitScreenCamGo in cameras)
            {
                GameObject.Destroy(splitScreenCamGo);
            }
            cameras.Clear();

            // and recreate some new ones
            for (int i = 0; i < num_cameras; i++)
            {
                AddSplitScreenCam(i);
            }
        }

        // for each camera, update the rect
        for (int i = 0; i < num_cameras; i++)
        {

            if (i > 0 || !GlobalState.overheadCamera) // if the camera isn't overhead, assign a car to it
            {
                GameObject splitScreenCamGo = cameras[i];
                GameObject car;
                if (GlobalState.overheadCamera) { car = cars[i - 1]; }
                else { car = cars[i]; }

                // set target to the corresponding car
                Camera splitScreenCam = splitScreenCamGo.GetComponent<Camera>();

                DrawLidar dLidar = splitScreenCamGo.GetComponent<DrawLidar>();
                dLidar.car = car;

                CameraFollow cameraFollow = splitScreenCam.GetComponent<CameraFollow>();
                cameraFollow.target = getChildGameObject(car, "CameraFollowTm").transform;
            }

            int x_index = i % SplitScreenWidth;
            int y_index = i / SplitScreenWidth;
            int number_in_row = Math.Min((cameras.Count - y_index * SplitScreenWidth), SplitScreenWidth);
            int number_of_row = 1 + ((cameras.Count - 1) / SplitScreenWidth);

            float w = 1 / (float)(number_in_row);
            float h = 1 / (float)(number_of_row);

            float x = (x_index) / (float)number_in_row;
            float y = (y_index) / (float)number_of_row;

            GameObject go = cameras[i];
            Camera camera = go.GetComponent<Camera>();
            camera.rect = new Rect(x, y, w, h);

            if (GlobalState.overheadCamera && i == 0) { OverHeadCamera ohcam = go.GetComponent<OverHeadCamera>(); ohcam.Init(); }
        }

        if (cameras.Count == 0 && mainCamera != null && !GlobalState.raceCameras)
        {
            mainCamera.SetActive(true);
        }
        else if (mainCamera != null)
        {
            mainCamera.SetActive(false); // make sure we are disabling main camera to avoid background rendering
        }
    }

    public void CarTextFacecamera(GameObject car, Transform target)
    {
        GameObject carNameObj = getChildGameObject(car, "CarName");

        if (!carNameObj)
            return;

        FaceTarget ft = carNameObj.GetComponent<FaceTarget>();

        if (!ft)
            return;

        ft.target = target;

    }

    public bool IsOccupied(Vector3 pos)
    {
        int carCount = cars.Count - 1;

        for (int iCar = 0; iCar < carCount; iCar++)
        {
            GameObject go = cars[iCar];
            Car car = go.GetComponent<Car>();
            if (Vector3.Distance(car.startPos, pos) < 1.0f)
                return true;
        }

        return false;
    }

    public (Vector3, Quaternion) GetStartPosRot(int iCar)
    {

        int iSpawn = iCar % startsTm.Length;
        Transform spawn = startsTm[iSpawn];
        Vector3 pos = spawn.position;
        Quaternion rot = spawn.rotation;

        int iCol = (iCar / startsTm.Length) % numCarRows;
        int iRow = (iCar / startsTm.Length) / numCarRows;

        Vector3 offset = Vector3.zero;
        offset.z = -distCarRows * iRow;
        offset.x = -distCarCols * iCol;

        return (spawn.position + rot * offset, rot);
    }

    public (Vector3, Quaternion) GetCarStartPosRot()
    {

        Vector3 startPos = startsTm[0].position; // default position
        Quaternion startRot = startsTm[0].rotation; // default rotation

        if (IsOccupied(startPos))
        {
            int iCar = 0;
            while (IsOccupied(startPos))
            {
                (startPos, startRot) = GetStartPosRot(iCar);
                iCar++;
            }
        }

        return (startPos, startRot);
    }


    public GameObject Spawn(tk.JsonTcpClient client, bool paceCar)
    {
        if (carPrefab == null)
        {
            Debug.LogError("No carPrefab set in CarSpawner!");
            return null;
        }

        // Create a car object, and also hook up all the connections
        // to various places in game that need to hook into the car.
        GameObject go = GameObject.Instantiate(carPrefab) as GameObject;

        if (go == null)
        {
            Debug.LogError("CarSpawner failed to instantiate prefab!");
            return null;
        }

        cars.Add(go);

        (Vector3 startPos, Quaternion startRot) = GetCarStartPosRot();
        go.transform.SetPositionAndRotation(startPos, startRot);
        go.GetComponent<Car>().SavePosRot();
        UpdateSplitScreenCams();

        GameObject TcpClientObj = getChildGameObject(go, "TCPClient");


        // CarTextFacecamera(go, cam.transform);

        if (TcpClientObj != null)
        {
            // without this it will not connect.
            TcpClientObj.SetActive(true);

            // now set the connection settings.
            TcpCarHandler carHandler = TcpClientObj.GetComponent<TcpCarHandler>();

            if (carHandler != null)
                carHandler.Init(client);
        }

        if (OnNewCarCB != null)
            OnNewCarCB.Invoke(go);

        ///////////////////////////////////////////////
        // Search scene to find these.
        MenuHandler menuHandler = GameObject.FindObjectOfType<MenuHandler>();
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        GameObject panelMenu = getChildGameObject(canvas.gameObject, "Panel Menu");
        GameObject pidPanel = getChildGameObject(canvas.gameObject, "PIDPanel");
        ///////////////////////////////////////////////

        // set camera target follow tm

        // Set menu handler hooks
        if (menuHandler != null)
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

        if (paceCar && !GlobalState.manualDriving)
        {
            GameObject pidController_go = getChildGameObject(go, "PIDController");
            pidController_go.SetActive(true);
        }
        else if (paceCar && GlobalState.manualDriving)
        {
            GameObject jsController = getChildGameObject(go, "JoyStickCarContoller");
            jsController.SetActive(true);
        }

        // Add race status, if possible.
        GameObject to = getChildGameObject(go, "Timer");

        if (to != null)
        {
            AddTimer(to.GetComponent<Timer>(), client);
        }
        else
        {
            Debug.LogError("failed to find Timer");
        }

        return go;
    }

    internal void EnsureOneCar()
    {
        // pace car doesn't always mean cars.Count = 0, so will need to refactor that
        if (cars.Count == 0)
            Spawn(null, GlobalState.paceCar);
    }

    public void RemoveUiReferences()
    {
        Camera cam = Camera.main;

        ///////////////////////////////////////////////
        // Search scene to find these.
        CameraFollow cameraFollow = cam.transform.GetComponent<CameraFollow>();
        MenuHandler menuHandler = GameObject.FindObjectOfType<MenuHandler>();
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        GameObject panelMenu = getChildGameObject(canvas.gameObject, "Panel Menu");
        PID_UI pid_ui = null;
        GameObject pidPanel = getChildGameObject(canvas.gameObject, "PIDPanel");
        ///////////////////////////////////////////////

        if (pidPanel)
            pid_ui = pidPanel.GetComponent<PID_UI>();

        // set camera target follow tm
        if (cameraFollow != null)
            cameraFollow.target = null;

        // Set menu handler hooks
        if (menuHandler != null)
        {
            menuHandler.PIDContoller = null;
            menuHandler.Logger = null;
            menuHandler.NetworkSteering = null;
            menuHandler.carJSControl = null;
            menuHandler.trainingManager = null;
        }

        // Set the PID ui hooks
        if (pid_ui != null)
        {
            pid_ui.pid = null;
            pid_ui.logger = null;
        }

    }

}
