using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandAssetsChallenge : MonoBehaviour, IChallenge
{
    public float minRange = 5;
    public float maxRange = 20;
    public float heightOffset = 0;
    public int numAssets = 10;
    public GameObject[] prefabList;

    public void InitChallenge(CarPath path)
    {
        GameObject[] randomList = new GameObject[numAssets];
        for (int i = 0; i < numAssets; i++) // pick some items from the prefab list and add them to the randomList array
        {
            int index = Random.Range(0, prefabList.Length);
            randomList[i] = prefabList[index];
        }

        PlaceAssets(path, randomList);
    }

    public void PlaceAssets(CarPath path, GameObject[] assetList)
    {
        if (path.centerNodes != null)
        {
            foreach (GameObject asset in assetList)
            {
                int random_index = Random.Range(0, path.centerNodes.Count);
                PathNode random_node = path.centerNodes[random_index];

                bool valid_pos = false;
                int max_iter = 10;
                while (!valid_pos && max_iter > 0)
                {

                    Vector3 rand_pos_offset = new Vector3(RandomMinMaxRange(minRange, maxRange), 0, RandomMinMaxRange(minRange, maxRange));
                    Vector3 xz_coords = new Vector3(random_node.pos.x, heightOffset, random_node.pos.z); // height variation is not supported yet
                    Vector3 new_point = rand_pos_offset + xz_coords + asset.transform.position;

                    asset.transform.RotateAround(Vector3.zero, Vector3.up, Random.Range(0, 180));

                    if (IsValid(path, new_point))
                    {
                        Instantiate(asset, new_point, asset.transform.rotation);
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
