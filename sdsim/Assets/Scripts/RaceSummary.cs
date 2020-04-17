using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceSummary : MonoBehaviour
{
    public Transform racerLayoutGroup;
    public GameObject racerSummaryPrefab;

    public void Init()
    {
        //clean out any previous summary...
        int count = racerLayoutGroup.childCount;
        for(int i = count - 1; i >= 0; i--)
        {
            Transform child = racerLayoutGroup.transform.GetChild(i);
            Destroy(child.gameObject);
        }

        // Now add one summary object per LapTimer
        LapTimer[] timers = GameObject.FindObjectsOfType<LapTimer>();

        // But first sort things according to place.
        Array.Sort(timers);

        for(int iT = 0; iT < timers.Length; iT++)
        {
            GameObject go = Instantiate(racerSummaryPrefab) as GameObject;

            RacerSummary s = go.GetComponent<RacerSummary>();

            s.Init(timers[iT], iT + 1);

            go.transform.SetParent(racerLayoutGroup);
        }
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }
   
}
