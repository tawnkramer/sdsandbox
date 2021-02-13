using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandAssetsChallenge : MonoBehaviour, IWaitCarPath
{
    public PathManager pathManager;
    public float minRange = 5;
    public float maxRange = 20;
    public float heightOffset = 0;
    public int numAssets = 10;
    public GameObject[] prefabList;
    public GameObject parentGameObject;
    private List<GameObject> createdObjects = new List<GameObject>();

    public void Init()
    {
        if (!GlobalState.generateTrees) { return; }
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
        GameObject[] randomList = new GameObject[numAssets];
        for (int i = 0; i < numAssets; i++) // pick some items from the prefab list and add them to the randomList array
        {
            int index = Random.Range(0, prefabList.Length);
            randomList[i] = prefabList[index];
        }

        PlaceAssets(randomList);
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

    public void PlaceAssets(GameObject[] assetList)
    {
        if (pathManager.carPath.centerNodes != null && pathManager.carPath.centerNodes.Count > 0)
        {
            for (int i = 0; i < assetList.Length; i++)
            {
                GameObject asset = assetList[i];
                int random_index = Random.Range(0, pathManager.carPath.centerNodes.Count);
                PathNode random_node = pathManager.carPath.centerNodes[random_index];

                bool valid_pos = false;
                int max_iter = 10;
                while (!valid_pos && max_iter > 0)
                {

                    Vector3 rand_pos_offset = new Vector3(RandomMinMaxRange(minRange, maxRange), 0, RandomMinMaxRange(minRange, maxRange));
                    Vector3 xyz_coords = new Vector3(random_node.pos.x, random_node.pos.y + heightOffset, random_node.pos.z); // height variation is not supported yet
                    Vector3 new_point = rand_pos_offset + xyz_coords + asset.transform.position;

                    asset.transform.RotateAround(Vector3.zero, Vector3.up, Random.Range(0, 180));

                    if (IsValid(pathManager.carPath, new_point))
                    {
                        GameObject go = Instantiate(asset, new_point, asset.transform.rotation);
                        if (parentGameObject != null)
                        {
                            go.transform.parent = parentGameObject.transform;
                        }
                        go.isStatic = true; // set the object to static to save some performance
                        createdObjects.Add(go);
                        break;
                    }

                    max_iter--;
                }
            }

        }
    }

    public bool IsValid(CarPath path, Vector3 point)
    {
        int pathLength = path.centerNodes.Count;
        float[] distances = new float[pathLength - 1];

        for (int i = 0; i < pathLength; i++)
        {
            Vector3 pathPoint = path.centerNodes[i].pos;
            float distance = Vector3.Distance(point, pathPoint);

            if (distance < minRange)
            {
                return false;
            }
        }

        return true;
    }

    public float RandomMinMaxRange(float min, float max)
    {
        int sign = -1;
        if (Random.value > 0.5)
        {
            sign = 1;
        }
        return Random.Range(max, min) * sign;
    }

}
