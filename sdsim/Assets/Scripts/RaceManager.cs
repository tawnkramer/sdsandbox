using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{

    public GameObject StartButton;
    public GameObject StopButton;
    public GameObject SplitButton;
    public GameObject ContinueButton;

    void Awake()
    {
        StartButton.SetActive(true);
        StopButton.SetActive(false);
        SplitButton.SetActive(false);
        ContinueButton.SetActive(false);
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
        SplitButton.SetActive(true);
        ContinueButton.SetActive(false);
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
        SplitButton.SetActive(false);
        ContinueButton.SetActive(false);
    }

    public void Split()
    {   
        Timer[] timers = GameObject.FindObjectsOfType<Timer>();
        foreach(Timer t in timers)
        {
            t.SplitTime();
        }
        StartButton.SetActive(false);
        StopButton.SetActive(true);
        SplitButton.SetActive(false);
        ContinueButton.SetActive(true);
    }

    public void Continue()
    {
        Timer[] timers = GameObject.FindObjectsOfType<Timer>();
        foreach(Timer t in timers)
        {
            t.ContinueTime();
        }
        StartButton.SetActive(false);
        StopButton.SetActive(true);
        SplitButton.SetActive(true);
        ContinueButton.SetActive(false);
    }

}
