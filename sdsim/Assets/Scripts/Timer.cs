using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public TextMesh currentTotTimeDisp;
    public TextMesh penaltiesDisp;
    public float TimePenalty = 1f; //seconds
    public bool enabled_timer = true;
    public string racerName;
    float penalties = 0.0f; //seconds
    float currentStart = 0.0f; //seconds

    void awake()
    {
        currentTotTimeDisp.gameObject.SetActive(false);
        penaltiesDisp.gameObject.SetActive(false);
    }

    public void StartTimer()
    {
        currentTotTimeDisp.gameObject.SetActive(true);
        penaltiesDisp.gameObject.SetActive(true);
        penalties = 0.0f;
        currentStart = GetTime();
        enabled_timer = true;
    }
    
    public void DisableTimer()
    {
        currentTotTimeDisp.gameObject.SetActive(false);
        penaltiesDisp.gameObject.SetActive(false);
        penalties = 0.0f;
        currentStart = 0.0f;
        enabled_timer = false;
    }

    float GetTime()
    {
        return Time.time;
    }    
    float GetPenalties()
    {
        return penalties;
    }
    float GetCurrentTime()
    {
        return (GetTime() - currentStart) + GetPenalties();
    }

    public void OnCollideCone()
    {   
        if(enabled_timer == false)
            return;

        penalties += TimePenalty;
        if(penaltiesDisp.gameObject.activeSelf){
            float penalties = GetPenalties();
            penaltiesDisp.text = penalties.ToString("00.00");
            Debug.Log("Added penalty");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(enabled_timer == false)
            return;

        if(currentTotTimeDisp.gameObject.activeSelf)
        {
            float currentTime = GetCurrentTime();
            currentTotTimeDisp.text = currentTime.ToString("00.00");
        }
    }
}
