using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class RepeatObjectAlong : MonoBehaviour, IWaitCarPath
{
    public PathManager pathManager; // we will use the carPath from the pathManager
    public Mesh[] meshesToRepeat;

    [Header("Offsets")]
    public float leftWidthOffset = 1f;
    public float rightWidthOffset = 1f;
    public float[] yOffset;
    public float[] xOffset;
    public float[] rotateYOffset;
    public float[] rotateXOffset;

    [Header("Repeating params")]
    public Vector3 MeshScale = new Vector3(1, 1, 1);
    public float placeEvery = 5f;

    [Header("Generation params")]
    public bool generateAtRuntime = true;
    public bool leftside = true;
    public bool rightside = true;
    public bool mirrorRotateObject = true; // apply a mirror rotation to the right side

    public string savePath = "Assets\\object_mesh.asset";

    private List<CombineInstance> combineInstances;

    public void Generate()
    {
        if (pathManager != null && pathManager.carPath != null)
        {

            if (xOffset.Length != meshesToRepeat.Length)
            {
                xOffset = new float[meshesToRepeat.Length];
            }

            if (yOffset.Length != meshesToRepeat.Length)
            {
                yOffset = new float[meshesToRepeat.Length];
            }

            if (rotateYOffset.Length != meshesToRepeat.Length)
            {
                rotateYOffset = new float[meshesToRepeat.Length];
            }
            
            if (rotateXOffset.Length != meshesToRepeat.Length)
            {
                rotateXOffset = new float[meshesToRepeat.Length];
            }


            combineInstances = new List<CombineInstance>();

            Vector3 leftScaling = MeshScale;
            Vector3 rightScaling = MeshScale;
            if (mirrorRotateObject)
            {
                rightScaling.x = -rightScaling.x;
            }

            List<List<Vector3>> leftPositions = new List<List<Vector3>>();
            List<List<Vector3>> rightPositions = new List<List<Vector3>>();

            for (int iM = 0; iM < meshesToRepeat.Length; iM++)
            {
                leftPositions.Add(new List<Vector3>());
                rightPositions.Add(new List<Vector3>());

                PathNode node = pathManager.carPath.centerNodes[0];
                PathNode prevNode = pathManager.carPath.centerNodes[pathManager.carPath.centerNodes.Count - 1];

                Vector3 leftPrevPosition = prevNode.pos + node.rotation * (Vector3.left * leftWidthOffset) + node.rotation * (Vector3.forward * xOffset[iM]) + yOffset[iM] * Vector3.up;
                Vector3 rightPrevPosition = prevNode.pos + node.rotation * (Vector3.right * rightWidthOffset) + node.rotation * (Vector3.forward * xOffset[iM]) + yOffset[iM] * Vector3.up;

                for (int i = 0; i < pathManager.carPath.centerNodes.Count + 2; i++)
                {

                    node = pathManager.carPath.centerNodes[i % pathManager.carPath.centerNodes.Count];

                    if (leftside)
                    {
                        Vector3 tmp_point = node.pos + node.rotation * (Vector3.left * leftWidthOffset) + node.rotation * (Vector3.forward * xOffset[iM]) + yOffset[iM] * Vector3.up;
                        float prev_distance = Vector3.Distance(leftPrevPosition, tmp_point);

                        if (prev_distance >= placeEvery)
                        {
                            float t_factor = placeEvery / prev_distance;

                            Vector3 position = Vector3.Lerp(leftPrevPosition, tmp_point, t_factor);
                            leftPositions[iM].Add(position);

                            leftPrevPosition = position;
                        }
                    }

                    if (rightside)
                    {
                        Vector3 tmp_point = node.pos + node.rotation * (Vector3.right * rightWidthOffset) + node.rotation * (Vector3.forward * xOffset[iM]) + yOffset[iM] * Vector3.up;
                        float prev_distance = Vector3.Distance(rightPrevPosition, tmp_point);

                        if (prev_distance >= placeEvery)
                        {
                            float t_factor = placeEvery / prev_distance;

                            Vector3 position = Vector3.Lerp(rightPrevPosition, tmp_point, t_factor);
                            rightPositions[iM].Add(position);

                            rightPrevPosition = position;
                        }
                    }
                    prevNode = node;
                }

            }

            // go through every points and try to snap the mesh on them

            for (int iM = 0; iM < meshesToRepeat.Length; iM++)
            {
                for (int i = 0; i < leftPositions[iM].Count; i++) // left side
                {

                    Vector3 position = leftPositions[iM][i];
                    Vector3 nextPosition = leftPositions[iM][(i + 1) % leftPositions[iM].Count];

                    Quaternion rot = Quaternion.LookRotation(nextPosition - position, Vector3.up) * Quaternion.AngleAxis(rotateYOffset[iM], Vector3.up) * Quaternion.AngleAxis(rotateXOffset[iM], Vector3.left);
                    Matrix4x4 transform = Matrix4x4.TRS((nextPosition + position) / 2, rot, leftScaling);

                    Mesh mesh = Instantiate(meshesToRepeat[iM]);
                    CombineInstance comb = new CombineInstance();
                    comb.mesh = mesh;
                    comb.transform = transform;
                    comb.subMeshIndex = iM;
                    combineInstances.Add(comb);
                }
            }

            for (int iM = 0; iM < meshesToRepeat.Length; iM++)
            {
                for (int i = 0; i < rightPositions[iM].Count; i++) // right side
                {

                    Vector3 position = rightPositions[iM][i];
                    Vector3 nextPosition = rightPositions[iM][(i + 1) % rightPositions[iM].Count];

                    Quaternion rot = Quaternion.LookRotation(nextPosition - position, Vector3.up) * Quaternion.AngleAxis(-rotateYOffset[iM], Vector3.up) * Quaternion.AngleAxis(rotateXOffset[iM], Vector3.left);
                    Matrix4x4 transform = Matrix4x4.TRS((nextPosition + position) / 2, rot, rightScaling);

                    Mesh mesh = Instantiate(meshesToRepeat[iM]);
                    CombineInstance comb = new CombineInstance();
                    comb.mesh = mesh;
                    comb.transform = transform;
                    comb.subMeshIndex = iM;
                    combineInstances.Add(comb);
                }
            }

            // combine meshes and sub meshes into a unique mesh
            MeshFilter mf = GetComponent<MeshFilter>();
            MeshRenderer mr = GetComponent<MeshRenderer>();
            MeshCollider mc = GetComponent<MeshCollider>();
            Mesh finalMesh = mf.sharedMesh;

            // sort submeshes by index, seperate them
            List<List<CombineInstance>> intermediateComb = new List<List<CombineInstance>>();
            for (int iM = 0; iM < meshesToRepeat.Length; iM++)
            {
                intermediateComb.Add(new List<CombineInstance>());
            }
            for (int i = 0; i < combineInstances.Count; i++)
            {
                CombineInstance comb = combineInstances[i];
                int idx = comb.subMeshIndex;
                comb.subMeshIndex = 0;
                intermediateComb[idx].Add(comb);
            }

            // once the submeshes are combined, combine the bigger meshes
            List<CombineInstance> finalMeshCombs = new List<CombineInstance>();
            foreach (List<CombineInstance> submesh_combs in intermediateComb)
            {
                Mesh submesh = new Mesh();
                submesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                submesh.CombineMeshes(submesh_combs.ToArray(), true, true);

                CombineInstance comb = new CombineInstance();
                comb.mesh = submesh;
                finalMeshCombs.Add(comb);
            }

            if (finalMesh == null)
            {
                finalMesh = new Mesh();
            }

            finalMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            finalMesh.subMeshCount = meshesToRepeat.Length;
            finalMesh.CombineMeshes(finalMeshCombs.ToArray(), false, false);
            finalMesh.subMeshCount = meshesToRepeat.Length;
            finalMesh.Optimize();
            finalMesh.RecalculateBounds();
            finalMesh.RecalculateNormals();
            finalMesh.RecalculateTangents();

            mf.sharedMesh = finalMesh;
            mc.sharedMesh = finalMesh;
        }
    }
    public void Init()
    {
        if (generateAtRuntime)
        {
            Generate();
        }
    }
}