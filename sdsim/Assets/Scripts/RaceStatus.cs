using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceStatus : MonoBehaviour
{
    LapTimer timer;
    GameObject dq_notifier;

    public void Reset()
    {
        dq_notifier.SetActive(false);
    }

    public void OnDisqualified()
    {
        dq_notifier.SetActive(true);
    }
}
