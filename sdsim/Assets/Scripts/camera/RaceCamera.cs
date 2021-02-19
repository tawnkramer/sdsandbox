using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceCamera : MonoBehaviour
{
    RaceCameras raceCameras;
    public CameraTrigger cameraTrigger;
    public Camera camera;
    public int index;

    void Awake()
    {
        GameObject goCamChild = new GameObject(string.Format("Camera"));
        goCamChild.transform.SetParent(transform);
        camera = goCamChild.AddComponent<Camera>();
        camera.enabled = false;
        camera.fieldOfView = 90;

        GameObject goTriggerChild = new GameObject(string.Format("Cam Trigger"));
        goTriggerChild.transform.SetParent(transform);
        cameraTrigger = goTriggerChild.AddComponent<CameraTrigger>();

        raceCameras = transform.GetComponentInParent<RaceCameras>();
    }

    public void SetCameraTrigger(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        cameraTrigger.setBoxCollider(position, rotation, scale);
    }
    public void SetCam(Vector3 position, Vector3 lookAt)
    {
        camera.transform.position = position;
        camera.transform.rotation = Quaternion.LookRotation(lookAt - position, Vector3.up);
    }

    public void CameraTriggered(Collider col)
    {
        raceCameras.CameraTriggered(col, camera, index);
    }

}
