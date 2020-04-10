using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LapTimer : MonoBehaviour
{
    public List<float> lapTimes = new List<float>();
    public float bestTime = 100000.0f;
    float currentStart = 0.0f; //milliseconds
    public TextMesh currentTimeDisp;
    public TextMesh bestTimeDisp;
    public TextMesh dqDisp;

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

    void Update()
    {
        if(currentTimeDisp.gameObject.activeSelf)
        {
            float lapTime = GetCurrentLapTime();
            currentTimeDisp.text = (lapTime / 1000.0f).ToString("00.00");
        }
    }
}
