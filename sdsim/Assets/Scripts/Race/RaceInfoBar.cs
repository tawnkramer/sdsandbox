using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class RaceInfoBar : MonoBehaviour
{
    public TMP_Text bar_text;
    public TMP_Text state_text;
    public TMP_Text timer_text;

    public void SetInfoText(string t)
    {
        bar_text.text = t;
    }
    public void SetStateName(string t)
    {
        state_text.text = t;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    internal void SetTimerDisplay(float timer)
    {
        int minutes = Mathf.FloorToInt(timer / 60F);
        int seconds = Mathf.FloorToInt(timer - minutes * 60);
        string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
        timer_text.text = niceTime;
    }
}
