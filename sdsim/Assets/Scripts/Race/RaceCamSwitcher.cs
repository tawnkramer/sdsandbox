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

    public void OnActivateDynamicCam(Camera cam)
    {
        activeDynamicCam = cam;

        if(mode == Mode.Dynamic)
        {
            activeDynamicCam.gameObject.SetActive(true);
        }
    }

    public void OnDynamicCams()
    {
        if(activeDynamicCam == null)
            return;

        mode = Mode.Dynamic;
        highOverheadCam.gameObject.SetActive(false);
        fpvCam.gameObject.SetActive(false);
        activeDynamicCam.gameObject.SetActive(true);
    }

    public void OnHighOverheadCam()
    {
        mode = Mode.HighOverhead;
        if(activeDynamicCam)
            activeDynamicCam.gameObject.SetActive(false);
        fpvCam.gameObject.SetActive(false);
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
        if(activeDynamicCam)
            activeDynamicCam.gameObject.SetActive(false);
        highOverheadCam.gameObject.SetActive(false);
        fpvCam.gameObject.SetActive(true);
        
        iActiveFPV = iActiveFPV % spawner.cars.Count;
        GameObject car = spawner.cars[iActiveFPV];
        CameraFollow cameraFollow = fpvCam.transform.GetComponent<CameraFollow>();
        cameraFollow.target = CarSpawner.getChildGameObject(car, "CameraFollowTm").transform;
        //cameraFollow.Cut();
    }
}
