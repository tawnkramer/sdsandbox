using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverheadViewManager : MonoBehaviour
{
    public GameObject carOverheadMarker;

    // Start is called before the first frame update
    void Start()
    {
        carOverheadMarker.SetActive(true);
    }
}
