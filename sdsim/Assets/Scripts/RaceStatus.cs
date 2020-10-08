using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceStatus : MonoBehaviour
{
    public Timer timer;
    public Text racerName;
    public Text currentTotTime;
    public Text penaltiesTime;
    tk.JsonTcpClient client;
    
    public void Init(Timer _timer, tk.JsonTcpClient _client)
    {
        timer = _timer;
        client = _client;
    }

    void Update()
    {
        if(timer != null)
        {
            racerName.text = timer.racerName;

            if(timer.currentTotTimeDisp.gameObject.activeSelf && timer.enabled_timer)
            {
                currentTotTime.text = timer.currentTotTimeDisp.text;
            }
            else
            {
                currentTotTime.text = "--:--";
            }

            if(timer.penaltiesDisp.gameObject.activeSelf && timer.enabled_timer)
            {
                penaltiesTime.text = timer.penaltiesDisp.text;
            }
            else
            {
                penaltiesTime.text = "--:--";
            }
        }
    }
}
