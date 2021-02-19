using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrivateAPI : MonoBehaviour
{
    private List<tk.TcpPrivateAPIHandler> clients = new List<tk.TcpPrivateAPIHandler>();

    public void Init(tk.JsonTcpClient _client)
    {
        tk.TcpPrivateAPIHandler tcpPrivateAPIHandler = gameObject.AddComponent<tk.TcpPrivateAPIHandler>();
        tcpPrivateAPIHandler.Init(_client);
        clients.Add(tcpPrivateAPIHandler);
    }

    public void CollisionWithStatingLine(string name, int startingLineIndex, float timeStamp)
    {
        foreach (tk.TcpPrivateAPIHandler client in clients)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(client.SendCollisionWithStartingLine(name, startingLineIndex, timeStamp));
        }
    }
    public void CollisionWithCone(string name, int coneIndex, float timeStamp)
    {
        foreach (tk.TcpPrivateAPIHandler client in clients)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(client.SendCollisionWithCone(name, coneIndex, timeStamp));
        }
    }
}
