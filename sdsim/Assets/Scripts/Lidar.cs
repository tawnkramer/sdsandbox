using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LidarPoint
{
    public float x;
    public float y;
    public float z;
    public float d;
    public float rx;
    public float ry;

    public LidarPoint(Vector3 p, float distance, float _rx, float _ry)
    {
        x = p.x;
        y = p.y;
        z = p.z;
        d = distance;
        rx = _rx;
        ry = _ry;
    }
}

[Serializable]
public class V3
{
    public float x;
    public float y;
    public float z;

    public V3(Vector3 p)
    {
        x = p.x;
        y = p.y;
        z = p.z;
    }
}

[Serializable]
public class LidarPointArray
{
    //All coordinates in World coordinate system

    public LidarPoint[] points;

    public void Init(int numPoints)
    {
        points = new LidarPoint[numPoints];
    }
}

public class Lidar : MonoBehaviour
{


    public LidarPointArray pointArr;

    //as the ray sweeps around, how many degrees does it advance per sample
    public float degPerSweepInc = 2f;

    //what is the starting angle for the initial sweep compared to the forward vector
    public float degAngDown = 25f;

    //what angle change between sweeps 
    public float degAngDelta = -1f;

    //how many complete 360 sweeps
    public int numSweepsLevels = 25;

    //what it max distance we will register a hit
    public float maxRange = 50f;

    //how large radius to use when rendering debug display
    public float gizmoSize = 0.1f;

    //what is the scalar on the perlin noise applied to point position
    public float noise = 0.2f;

    public bool DisplayDebugInScene = false;

    // are there layers we don't want to collide with?
    public string[] layerMaskNames;

    int collMask = 0;

    void Awake()
    {
        pointArr = new LidarPointArray();
        pointArr.Init((int)(360 / degPerSweepInc * numSweepsLevels));

        int v = 0;

        foreach (string layerName in layerMaskNames)
        {
            int layer = LayerMask.NameToLayer(layerName);
            v |= 1 << layer;
        }

        collMask |= ~v;

    }

    public void SetConfig(float offset_x, float offset_y, float offset_z, float rot_x,
        float _degPerSweepInc, float _degAngDown, float _degAngDelta, float _maxRange, float _noise, int _numSweepsLevels)
    {
        degPerSweepInc = _degPerSweepInc;
        degAngDown = _degAngDown;
        degAngDelta = _degAngDelta;
        maxRange = _maxRange;
        noise = _noise;
        numSweepsLevels = _numSweepsLevels;

        if (offset_x != 0.0f || offset_y != 0.0f || offset_z != 0.0f)
            transform.localPosition = new Vector3(offset_x, offset_y, offset_z);

        if (rot_x != 0.0f)
            transform.localEulerAngles = new Vector3(rot_x, 0.0f, 0.0f);

        pointArr = new LidarPointArray();
        pointArr.Init((int)(360 / degPerSweepInc * numSweepsLevels));
    }

    public JSONObject GetOutputAsJson()
    {
        LidarPointArray points = GetOutput();
        JSONObject json = JSONObject.Create();
        foreach (LidarPoint p in points.points)
        {
            JSONObject vec = JSONObject.Create();
            try
            {
                vec.AddField("d", (float)Math.Round(p.d, 2));
                vec.AddField("rx", p.rx);
                vec.AddField("ry", p.ry);
                json.Add(vec);
            }
            catch
            {
                // just ignore points that don't resolve.
            }
        }

        return json;
    }

    public LidarPointArray GetOutput()
    {
        int numSweep = (int)(360 / degPerSweepInc);
        pointArr = new LidarPointArray();
        pointArr.Init(numSweep * numSweepsLevels);

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Vertical rotation
        Quaternion rotUp = Quaternion.AngleAxis(degAngDelta, transform.right);

        // Horizontal rotation
        Quaternion rotSide = Quaternion.AngleAxis(degPerSweepInc, transform.up);

        //Sample the output texture to create rays.
        int iP = 0;
        float rx = 0.0f;
        float ry = 0.0f;

        for (int iS = 0; iS < numSweepsLevels; iS++)
        {
            // reset the orientation of the ray
            Quaternion rotDown = Quaternion.AngleAxis(degAngDown + iS * degAngDelta, transform.right);
            ray.direction = rotDown * transform.forward;
            rx = 0.0f;

            for (int iA = 0; iA < numSweep; iA++)
            {
                if (Physics.Raycast(ray, out hit, maxRange, collMask))
                {
                    //sample that ray at the depth given by the pixel.
                    Vector3 pos = hit.point - transform.position;
                    float distance = hit.distance;

                    //shouldn't hit this unless user is messing around in the interface with things running.
                    if (iP >= pointArr.points.Length)
                        break;

                    // add some noise to the point position
                    float noiseX = Mathf.PerlinNoise(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
                    float noiseY = Mathf.PerlinNoise(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
                    float noiseZ = Mathf.PerlinNoise(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
                    pos.x += noise * noiseX;
                    pos.y += noise * noiseY;
                    pos.z += noise * noiseZ;

                    //set iPoint
                    pointArr.points[iP] = new LidarPoint(pos, distance, rx, ry);
                    iP++;
                }

                ray.direction = rotSide * ray.direction;
                rx += degPerSweepInc;
            }

            ray.direction = rotUp * ray.direction;
            ry += degAngDelta;
        }

        return pointArr;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        pointArr = GetOutput(); // not really efficient but won't be called in the app, just for visualization purpose

        Vector3 pos = Vector3.zero;
        foreach (LidarPoint p in pointArr.points)
        {
            if (p == null)
                continue;

            pos.x = p.x;
            pos.y = p.y;
            pos.z = p.z;

            //make points global space for drawing
            pos += transform.position;
            Gizmos.DrawSphere(pos, gizmoSize);
        }
    }
}
