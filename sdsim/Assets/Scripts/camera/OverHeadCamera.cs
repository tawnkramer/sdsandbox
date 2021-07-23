using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverHeadCamera : MonoBehaviour
{
    public PathManager pathManager;
    public float margin = 5;
    public float height = 10;
    Camera cam;

    float previousHeight = 0;
    float previousWidth = 0;

    public void Init()
    {
        if (pathManager == null) { return; }
        if (pathManager.carPath == null) { return; }

        cam = GetComponent<Camera>();
        if (cam == null) { return; }

        cam.orthographic = true; // make sure the camera is orthographic


        List<Vector3> points = new List<Vector3>();
        float xmin = float.MaxValue;
        float xmax = float.MinValue;
        float zmin = float.MaxValue;
        float zmax = float.MinValue;

        Vector3 vxmin = new Vector3();
        Vector3 vzmin = new Vector3();

        foreach (PathNode centerNode in pathManager.carPath.centerNodes)
        {
            Vector3 pos = centerNode.pos;
            points.Add(pos);
            if (pos.x < xmin)
            {
                xmin = pos.x;
                vxmin = pos;
            }
            if (pos.z < zmin)
            {
                zmin = pos.z;
                vzmin = pos;
            }
            if (pos.x > xmax)
            {
                xmax = pos.x;
            }
            if (pos.z > zmax)
            {
                zmax = pos.z;
            }
        }

        // place the camera in the center of the circuit
        Vector3 center = new Vector3((xmin + xmax) / 2.0f, 0, (zmin + zmax) / 2.0f);
        transform.position = center + Vector3.up * height;
        transform.LookAt(center, Vector3.up);

        // try to best fit the camera to the screen
        Vector3 vpxmin = cam.WorldToViewportPoint(vxmin);
        Vector3 vpzmin = cam.WorldToViewportPoint(vzmin);

        float xdistanceToBorder = vpxmin.x;
        float zdistanceToBorder = vpzmin.y;

        float zoomValue = 1 - Mathf.Min(xdistanceToBorder, zdistanceToBorder) * 2;
        cam.orthographicSize = cam.orthographicSize * zoomValue + margin;
    }

    public void Update()
    {
        float currentHeight = Display.main.renderingHeight;
        float currentWidth = Display.main.renderingWidth;

        // if the resolution of the window changed, re-Init the camera
        if (currentHeight != previousHeight || currentWidth != previousWidth)
        {
            previousHeight = currentHeight;
            previousWidth = currentWidth;

            Init();
        }
    }

}
