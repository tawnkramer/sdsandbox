using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaded : MonoBehaviour
{
    public bool inFrontEnd = false;

    // Start is called before the first frame update
    void Start()
    {
        SandboxServer server = GameObject.FindObjectOfType<SandboxServer>();

        if (server)
            server.OnSceneLoaded(inFrontEnd);
    }    
}
