using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceStatus : MonoBehaviour
{
    public LapTimer timer;
    public Text userName;
    public Text currentLapTime;
    public Text bestLapTime;
    public Text iLap;
    public Text dqNot;

    tk.JsonTcpClient client;
    
    public void Init(LapTimer _timer, tk.JsonTcpClient _client)
    {
        timer = _timer;
        dqNot.gameObject.SetActive(false);
        client = _client;
    }

    public void BootRacer()
    {
        KickPlayerUI kickUI = GameObject.FindObjectOfType<KickPlayerUI>();

        if(kickUI != null && timer != null)
            kickUI.Init(client, timer.racerName);
    }

    void Update()
    {
        if(timer != null)
        {
            userName.text = timer.racerName;

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
