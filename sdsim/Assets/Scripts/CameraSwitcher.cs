using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public string targetName = "body";
    public Camera activeCam;
    public CameraSwitcher next; 
    public CameraSwitcher previous;

    public void SwitchToThisCam()
    {
        if(previous)
            previous.activeCam.gameObject.SetActive(false);

        RaceCamSwitcher raceCamSw = GameObject.FindObjectOfType<RaceCamSwitcher>();

        if(raceCamSw != null)
            raceCamSw.OnActivateDynamicCam(activeCam);
        else
            activeCam.gameObject.SetActive(true);

        this.gameObject.SetActive(false);

        next.gameObject.SetActive(true);
    }

    void OnTriggerEnter(Collider col)
    {
        Debug.Log("got coll w" + col.gameObject.name);

        if(col.gameObject.name != targetName)
            return;

        SwitchToThisCam();
    }
}
