using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapTimer : MonoBehaviour, IComparable<LapTimer>
{
    public List<float> lapTimes = new List<float>();
    public TextMesh currentTimeDisp;
    public TextMesh bestTimeDisp;
    // public TextMesh dqDisp;
    public TextMesh lapCountDisp;
    public string racerName;
    public bool is_enabled = false;
    bool prev_bool = false;
    float bestTime = 100000.0f;
    float currentStart = 0.0f; //milliseconds

    void Awake()
    {
        currentTimeDisp.gameObject.SetActive(false);
        bestTimeDisp.gameObject.SetActive(false);
        lapCountDisp.gameObject.SetActive(false);
        // dqDisp.gameObject.SetActive(false);
    }

    public void ResetRace()
    {
        currentTimeDisp.gameObject.SetActive(false);
        bestTimeDisp.gameObject.SetActive(false);
        lapCountDisp.gameObject.SetActive(false);
        // dqDisp.gameObject.SetActive(false);

        bestTime = 100000.0f;
        currentStart = 0.0f;
        lapTimes = new List<float>();
    }
    
    // implement IComparable interface
    public int CompareTo(LapTimer obj)
    {
        if (obj is LapTimer) {
            return this.bestTime.CompareTo((obj as LapTimer).bestTime);  // compare user names
        }

        throw new ArgumentException("Object is not a LapTime");
    }
    
    public int GetNumLapsCompleted()
    {
        return lapTimes.Count;
    }

    float GetCurrentMS()
    {
        return Time.time * 1000.0f;
    }

    float GetCurrentLapTime()
    {
        float timeNow = GetCurrentMS();
        float lapTime = timeNow - currentStart;
        return lapTime;
    }
    
    public void OnCollideFinishLine()
    {   
        if (is_enabled == false){
            return;
        }

        if (currentStart == 0.0f)
        {
            currentStart = GetCurrentMS();
            currentTimeDisp.gameObject.SetActive(true);
        }
        else
        {
            float timeNow = GetCurrentMS();
            float lapTime = GetCurrentLapTime();

            lapTimes.Add(lapTime);

            if (lapTime < bestTime)
            {
                bestTime = lapTime;
                bestTimeDisp.text = (bestTime / 1000.0f).ToString("00.00");
                bestTimeDisp.gameObject.SetActive(true);
            }

            currentStart = timeNow;
        }
        lapCountDisp.text = GetNumLapsCompleted().ToString("0");
        lapCountDisp.gameObject.SetActive(true);
    }

    void Update()
    {   
        if (is_enabled != prev_bool){
            ResetRace();
            prev_bool = is_enabled;
        }
        else if (is_enabled == false){
            return;
        }

        if(currentTimeDisp.gameObject.activeSelf)
        {
            float lapTime = GetCurrentLapTime();
            currentTimeDisp.text = (lapTime / 1000.0f).ToString("00.00");
        }
    }

}
