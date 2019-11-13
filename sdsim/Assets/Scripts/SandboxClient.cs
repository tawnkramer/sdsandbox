using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(tk.TcpClient))]
public class SandboxClient : MonoBehaviour
{
    tk.TcpClient _client = null;

    public void Awake()
    {
        _client = GetComponent<tk.TcpClient>();
        _client.onDataRecvCB += new tk.TcpClient.OnDataRecv(OnRecvData);
    }

    public void OnRecvData(byte[] data)
    {
        /// echo client...
        _client.SendDataToPeers(data);
    }

}
