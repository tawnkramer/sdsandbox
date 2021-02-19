using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeChallenge : MonoBehaviour, IWaitCarPath
{

    public PathManager pathManager;
    public int numRandCone = 0;
    public float coneHeightOffset = 0.0f;
    public float coneOffset = 1.0f;
    public int iConePrefab = 0;
    public int nodesAfterStart = 10;
    public GameObject[] conePrefabs;
    private List<GameObject> createdObjects = new List<GameObject>();

    public void Init()
    {
        if (!GlobalState.generateRandomCones) { return; }
        foreach (GameObject createdObject in createdObjects)
        {
            GameObject.Destroy(createdObject);
        }
        createdObjects = new List<GameObject>();
        Generate();
    }

    public void Generate()
    {
        if (GlobalState.useSeed) { Random.InitState(GlobalState.seed); };
        for (int i = 0; i < numRandCone; i++)
        {
            RandomCone(i);
        }
    }

    public void ResetChallenge()
    {
        foreach (GameObject createdObject in createdObjects)
        {
            GameObject.Destroy(createdObject);
        }
        createdObjects = new List<GameObject>();
        Generate();
    }

    public void RandomCone(int index)
    {
        if (pathManager.carPath.centerNodes != null && pathManager.carPath.centerNodes.Count > 0)
        {

            int random_index = Random.Range(nodesAfterStart, pathManager.carPath.centerNodes.Count - nodesAfterStart);
            PathNode random_node = pathManager.carPath.centerNodes[random_index];

            Vector3 rand_pos_offset = new Vector3(Random.Range(-coneOffset, coneOffset), 0, Random.Range(-coneOffset, coneOffset));
            Vector3 xz_coords = new Vector3(random_node.pos.x, coneHeightOffset, random_node.pos.z); // height variation is not supported yet
            GameObject go = Instantiate(conePrefabs[iConePrefab], xz_coords + rand_pos_offset, conePrefabs[iConePrefab].transform.rotation);
            ColCone col = go.GetComponentInChildren<ColCone>();
            if (col != null) { col.index = index; }
            createdObjects.Add(go);
        }
    }
}
