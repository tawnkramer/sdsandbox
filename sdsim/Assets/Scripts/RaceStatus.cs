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
        tk.TcpServer server = GameObject.FindObjectOfType<tk.TcpServer>();
        if(client)
        {
            //Block client from coming back.
            if(server)
                server.Block(client.GetIPAddress());

            client.Drop();
        }
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

            if(timer.bestTimeDisp.gameObject.activeSelf)
            {
                bestLapTime.text = timer.bestTimeDisp.text;
            }

            if(timer.IsDisqualified())
            {
                dqNot.gameObject.SetActive(true);
            }
        }
    }
}
