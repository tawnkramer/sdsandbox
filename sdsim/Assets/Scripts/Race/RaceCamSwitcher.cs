using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceCamSwitcher : MonoBehaviour
{
    public Camera activeDynamicCam;
    public Camera highOverheadCam;
    public Camera fpvCam;

    enum Mode { Dynamic, HighOverhead, FPV }

    Mode mode = Mode.Dynamic;

    int iActiveFPV = 0;

    void DisableAllCamerasExSensors()
    {
        Camera[] cameras = GameObject.FindObjectsOfType<Camera>();

        foreach(Camera cam in cameras)
        {
            CameraSensor sensor = cam.gameObject.GetComponent<CameraSensor>();

            if(sensor != null)
                continue;

            cam.gameObject.SetActive(false);
        }
    }

    public void OnActivateDynamicCam(Camera cam)
    {
        activeDynamicCam = cam;

        if(mode == Mode.Dynamic)
        {
             if(activeDynamicCam == null)
                return;

            DisableAllCamerasExSensors();
            activeDynamicCam.gameObject.SetActive(true);
        }
    }

    public void OnDynamicCams()
    {
        if(activeDynamicCam == null)
            return;

        mode = Mode.Dynamic;
        DisableAllCamerasExSensors();
        activeDynamicCam.gameObject.SetActive(true);
    }

    public void OnHighOverheadCam()
    {
        mode = Mode.HighOverhead;
        DisableAllCamerasExSensors();
        highOverheadCam.gameObject.SetActive(true);
    }

    public void OnFPVCam()
    {
        CarSpawner spawner = GameObject.FindObjectOfType<CarSpawner>();
        if(spawner == null || spawner.cars.Count == 0)
            return;

        // clicking the FPV button multiple times switches the active car.
        if(mode == Mode.FPV)
            iActiveFPV += 1;

        mode = Mode.FPV;
        DisableAllCamerasExSensors();
        fpvCam.gameObject.SetActive(true);
        
        iActiveFPV = iActiveFPV % spawner.cars.Count;
        GameObject car = spawner.cars[iActiveFPV];
        CameraFollow cameraFollow = fpvCam.transform.GetComponent<CameraFollow>();
        cameraFollow.target = CarSpawner.getChildGameObject(car, "CameraFollowTm").transform;
        //cameraFollow.Cut();
    }
}
