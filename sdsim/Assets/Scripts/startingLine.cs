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
        if (col.gameObject.name != target || privateAPI == null) { return; }

        Transform parent = col.transform.parent;
        if (parent == null) { return; }
        string carName = parent.name;

        if (privateAPI == null) { return; }
        privateAPI.CollisionWithStatingLine(carName, index, Time.realtimeSinceStartup);
    }
}
