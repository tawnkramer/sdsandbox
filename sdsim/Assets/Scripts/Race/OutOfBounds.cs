using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    public string targetName = "body";

    void OnTriggerEnter(Collider col)
    {
        Debug.Log("got coll w" + col.gameObject.name);

        if(col.gameObject.name != targetName)
            return;
        
        Transform parent = col.transform.parent;

        if(parent == null)
            return;

        Debug.Log("parent" + parent.gameObject.name);
        
        RaceManager rm = GameObject.FindObjectOfType<RaceManager>();

        if(rm)
        {
            rm.OnCarOutOfBounds(parent.gameObject);
        }
    }
}
