using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RepeatObjectAlong : MonoBehaviour, IWaitCarPath
{
    public PathManager pathManager; // we will use the carPath from the pathManager

    [Header("Offsets")]
    public float leftWidthOffset = 1f;
    public float rightWidthOffset = 1f;
    public float heightOffset = 0f;
    public float rotateYOffset = 0f;

    [Header("Repeating params")]
    public float objectScaling = 1f;
    public float placeEvery = 5f;

    [Header("Generation params")]
    public bool generateAtRuntime = true;
    public bool leftside = true;
    public bool rightside = true;
    public bool mirrorRotateObject = true; // apply a mirror rotation to the right side
    public Mesh meshToRepeat;

    public string savePath = "Assets\\object_mesh.asset";

    private List<CombineInstance> combineInstances;

    public void Generate()
    {
        if (pathManager != null && pathManager.carPath != null)
        {
            combineInstances = new List<CombineInstance>();

            Vector3 leftScaling = Vector3.one * objectScaling;
            Vector3 rightScaling = Vector3.one * objectScaling;
            if (mirrorRotateObject)
            {
                rightScaling.x = -rightScaling.x;
            }

            Quaternion leftRotationOffset = Quaternion.AngleAxis(rotateYOffset, Vector3.up);
            Quaternion rightRotationOffset = Quaternion.AngleAxis(-rotateYOffset, Vector3.up);

            PathNode node = pathManager.carPath.centerNodes[0];
            PathNode prevNode = pathManager.carPath.centerNodes[pathManager.carPath.centerNodes.Count - 1];

            Vector3 leftPrevPosition = prevNode.pos + node.rotation * (Vector3.left * leftWidthOffset);
            Vector3 rightPrevPosition = prevNode.pos + node.rotation * (Vector3.right * rightWidthOffset);


            List<Vector3> leftPositions = new List<Vector3>();
            leftPositions.Add(node.pos + node.rotation * (Vector3.left * leftWidthOffset));
            List<Vector3> rightPositions = new List<Vector3>();
            rightPositions.Add(node.pos + node.rotation * (Vector3.right * rightWidthOffset));

            for (int i = 0; i < pathManager.carPath.centerNodes.Count; i++)
            {
                node = pathManager.carPath.centerNodes[i];

                if (leftside)
                {
                    float prev_distance = Vector3.Distance(leftPrevPosition, node.pos + node.rotation * (Vector3.left * leftWidthOffset));

                    if (prev_distance >= placeEvery)
                    {
                        float t_factor = placeEvery / prev_distance;

                        Vector3 position = Vector3.Lerp(leftPrevPosition, node.pos + node.rotation * (Vector3.left * leftWidthOffset), t_factor);
                        leftPositions.Add(position);

                        leftPrevPosition = position;
                    }
                }

                if (rightside)
                {
                    float prev_distance = Vector3.Distance(rightPrevPosition, node.pos + node.rotation * (Vector3.right * rightWidthOffset));

                    if (prev_distance >= placeEvery)
                    {
                        float t_factor = placeEvery / prev_distance;

                        Vector3 position = Vector3.Lerp(rightPrevPosition, node.pos + node.rotation * (Vector3.right * rightWidthOffset), t_factor);
                        rightPositions.Add(position);

                        rightPrevPosition = position;
                    }

                }
                prevNode = node;

            }

            // go through every points and try to snap the mesh on them
            for (int i = 0; i < leftPositions.Count; i++) // left side
            {

                Vector3 position = leftPositions[i];
                Vector3 nextPosition = leftPositions[(i + 1) % leftPositions.Count];

                Quaternion rot = Quaternion.LookRotation(nextPosition - position, Vector3.up) * leftRotationOffset;
                Matrix4x4 transform = Matrix4x4.TRS((nextPosition + position) / 2 + heightOffset * Vector3.up, rot, leftScaling);

                Mesh mesh = Instantiate(meshToRepeat);
                CombineInstance comb = new CombineInstance();
                comb.mesh = mesh;
                comb.transform = transform;
                combineInstances.Add(comb);
            }

            for (int i = 0; i < rightPositions.Count; i++) // right side
            {

                Vector3 position = rightPositions[i];
                Vector3 nextPosition = rightPositions[(i + 1) % rightPositions.Count];

                Quaternion rot = Quaternion.LookRotation(nextPosition - position, Vector3.up) * rightRotationOffset;
                Matrix4x4 transform = Matrix4x4.TRS((nextPosition + position) / 2 + heightOffset * Vector3.up, rot, rightScaling);

                Mesh mesh = Instantiate(meshToRepeat);
                CombineInstance comb = new CombineInstance();
                comb.mesh = mesh;
                comb.transform = transform;
                combineInstances.Add(comb);
            }


            // combine meshes into a unique mesh
            MeshFilter mf = GetComponent<MeshFilter>();
            MeshRenderer mr = GetComponent<MeshRenderer>();
            MeshCollider mc = GetComponent<MeshCollider>();
            Mesh finalMesh = mf.sharedMesh;

            if (finalMesh == null)
            {
                finalMesh = new Mesh();
            }

            finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            finalMesh.CombineMeshes(combineInstances.ToArray(), true);
            finalMesh.Optimize();
            finalMesh.RecalculateBounds();
            finalMesh.RecalculateNormals();
            finalMesh.RecalculateTangents();

            mf.sharedMesh = finalMesh;
            if (mc != null)
            {
                mc.sharedMesh = finalMesh;
            }
        }
    }
    public void Init()
    {
        if (generateAtRuntime)
        {
            Generate();
        }
    }

    public void SaveMesh()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = mf.sharedMesh;
        if (mesh == null)
        {
            Debug.LogWarning("Mesh is null, creating a new one");
            mesh = new Mesh();
        }
        AssetDatabase.CreateAsset(mesh, savePath);
    }
}