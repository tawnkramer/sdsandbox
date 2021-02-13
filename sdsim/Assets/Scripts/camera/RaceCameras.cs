using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceCameras : MonoBehaviour, IWaitCarPath
{
    public PathManager pathManager;
    List<RaceCamera> raceCameras = new List<RaceCamera>();
    List<int> NodeIndexes = new List<int>();
    List<float> deltaAngles = new List<float>();

    public float distanceBetweenCameras = 30.0f;
    public float roadWidth = 10.0f;
    public float cameraHeight = 10.0f;
    float distance;

    public void Init()
    {
        if (!GlobalState.raceCameras) { return;}

        if (pathManager.carPath.centerNodes.Count < 1) { return; }

        // Detect where to put cameras
        Vector3 prev_point = pathManager.carPath.centerNodes[0].pos;
        NodeIndexes.Add(0);
        for (int i = 0; i < pathManager.carPath.centerNodes.Count; i++)
        {

            PathNode node = pathManager.carPath.centerNodes[i];
            PathNode nextNode = pathManager.carPath.centerNodes[(i + 1) % pathManager.carPath.centerNodes.Count];

            float deltaAngle = Vector3.SignedAngle(nextNode.pos - node.pos, node.rotation * Vector3.forward, Vector3.up);
            deltaAngles.Add(deltaAngle);

            distance = Vector3.Distance(node.pos, nextNode.pos) + distance;
            if (distance > distanceBetweenCameras)
            {
                NodeIndexes.Add(i);
                prev_point = node.pos;
                distance = 0;
            }

        }

        // Add cameras
        for (int i = 0; i < NodeIndexes.Count; i++)
        {
            int nodeIndex;
            if (i < NodeIndexes.Count - 1)
            {
                nodeIndex = ((NodeIndexes[i] + NodeIndexes[(i + 1)]) / 2) % pathManager.carPath.centerNodes.Count;
            }
            else
            {
                nodeIndex = ((NodeIndexes[i] + NodeIndexes[0] + pathManager.carPath.centerNodes.Count) / 2) % pathManager.carPath.centerNodes.Count;
            }
            float sign = Mathf.Sign(deltaAngles[nodeIndex]);
            PathNode node = pathManager.carPath.centerNodes[NodeIndexes[i]];
            PathNode midNode = pathManager.carPath.centerNodes[nodeIndex];

            GameObject goRaceCamChild = new GameObject(string.Format("RaceCamera {0}", i));
            goRaceCamChild.transform.SetParent(transform);
            RaceCamera cmp = goRaceCamChild.AddComponent<RaceCamera>();
            cmp.SetCameraTrigger(node.pos, node.rotation * Quaternion.AngleAxis(90, Vector3.up), new Vector3(0.1f, 1, roadWidth));
            cmp.SetCam(midNode.pos + midNode.rotation * (6f * sign * Vector3.right) + (cameraHeight * Vector3.up), midNode.pos);
            raceCameras.Add(cmp);
        }
    }

    public void EnableCameras(bool enabled)
    {
        foreach (RaceCamera raceCamera in raceCameras)
        {
            raceCamera.camera.enabled = enabled;
        }
    }
}
