using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalStateWatcher : MonoBehaviour {

    public GameObject menuObj;
    public GameObject networkObj;

    // Use this for initialization
    void Start () {

        if (GlobalState.bAutoHideSceneMenu && menuObj != null)
            menuObj.SetActive(false);

        if (GlobalState.bAutoConnectToWebSocket && networkObj != null)
            networkObj.SetActive(true);

    }
	
}
