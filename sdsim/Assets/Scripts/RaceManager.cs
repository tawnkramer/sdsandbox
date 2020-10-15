using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{

    public GameObject StartButton;
    public GameObject StopButton;
    public GameObject ResetButton;

    void Awake()
    {
        StartButton.SetActive(true);
        StopButton.SetActive(false);
        ResetButton.SetActive(false);
    }

    public void StartRace()
    {
        Timer[] timers = GameObject.FindObjectsOfType<Timer>();
        foreach(Timer t in timers)
        {
            t.StartTimer();
        }
        StartButton.SetActive(false);
        StopButton.SetActive(true);
        ResetButton.SetActive(true);
    }

    public void StopRace()
    {
        Timer[] timers = GameObject.FindObjectsOfType<Timer>();
        foreach(Timer t in timers)
        {
            t.DisableTimer();
        }
        StartButton.SetActive(true);
        StopButton.SetActive(false);
        ResetButton.SetActive(false);
    }

    public void ResetRace()
    {
        Timer[] timers = GameObject.FindObjectsOfType<Timer>();
        foreach(Timer t in timers)
        {
            t.ResetTimer();
        }
    }
}
