using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColCone : MonoBehaviour
{
    public int index = 0;
    string target = "body";
    float penalty = 1;
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
        privateAPI.CollisionWithCone(carName, index, Time.realtimeSinceStartup);

        Timer[] status = parent.gameObject.GetComponentsInChildren<Timer>();
        foreach (Timer t in status)
        {
            Debug.Log("Collision with penalty cone");
            t.OnCollideCone(penalty);
        }
    }
}
