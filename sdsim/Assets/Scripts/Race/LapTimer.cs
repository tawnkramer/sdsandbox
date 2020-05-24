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
    public string car_name;

    public float min_lap_time = 10.0f; //seconds
    public bool race_completed = false;

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

        bestTime = 1000000.0f;
        race_completed = false;
        currentStart = 0.0f;
        lapTimes = new List<float>();
    }

    public void OnRaceCompleted()
    {
        race_completed = true;
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

    public float GetCurrentLapTimeMS()
    {
        if (currentStart == 0.0f)
            return 0.0f;

        float timeNow = GetCurrentMS();
        float lapTime = timeNow - currentStart;
        return lapTime;
    }

    public float GetCurrentLapTimeSec()
    {
        return GetCurrentLapTimeMS() / 1000.0f;
    }

    public bool IsDisqualified()
    {
        return dqDisp.gameObject.activeSelf;
    }

    public void OnDisqualified()
    {
        dqDisp.gameObject.SetActive(true);
    }

    public void ResetDisqualified()
    {
        dqDisp.gameObject.SetActive(false);
    }

    public void RestartCurrentLap()
    {
        float timeNow = GetCurrentMS();
        currentStart = timeNow;
    }

    public void OnCollideFinishLine()
    {
        if( IsDisqualified() || race_completed)
            return;
            
        if(currentStart == 0.0f)
        {
            Debug.Log(car_name + " crossed start line.");
            currentStart = GetCurrentMS();
            currentTimeDisp.gameObject.SetActive(true);
        }
        else
        {

            float timeNow = GetCurrentMS();
            float lapTime = GetCurrentLapTimeMS();

            // preventing quick loop and collide again w finish.
            if (lapTime < (min_lap_time * 1000.0f))
            {
                return;
            }

            Debug.Log(car_name + " finished a lap " + lapTime / 1000.0f);

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

    public float GetLapTimeMS(int iLap)
    {
        if(iLap < lapTimes.Count)
            return lapTimes[iLap];

        return 0.0f;
    }

    public float GetTotalTimeMS()
    {
        float total = 0.0f;

        foreach(float t in lapTimes)
        {
            total += t;
        }

        return total;
    }

    public float GetBestLapTimeMS()
    {
        return bestTime;
    }

    public float GetBestLapTimeSec()
    {
        return bestTime / 1000.0f;
    }

    void Update()
    {
        //if(currentTimeDisp.gameObject.activeSelf && !IsDisqualified())
        if(currentTimeDisp.gameObject.activeSelf)
        {
            float lapTime = GetCurrentLapTimeSec();
            currentTimeDisp.text = lapTime.ToString("00.00");
        }
    }
}
