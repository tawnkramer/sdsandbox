using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KickPlayerUI : MonoBehaviour
{
    public Text playerName;
    public GameObject panelDisplay;

    tk.JsonTcpClient client;


    public void Init(tk.JsonTcpClient _client, string _playerName)
    {
        client = _client;
        playerName.text = _playerName;
        this.panelDisplay.SetActive(true);
    }

    public void OnBoot()
    {
        tk.TcpServer server = GameObject.FindObjectOfType<tk.TcpServer>();

        if(client)
        {         
            client.Drop();
        }

        Close();
    }

    public void OnBan()
    {
        tk.TcpServer server = GameObject.FindObjectOfType<tk.TcpServer>();
        if(client)
        {
            //Block client from coming back.
            if(server)
                server.Block(client.GetIPAddress());

            client.Drop();
        }

        Close();
    }

    public void OnCancel()
    {
        Close();
    }

    public void Close()
    {
        this.panelDisplay.SetActive(false);
    }

}
