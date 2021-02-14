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
    int lastTriggered = 0;

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

            GameObject goRaceCamChild = new GameObject(string.Format("RaceCamera {0}", i));
            goRaceCamChild.transform.SetParent(transform);
            RaceCamera cmp = goRaceCamChild.AddComponent<RaceCamera>();
            cmp.SetCameraTrigger(node.pos + node.rotation * (laneXOffset * Vector3.right), node.rotation * Quaternion.AngleAxis(90, Vector3.up), new Vector3(0.1f, roadHeight, roadWidth));
            cmp.SetCam(midNode.pos + midNode.rotation * (6f * sign * Vector3.right) + (cameraHeight * Vector3.up), midNode.pos);
            cmp.index = i;
            raceCameras.Add(cmp);
        }

        // Enable first camera
        raceCameras[0].camera.enabled = true;
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
            Debug.Log("Adding car key");
            carProgress.Add(carID, index);
        }

    }

    public float GetCoverage()
    {
        if (raceCameras.Count > 0)
        {
            // TODO
            return 1f;

        }

        else
        {
            return 0f;
        }
    }
}
