using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class startingLine : MonoBehaviour
{
    public int index = 0;
    string target = "body";
    PrivateAPI privateAPI;

    void Start()
    {
        privateAPI = GameObject.FindObjectOfType<PrivateAPI>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.name != target) { return; }

        float time = Time.realtimeSinceStartup;

        Transform parent = col.transform.parent;
        if (parent == null) { return; }

        string carName = parent.name;
        tk.TcpCarHandler client = parent.GetComponentInChildren<tk.TcpCarHandler>();

        if (client != null)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(client.SendCollisionWithStartingLine(index, time));
        }

        if (privateAPI != null)
        {
            privateAPI.CollisionWithStatingLine(carName, index, time);
        }
    }
}
