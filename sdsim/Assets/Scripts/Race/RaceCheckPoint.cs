using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceCheckPoint : MonoBehaviour
{
    public List<GameObject> required_col = new List<GameObject>();

    public float timer = 0.0f;

    public void Reset()
    {
        timer = 0.0f;
        required_col.Clear();
    }

    public void SetReqTime(float required_col_time)
    {
        timer = required_col_time;        
    }

    public void AddRequiredHit(GameObject ob)
    {
        required_col.Add(ob);
    } 

    public void RemoveBody(GameObject go)
    {
        required_col.Remove(go);
    }

    void OnTriggerEnter(Collider col)
    {
        for(int iO = 0; iO < required_col.Count; iO++)
        {
            if(required_col[iO] == col.gameObject)
            {
                RaceManager rm = GameObject.FindObjectOfType<RaceManager>();
                rm.OnHitCheckPoint(col.gameObject);
                required_col.RemoveAt(iO);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(timer > 0.0f)
        {
            timer -= Time.deltaTime;

            if(timer <= 0.0f)
            {
                timer = 0.0f;
                RaceManager rm = GameObject.FindObjectOfType<RaceManager>();

                foreach(GameObject go in required_col)
                {
                    if(go != null)
                        rm.OnCheckPointTimedOut(go);
                }
            }
        }
    }
}
