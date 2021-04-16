using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceCameras : MonoBehaviour, IWaitCarPath
{
    public PathManager pathManager;
    List<RaceCamera> raceCameras = new List<RaceCamera>();
    List<int> nodeIndexes = new List<int>();
    List<float> deltaAngles = new List<float>();
    public Dictionary<int, int> carProgress = new Dictionary<int, int>();


    public float distanceBetweenCameras = 30.0f;
    public float roadWidth = 10.0f;
    public float roadHeight = 1.0f;
    public float cameraHeight = 10.0f;
    public float laneXOffset = 0.0f;
    public float startIndexOffset = 0.0f;

    float distance;

    public void Init()
    {
        if (!GlobalState.raceCameras) { return; }

        if (pathManager.carPath.centerNodes.Count < 1) { return; }

        // Detect where to put cameras
        Vector3 prev_point = pathManager.carPath.centerNodes[0].pos;
        nodeIndexes.Add(0);
        deltaAngles.Add(0);
        distance = startIndexOffset;

        for (int i = 1; i < pathManager.carPath.centerNodes.Count; i++)
        {

            PathNode node = pathManager.carPath.centerNodes[i];
            PathNode nextNode = pathManager.carPath.centerNodes[(i + 1) % pathManager.carPath.centerNodes.Count];

            float deltaAngle = Vector3.SignedAngle(nextNode.pos - node.pos, node.rotation * Vector3.forward, Vector3.up);
            deltaAngles.Add(deltaAngle);

            distance = Vector3.Distance(node.pos, nextNode.pos) + distance;
            if (distance > distanceBetweenCameras)
            {
                nodeIndexes.Add(i);
                prev_point = node.pos;
                distance = 0;
            }

        }

        // Add cameras
        for (int i = 0; i < nodeIndexes.Count; i++)
        {
            int nodeIndex;
            if (i < nodeIndexes.Count - 1)
            {
                nodeIndex = ((nodeIndexes[i] + nodeIndexes[(i + 1)]) / 2) % pathManager.carPath.centerNodes.Count;
            }
            else
            {
                nodeIndex = ((nodeIndexes[nodeIndexes.Count - 1] + nodeIndexes[0] + pathManager.carPath.centerNodes.Count) / 2) % pathManager.carPath.centerNodes.Count;
            }
            float sign = Mathf.Sign(deltaAngles[nodeIndex]);
            PathNode node = pathManager.carPath.centerNodes[nodeIndexes[i]];
            PathNode midNode = pathManager.carPath.centerNodes[nodeIndex];
            Vector3 nodepos = node.pos + node.rotation * (laneXOffset * Vector3.right);
            Vector3 midNodepos = midNode.pos + midNode.rotation * (laneXOffset * Vector3.right);


            GameObject goRaceCamChild = new GameObject(string.Format("RaceCamera {0}", i));
            goRaceCamChild.transform.SetParent(transform);
            RaceCamera cmp = goRaceCamChild.AddComponent<RaceCamera>();
            cmp.SetCameraTrigger(nodepos, node.rotation * Quaternion.AngleAxis(90, Vector3.up), new Vector3(0.1f, roadHeight, roadWidth));
            cmp.SetCam(midNodepos + midNode.rotation * (6f * sign * Vector3.right) + (cameraHeight * Vector3.up), midNodepos);
            cmp.index = i;
            raceCameras.Add(cmp);
        }

        // Enable first camera
        raceCameras[0].camera.enabled = true;

        float coverage = GetCoverage(raceCameras.ToArray(), pathManager.carPath.centerNodes.ToArray(), nodeIndexes.ToArray());
        Debug.Log(string.Format("Race cameras coverage: {0}%", coverage));

    }

    public void EnableCameras(bool enabled)
    {
        foreach (RaceCamera raceCamera in raceCameras)
        {
            raceCamera.camera.enabled = enabled;
        }
    }

    public void CameraTriggered(Collider col, Camera camera, int index)
    {
        int carID = col.attachedRigidbody.GetInstanceID();

        if (carProgress.ContainsKey(carID))
        {
            carProgress[carID] = index;

            int furtherValue = int.MinValue;
            int furtherID = 0;
            foreach (int key in carProgress.Keys)
            {

                if (carProgress[key] > furtherValue)
                {
                    furtherValue = carProgress[key];
                    furtherID = key;
                }

            }

            if (furtherID != 0)
            {
                EnableCameras(false);
                raceCameras[carProgress[furtherID]].camera.enabled = true;
            }
        }
        else
        {
            carProgress.Add(carID, index);
        }

    }

    public float GetCoverage(RaceCamera[] raceCams, PathNode[] nodes, int[] nodeIndexes)
    {
        if (raceCams.Length == nodeIndexes.Length)
        {
            int count = 0;

            for (int i = 0; i < nodeIndexes.Length; i++)
            {
                int from = nodeIndexes[i];
                int to = nodeIndexes[(i + 1) % nodeIndexes.Length];
                if (i + 1 >= nodeIndexes.Length) { to += nodes.Length; }


                for (int j = from; j < to; j++)
                {
                    PathNode node = nodes[j % nodes.Length];

                    bool isSeenByCamera = IsSeenByCamera(raceCams[i].camera, node.pos + node.rotation * (laneXOffset * Vector3.right));
                    if (isSeenByCamera) { count++; }
                }


            }

            return ((float)count / (float)nodes.Length) * 100.0f; // get coverage percentage
        }

        else
        {
            Debug.LogWarning("No race camera found"); // no cameras found
            return 0f;
        }
    }

    bool IsSeenByCamera(Camera camera, Vector3 position)
    {
        // check whether the object is visible by the camera
        Vector3 viewPos = camera.WorldToViewportPoint(position);
        if (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0)
        {
            return true;
        }
        else
            return false;
    }
}
