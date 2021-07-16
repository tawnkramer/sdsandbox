using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverHeadCamera : MonoBehaviour
{
    public PathManager pathManager;
    public float margin = 5;
    public float height = 10;
    Camera cam;

    public void Init ()
    {
        if (pathManager == null) { return; }

        cam = GetComponent<Camera>();
        if (cam == null) { return; }

        cam.orthographic = true; // make sure the camera is orthographic


        List<Vector3> points = new List<Vector3>();
        float xmin = float.MaxValue;
        float xmax = float.MinValue;
        float zmin = float.MaxValue;
        float zmax = float.MinValue;

        foreach (PathNode centerNode in pathManager.carPath.centerNodes)
        {
            Vector3 pos = centerNode.pos;
            points.Add(pos);
            if (pos.x < xmin)
                xmin = pos.x;
            if (pos.z < zmin)
                zmin = pos.z;
            if (pos.x > xmax)
                xmax = pos.x;
            if (pos.z > zmax)
                zmax = pos.z;
        }

        // place the camera in the center of the path
        Vector3 center = new Vector3((xmin + xmax) / 2.0f, 0, (zmin + zmax) / 2.0f);
        transform.position = center + Vector3.up * height;

        float xSize = Mathf.Abs(xmax - xmin);
        float zSize = Mathf.Abs(zmax - zmin);
        float camSize = Mathf.Max(xSize, zSize);

        cam.orthographicSize = (camSize / 2) + margin;
        // try to best fit the camera to the screen
    }
}
