using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapTimer : MonoBehaviour, IComparable<LapTimer>
{
    public List<float> lapTimes = new List<float>();
    public float bestTime = 100000.0f;
    float currentStart = 0.0f; //milliseconds
    public TextMesh currentTimeDisp;
    public TextMesh bestTimeDisp;
    public TextMesh dqDisp;
    public string racerName;

    void Awake()
    {
        currentTimeDisp.gameObject.SetActive(false);
        bestTimeDisp.gameObject.SetActive(false);
        dqDisp.gameObject.SetActive(false);
    }

    public void ResetRace()
    {
        currentTimeDisp.gameObject.SetActive(false);
        bestTimeDisp.gameObject.SetActive(false);
        dqDisp.gameObject.SetActive(false);

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

    public bool IsDisqualified()
    {
        return dqDisp.gameObject.activeSelf;
    }

    public void OnDisqualified()
    {
        dqDisp.gameObject.SetActive(true);
    }

    public void OnCollideFinishLine()
    {
        if( IsDisqualified())
            return;
            
        if(currentStart == 0.0f)
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
    }

    public float GetLapTime(int iLap)
    {
        if(iLap < lapTimes.Count)
            return lapTimes[iLap];

        return 0.0f;
    }

    public float GetTotalTime()
    {
        float total = 0.0f;

        foreach(float t in lapTimes)
        {
            total += t;
        }

        return total;
    }

    public float GetBestLapTime()
    {
        return bestTime;
    }

    void Update()
    {
        if(currentTimeDisp.gameObject.activeSelf && !IsDisqualified())
        {
            float lapTime = GetCurrentLapTime();
            currentTimeDisp.text = (lapTime / 1000.0f).ToString("00.00");
        }
    }
}
