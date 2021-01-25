using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class RepeatObjectAlong : MonoBehaviour, IWaitCarPath
{
    public PathManager pathManager; // we will use the carPath from the pathManager
    public float widthOffset = 1f;
    public float rotateYOffset = 0f;
    public float objectScaling = 1f;
    public float placeEvery = 5f;
    public bool leftside = true;
    public bool rightside = true;
    public bool mirrorRotateObject = true;
    public GameObject goToRepeat;
    public GameObject parentObject;

    private List<CombineInstance> combineInstances = new List<CombineInstance>();

    public void Init()
    {
        if (pathManager != null)
        {
            Vector3 leftScaling = Vector3.one * objectScaling;
            Vector3 rightScaling = Vector3.one * objectScaling;
            if (mirrorRotateObject)
            {
                rightScaling.x = -rightScaling.x;
            }

            Quaternion rotationOffset = Quaternion.AngleAxis(rotateYOffset, Vector3.up);

            PathNode node = pathManager.carPath.centerNodes[0];
            PathNode prev_node = pathManager.carPath.centerNodes[pathManager.carPath.centerNodes.Count - 1];

            Vector3 left_prev_position = prev_node.pos + node.rotation * (Vector3.left * widthOffset);
            Quaternion left_prev_rotation = prev_node.rotation;

            Vector3 right_prev_position = prev_node.pos + node.rotation * (Vector3.right * widthOffset);
            Quaternion right_prev_rotation = prev_node.rotation;

            for (int i = 0; i < pathManager.carPath.centerNodes.Count; i++)
            {
                node = pathManager.carPath.centerNodes[i];

                if (leftside)
                {
                    float prev_distance = Vector3.Distance(left_prev_position, node.pos + node.rotation * (Vector3.left * widthOffset));

                    if (prev_distance >= placeEvery)
                    {
                        float t_factor = placeEvery / prev_distance;

                        Vector3 position = Vector3.Lerp(left_prev_position, node.pos + node.rotation * (Vector3.left * widthOffset), t_factor);
                        Quaternion rotation = Quaternion.Lerp(left_prev_rotation, node.rotation, t_factor) * rotationOffset;

                        GameObject go = Instantiate(goToRepeat);
                        if (parentObject != null)
                            go.transform.parent = parentObject.transform;
                        go.transform.localScale = leftScaling;
                        go.transform.SetPositionAndRotation(position, rotation);

                        MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer mr in meshRenderers)
                        {
                            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        }

                        left_prev_position = position;
                        left_prev_rotation = rotation;
                    }
                }

                if (rightside)
                {
                    float prev_distance = Vector3.Distance(right_prev_position, node.pos + node.rotation * (Vector3.right * widthOffset));

                    if (prev_distance >= placeEvery)
                    {
                        float t_factor = placeEvery / prev_distance;

                        Vector3 position = Vector3.Lerp(right_prev_position, node.pos + node.rotation * (Vector3.right * widthOffset), t_factor);
                        Quaternion rotation = Quaternion.Lerp(right_prev_rotation, node.rotation, t_factor) * rotationOffset;

                        GameObject go = Instantiate(goToRepeat);
                        if (parentObject != null)
                            go.transform.parent = parentObject.transform;
                        go.transform.localScale = rightScaling;
                        go.transform.SetPositionAndRotation(position, rotation);

                        MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer mr in meshRenderers)
                        {
                            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        }

                        right_prev_position = position;
                        right_prev_rotation = rotation;
                        prev_node = node;
                    }

                }
                prev_node = node;

            }
        }
    }
}