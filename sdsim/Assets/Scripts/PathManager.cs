using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using PathCreation;


public class PathManager : MonoBehaviour
{
    public CarPath carPath;
    public PathCreator pathCreator;

    [Header("Path type")]
    public bool doMakeRandomPath = true;
    public bool doLoadScriptPath = false;
    public bool doLoadPointPath = false;
    public bool doLoadGameObjectPath = false;

    [Header("Path making")]
    public Transform startPos;
    public string pathToLoad = "none";
    public int smoothPathIter = 0;
    public GameObject locationMarkerPrefab;
    public int markerEveryN = 2;
    public bool doChangeLanes = false;
    public bool invertNodes = false;

    [Header("Random path parameters")]
    public int numSpans = 100;
    public float turnInc = 1f;
    public float spanDist = 5f;
    public bool sameRandomPath = true;
    public int randSeed = 2;

    [Header("Debug")]
    public bool doShowNodePath = false;
    public bool doShowCenterNodePath = false;
    public GameObject pathelem;

    [Header("Aux")]
    public GameObject[] initAfterCarPathLoaded; // Scripts using the IWaitCarPath interface to init after loading the CarPath
    public GameObject[] challenges; // Challenges using the IWaitCarPath interface to init after loading the CarPath or on private API call

    Vector3 span = Vector3.zero;
    GameObject generated_mesh;

    void Awake()
    {
        if (sameRandomPath)
            Random.InitState(randSeed);

        InitCarPath();
    }

    public void InitCarPath()
    {
        if (doMakeRandomPath)
        {
            MakeRandomPath();
        }
        else if (doLoadScriptPath)
        {
            MakeScriptedPath();
        }
        else if (doLoadPointPath)
        {
            MakePointPath();
        }
        else if (doLoadGameObjectPath)
        {
            MakeGameObjectPath();
        }

        if (carPath == null) // if no carPath was created, skip the following block of code
        {
            return;
        }

        if (invertNodes)
        {
            CarPath new_carPath = new CarPath();
            for (int i = carPath.nodes.Count - 1; i >= 0; i--)
            {
                PathNode node = carPath.nodes[i];
                new_carPath.nodes.Add(node);
                new_carPath.centerNodes.Add(node);
            }
            carPath = new_carPath;
        }

        if (startPos != null)
        {
            // Get the closest point to the start and make it index 0 of carPath
            int startIndex = 0;
            float closest = float.MaxValue;
            for (int i = 0; i < carPath.nodes.Count; i++)
            {
                PathNode node = carPath.nodes[i];
                float distance = Vector3.Distance(node.pos, startPos.position);
                if (distance < closest)
                {
                    closest = distance;
                    startIndex = i;
                }
            }

            if (startIndex != 0)
            {
                CarPath new_carPath = new CarPath();
                for (int i = startIndex; i < carPath.nodes.Count + startIndex; i++)
                {
                    if (i % carPath.nodes.Count == 0) { continue; } // avoid two consecutive values to be the same

                    PathNode node = carPath.nodes[i % carPath.nodes.Count];
                    new_carPath.nodes.Add(node);
                    new_carPath.centerNodes.Add(node);

                }
                // close the loop
                new_carPath.nodes.Add(new_carPath.nodes[0]);
                new_carPath.centerNodes.Add(new_carPath.nodes[0]);

                carPath = new_carPath;
            }
        }

        // execute in the next update loop
        UnityMainThreadDispatcher.Instance().Enqueue(InitAfterCarPathLoaded(initAfterCarPathLoaded));
        UnityMainThreadDispatcher.Instance().Enqueue(InitAfterCarPathLoaded(challenges));

        // if (locationMarkerPrefab != null && carPath != null)
        // {
        //     int iLocId = 0;
        //     for (int iN = 0; iN < carPath.nodes.Count; iN += markerEveryN)
        //     {
        //         Vector3 np = carPath.nodes[iN].pos;
        //         GameObject go = Instantiate(locationMarkerPrefab, np, Quaternion.identity) as GameObject;
        //         go.transform.parent = this.transform;
        //         go.GetComponent<LocationMarker>().id = iLocId;
        //         iLocId++;
        //     }
        // }

        if (doShowNodePath)
        {
            for (int iN = 0; iN < carPath.nodes.Count; iN++)
            {
                Vector3 np = carPath.nodes[iN].pos;
                Quaternion rotation = carPath.nodes[iN].rotation;
                GameObject go = Instantiate(pathelem, np, rotation) as GameObject;
                go.tag = "pathNode";
                go.transform.parent = this.transform;
            }
        }

        if (doShowCenterNodePath)
        {
            for (int iN = 0; iN < carPath.centerNodes.Count; iN++)
            {
                Vector3 np = carPath.centerNodes[iN].pos;
                Quaternion rotation = carPath.centerNodes[iN].rotation;
                GameObject go = Instantiate(pathelem, np, rotation) as GameObject;
                go.tag = "pathNode";
                go.transform.parent = this.transform;
            }
        }
    }

    public IEnumerator InitAfterCarPathLoaded(GameObject[] scriptList)
    {
        if (carPath != null)
        {
            foreach (GameObject go in scriptList) // Init each Object that need a carPath
            {
                try
                {
                    IWaitCarPath script = go.GetComponent<IWaitCarPath>();
                    if (script != null)
                    {
                        script.Init();
                    }
                    else
                    {
                        Debug.LogError("Provided GameObject doesn't contain an IWaitCarPath script");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError(string.Format("Could not initialize: {0}, Exception: {1}", go.name, e));
                }
            }
        }

        else
        {
            Debug.LogError("No carPath loaded"); yield return null;
        }
        yield return null;
    }

    public void DestroyRoad() // old, need refactoring in RoadBuilder
    {
        GameObject[] prev = GameObject.FindGameObjectsWithTag("pathNode");

        foreach (GameObject g in prev)
            Destroy(g);

        // if (roadBuilder != null)
        //     roadBuilder.DestroyRoad();
    }

    public Vector3 GetPathStart()
    {
        return startPos.position;
    }

    public Vector3 GetPathEnd()
    {
        int iN = carPath.nodes.Count - 1;

        if (iN < 0)
            return GetPathStart();

        return carPath.nodes[iN].pos;
    }

    float nfmod(float a, float b) // formula for negative and positive modulo
    {
        return a - b * Mathf.Floor(a / b);
    }

    void MakeGameObjectPath(float precision = 3f)
    {
        carPath = new CarPath();

        List<Vector3> points = new List<Vector3>();

        float stepping = 1 / (pathCreator.path.length * precision);
        for (float i = 0; i <= 1; i += stepping)
        {
            points.Add(pathCreator.path.GetPointAtTime(i));
        }
        points.Add(pathCreator.path.GetPointAtTime(0));


        while (smoothPathIter > 0) // not working for the moment, looking forward using the same system as MakePointPath with LookAt
        {
            points = Chaikin(points);
            smoothPathIter--;
        }

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 point = points[(int)nfmod(i, (points.Count - 1))];
            Vector3 previous_point = points[(int)nfmod(i - 1, (points.Count - 1))];
            Vector3 next_point = points[(int)nfmod(i + 1, (points.Count - 1))];

            PathNode p = new PathNode();
            p.pos = point;
            p.rotation = Quaternion.LookRotation(next_point - previous_point, Vector3.up); ;
            carPath.nodes.Add(p);
            carPath.centerNodes.Add(p);
        }
    }

    void MakePointPath()
    {
        string filename = pathToLoad;

        TextAsset bindata = Resources.Load("Track/" + filename) as TextAsset;

        if (bindata == null)
            return;

        string[] lines = bindata.text.Split('\n');

        Debug.Log(string.Format("found {0} path points. to load", lines.Length));

        carPath = new CarPath();

        Vector3 np = Vector3.zero;

        float offsetY = -0.1f;
        List<Vector3> points = new List<Vector3>();

        foreach (string line in lines)
        {
            string[] tokens = line.Split(',');

            if (tokens.Length != 3)
                continue;
            np.x = float.Parse(tokens[0], CultureInfo.InvariantCulture.NumberFormat);
            np.y = float.Parse(tokens[1], CultureInfo.InvariantCulture.NumberFormat) + offsetY;
            np.z = float.Parse(tokens[2], CultureInfo.InvariantCulture.NumberFormat);

            points.Add(np);
        }

        while (smoothPathIter > 0)
        {
            points = Chaikin(points);
            smoothPathIter--;
        }

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 point = points[(int)nfmod(i, (points.Count))];
            Vector3 previous_point = points[(int)nfmod(i - 1, (points.Count))];
            Vector3 next_point = points[(int)nfmod(i + 1, (points.Count))];

            PathNode p = new PathNode();
            p.pos = point;
            p.rotation = Quaternion.LookRotation(next_point - previous_point, Vector3.up); ;
            carPath.nodes.Add(p);
            carPath.centerNodes.Add(p);
        }
    }

    public List<Vector3> Chaikin(List<Vector3> pts)
    {
        List<Vector3> newPts = new List<Vector3>();

        newPts.Add(pts[0]);

        for (int i = 0; i < pts.Count - 2; i++)
        {
            newPts.Add(pts[i] + (pts[i + 1] - pts[i]) * 0.75f);
            newPts.Add(pts[i + 1] + (pts[i + 2] - pts[i + 1]) * 0.25f);
        }

        // newPts.Add(pts[pts.Count - 1]);
        return newPts;
    }


    void MakeScriptedPath()
    {
        TrackScript script = new TrackScript();

        if (script.Read(pathToLoad))
        {
            carPath = new CarPath();
            TrackParams tparams = new TrackParams();
            tparams.numToSet = 0;
            tparams.rotCur = Quaternion.identity;
            tparams.lastPos = startPos.position;

            float dY = 0.0f;
            float turn = 0f;

            Vector3 s = startPos.position;
            s.y = 0.5f;
            span.x = 0f;
            span.y = 0f;
            span.z = spanDist;
            float turnVal = 10.0f;

            List<Vector3> points = new List<Vector3>();

            foreach (TrackScriptElem se in script.track)
            {
                if (se.state == TrackParams.State.AngleDY)
                {
                    turnVal = se.value;
                }
                else if (se.state == TrackParams.State.CurveY)
                {
                    turn = 0.0f;
                    dY = se.value * turnVal;
                }
                else
                {
                    dY = 0.0f;
                    turn = 0.0f;
                }

                for (int i = 0; i < se.numToSet; i++)
                {

                    Vector3 np = s;
                    PathNode p = new PathNode();
                    p.pos = np;
                    points.Add(np);

                    turn = dY;

                    Quaternion rot = Quaternion.Euler(0.0f, turn, 0f);
                    span = rot * span.normalized;
                    span *= spanDist;
                    s = s + span;
                }

            }


            for (int i = 0; i < points.Count; i++)
            {
                Vector3 point = points[(int)nfmod(i, (points.Count))];
                Vector3 previous_point = points[(int)nfmod(i - 1, (points.Count))];
                Vector3 next_point = points[(int)nfmod(i + 1, (points.Count))];

                PathNode p = new PathNode();
                p.pos = point;
                p.rotation = Quaternion.LookRotation(next_point - previous_point, Vector3.up); ;
                carPath.nodes.Add(p);
                carPath.centerNodes.Add(p);
            }

        }
    }

    void MakeRandomPath()
    {
        carPath = new CarPath();

        Vector3 s = startPos.position;
        float turn = 0f;
        s.y = 0.5f;

        span.x = 0f;
        span.y = 0f;
        span.z = spanDist;

        List<Vector3> points = new List<Vector3>();

        for (int iS = 0; iS < numSpans; iS++)
        {
            Vector3 np = s;
            points.Add(np);

            float t = UnityEngine.Random.Range(-1.0f * turnInc, turnInc);

            turn += t;

            Quaternion rot = Quaternion.Euler(0.0f, turn, 0f);
            span = rot * span.normalized;

            if (SegmentCrossesPath(np + (span.normalized * 100.0f), 90.0f, points.ToArray()))
            {
                //turn in the opposite direction if we think we are going to run over the path
                turn *= -0.5f;
                rot = Quaternion.Euler(0.0f, turn, 0f);
                span = rot * span.normalized;
            }

            span *= spanDist;

            s = s + span;
        }

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 point = points[(int)nfmod(i, (points.Count))];
            Vector3 previous_point = points[(int)nfmod(i - 1, (points.Count))];
            Vector3 next_point = points[(int)nfmod(i + 1, (points.Count))];

            PathNode p = new PathNode();
            p.pos = point;
            p.rotation = Quaternion.LookRotation(next_point - previous_point, Vector3.up); ;
            carPath.nodes.Add(p);
            carPath.centerNodes.Add(p);
        }

    }

    public bool SegmentCrossesPath(Vector3 posA, float rad, Vector3[] posN)
    {
        foreach (Vector3 pn in posN)
        {
            float d = (posA - pn).magnitude;

            if (d < rad)
                return true;
        }

        return false;
    }

    public void SetPath(CarPath p)
    {
        carPath = p;

        GameObject[] prev = GameObject.FindGameObjectsWithTag("pathNode");

        Debug.Log(string.Format("Cleaning up {0} old nodes. {1} new ones.", prev.Length, p.nodes.Count));

        DestroyRoad();

        foreach (PathNode pn in carPath.nodes)
        {
            GameObject go = Instantiate(pathelem, pn.pos, Quaternion.identity) as GameObject;
            go.tag = "pathNode";
        }
    }
}
