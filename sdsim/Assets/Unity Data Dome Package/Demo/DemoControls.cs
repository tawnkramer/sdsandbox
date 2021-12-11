using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DemoControls : MonoBehaviour
{
    const float PositionIncrement = 0.1f;
 
    public DomeController domeController;

    public Transform demoCameraBase;

	void Awake()
    {
        if (domeController == null)
            Debug.LogError("DemoControls: Dome Controller not set!");

        if (demoCameraBase == null)
            Debug.LogError("DemoControls: Demo Camera Base not set!");
	}
	
	void LateUpdate()
    {
        if (domeController == null || domeController.worldCamera == null)
            return;

        Vector3 cameraPos       = demoCameraBase.position;
        Vector3 cameraForward   = domeController.worldCamera.transform.forward;
        Vector3 cameraRight     = domeController.worldCamera.transform.right;
        Vector3 cameraUp        = domeController.worldCamera.transform.up;

        // Demo Controls:
        // --------------

        // W,A,S,D controls camera position.
        if (Input.GetKeyDown(KeyCode.D))
            cameraPos += cameraRight * PositionIncrement;
        else if (Input.GetKeyDown(KeyCode.A))
            cameraPos -= cameraRight * PositionIncrement;
        else if (Input.GetKeyDown(KeyCode.W))
            cameraPos += cameraForward * PositionIncrement;
        else if (Input.GetKeyDown(KeyCode.S))
            cameraPos -= cameraForward * PositionIncrement;

        // Q and E control camera elevation.
        else if (Input.GetKeyDown(KeyCode.E))
            cameraPos += cameraUp * PositionIncrement;
        else if (Input.GetKeyDown(KeyCode.Q))
            cameraPos -= cameraUp * PositionIncrement;

        // R resets demo parameters.
        else if (Input.GetKeyDown(KeyCode.R))
        {
            // Reset
            cameraPos = Vector3.zero;
        }

        // -------------
	}
}
