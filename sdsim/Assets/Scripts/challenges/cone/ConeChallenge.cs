using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeChallenge : MonoBehaviour, IChallenge
{   
    
	public int numRandCone = 0;
	public float coneHeightOffset = 0.0f;
	public float coneOffset = 1.0f;
	public int iConePrefab = 0;
	public GameObject[] conePrefabs;

    public void InitChallenge(CarPath path)
    {
		for (int i = 0; i < numRandCone; i++){
			RandomCone(path);
		}
    }

    public void RandomCone(CarPath path){
		if (path.centerNodes != null){

			int random_index = Random.Range(0, path.centerNodes.Count);
			PathNode random_node = path.centerNodes[random_index];

			Vector3 rand_pos_offset = new Vector3(Random.Range(-coneOffset, coneOffset), 0, Random.Range(-coneOffset, coneOffset));
			Vector3 xz_coords = new Vector3(random_node.pos.x, coneHeightOffset, random_node.pos.z); // height variation is not supported yet
			Instantiate(conePrefabs[iConePrefab], xz_coords+rand_pos_offset, conePrefabs[iConePrefab].transform.rotation);
			Debug.Log("Added a new random cone");
		}
	}
}
