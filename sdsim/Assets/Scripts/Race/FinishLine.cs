using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    public string targetName = "body";

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.name == targetName)
        {
            Transform parent = col.transform.parent;

            if(parent)
            {
                LapTimer[] status = parent.gameObject.GetComponentsInChildren<LapTimer>();

                foreach(LapTimer t in status)
                {
                    Debug.Log("on timer collide w finish line.");
                    t.OnCollideFinishLine();
                }
            }
        }
    }
}
