using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{

    public GameObject ResetButton;
    public GameObject StopButton;

    public void ResetRace()
    {
        Timer[] timers = GameObject.FindObjectsOfType<Timer>();
        foreach(Timer t in timers)
        {
            t.StartTimer();
        }
        ResetButton.SetActive(false);
        StopButton.SetActive(true);
    }

    public void StopRace()
    {
        Timer[] timers = GameObject.FindObjectsOfType<Timer>();
        foreach(Timer t in timers)
        {
            t.DisableTimer();
        }
        StopButton.SetActive(false);
        ResetButton.SetActive(true);
    }

}
