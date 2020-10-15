using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColCone : MonoBehaviour
{
    string targetName = "body";
    float penalty = 1;
    
    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.name == targetName)
        {
            Transform parent = col.transform.parent;

            if(parent)
            {
                Timer[] status = parent.gameObject.GetComponentsInChildren<Timer>();

                foreach(Timer t in status)
                {
                    Debug.Log("Collision with penalty cone");
                    t.OnCollideCone(penalty);
                }
            }
        }
    }

}
