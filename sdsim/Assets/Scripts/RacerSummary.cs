using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RacerSummary : MonoBehaviour
{


    public LapTimer timer;
    public Text place;
    public Text userName;
    public Text[] lap_times;
    public Text lap_total;
    public Text dqNot;
    public Color lapTimeColor;
    public Color bestLapTimeColor;
    

    public void Init(LapTimer _timer, int _place)
    {
        timer = _timer;

        place.text = _place.ToString() + ".";
        userName.text = timer.racerName;

        if(timer.IsDisqualified())
        {
            dqNot.gameObject.SetActive(false);
            lap_total.gameObject.SetActive(false);
        }

        float best = timer.GetBestLapTime();

        for(int iLap = 0; iLap < lap_times.Length; iLap++)
        {
            Text lap_time = lap_times[iLap];
            float t = timer.GetLapTime(iLap);

            if( t == best)
            {
                lap_time.color = bestLapTimeColor;
            }
            else
            {
                lap_time.color = lapTimeColor;
            }

            if(t != 0.0f)
                lap_time.text = (t / 1000.0f).ToString("00.00");
        }

        float totalTime = timer.GetTotalTime();
        lap_total.text = (totalTime / 1000.0f).ToString("00.00");
    }
}
