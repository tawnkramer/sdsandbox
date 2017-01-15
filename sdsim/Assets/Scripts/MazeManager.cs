using UnityEngine;
using System.Collections;

public class MazeManager : MonoBehaviour 
{
	public Transform minLimit;
	public Transform maxLimit;
	public Transform goalObj;
	public GameObject obstaclePrefab;
	public int maxObjects = 100;
	public Transform carStart;

	void Start()
	{
		MakeMaze();
	}

	void MakeMaze()
	{
		Random.InitState(1);

		Vector3 pos;

		for(int i = 0; i < maxObjects; i++)
		{
			RandomPosInMaze(out pos);

			if((pos - carStart.position).magnitude < 10)
				continue;

			Quaternion rot = Quaternion.Euler(0f, Random.Range(0, 4) * 90f, 0.0f);

			Instantiate(obstaclePrefab, pos, rot);
		}

		//find a clear area in the maze to put the goal.
		while( true)
		{
			RandomPosInMaze(out pos);

			Vector3 origin = pos;
			origin.y += 10;
			RaycastHit hitInfo;

			if(Physics.Raycast(origin, Vector3.down, out hitInfo))
			{
				if(hitInfo.collider.gameObject.tag == "ground")
				{
					BarrierPiece[] bps = GameObject.FindObjectsOfType<BarrierPiece>();

					bool clear = true;
					float clearRad = 5f;

					foreach(BarrierPiece b in bps)
					{
						if((b.transform.position - pos).magnitude < clearRad)
						{
							clear = false;
							break;
						}
					}

					if(clear)
					{
						goalObj.transform.position = pos;
						break;
					}
				}
			}
		}
	}

	void RandomPosInMaze(out Vector3 pos)
	{
		Vector3 minPos = minLimit.position;
		Vector3 maxPos = maxLimit.position;
		Vector3 dM = maxPos - minPos;

		pos = minPos;
		pos.x += Random.Range(0.0f, dM.x);
		pos.y += Random.Range(0.0f, dM.y);
		pos.z += Random.Range(0.0f, dM.z);
	}


}
