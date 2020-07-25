using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceStatus : MonoBehaviour
{
    public LapTimer timer;
    public Text carName;
    public Text racerName;
    public Text currentLapTime;
    public Text bestLapTime;
    public Text iLap;
    public Text dqNot;

    Competitor comp;
    
    public void Init(LapTimer _timer, Competitor c)
    {
        timer = _timer;
        dqNot.gameObject.SetActive(false);
        comp = c;
    }

    public void BootRacer()
    {
        KickPlayerUI kickUI = GameObject.FindObjectOfType<KickPlayerUI>();

        if(kickUI != null && timer != null)
            kickUI.Init(comp.client, comp.car_name);
    }

    void Update()
    {
        if(timer != null)
        {
            carName.text = comp.car_name;
            racerName.text = comp.racer_name;

            if(timer.currentTimeDisp.gameObject.activeSelf)
            {
                currentLapTime.text = timer.currentTimeDisp.text;
            }
            else
            {
                currentLapTime.text = "--:--";
            }

            if(timer.bestTimeDisp.gameObject.activeSelf)
            {
                bestLapTime.text = timer.bestTimeDisp.text;
            }
            else
            {
                bestLapTime.text = "--:--";
            }

            iLap.text = timer.GetNumLapsCompleted().ToString();
            dqNot.gameObject.SetActive(timer.IsDisqualified());
        }
    }
}
