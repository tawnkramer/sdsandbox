using UnityEngine;
using System.Collections;

[System.Serializable]
public class Map
{
	public int[,] cells;
	public Vector3 startPos; //of 0,0 cell.
	public float dX; //the delta X position to the next cell
	public float dZ; //the delta Z position to the next cell
	public int numX;
	public int numZ;

	public void Init(int numXCells, int numZCells)
	{
		numX = numXCells;
		numZ = numZCells;

		cells = new int[numXCells, numZCells];
	}
}

public class MapManager : MonoBehaviour 
{

	//two node giving the extent of the map.
	public Transform minLimit;
	public Transform maxLimit;
	public float cellSize = 1f; //how big is the cell in world units 
	public int numXCells = 1;
	public int numZCells = 1;
	public GameObject markerPrefab;
	public int numMarkers = 1;
	public int numMarkerIds = 10;

	public Map map;

	// Use this for initialization
	void Awake () {
		GenerateMap();
	}
	
	// Update is called once per frame
	void GenerateMap () 
	{
		Vector3 minPos = minLimit.position;
		Vector3 maxPos = maxLimit.position;
		numXCells = (int)((maxPos.x - minPos.x) / cellSize); 
		numZCells = (int)((maxPos.z - minPos.z) / cellSize); 
		float dx = (maxPos.x - minPos.x) / (numXCells);	
		float dz = (maxPos.z - minPos.z) / (numZCells);

		map = new Map();
		map.Init(numXCells, numZCells);
		map.startPos = minPos;
		map.dX = dx;
		map.dZ = dz;

		for(int iM = 0; iM < numMarkers; iM++)
		{
			int iX = Random.Range(0, numXCells);
			int iZ = Random.Range(0, numZCells);
			Vector3 pos = Vector3.zero;
			pos.x = iX * dx + minPos.x;
			pos.z = iZ * dz + minPos.z;

			GameObject go = Instantiate(markerPrefab, pos, Quaternion.identity) as GameObject;

			Marker m = go.GetComponent<Marker>();

			//go.transform.parent = this.transform;

			if(m)
			{
				m.id = Random.Range(1, numMarkerIds);

				//map cell has this id now.
				map.cells[iX, iZ] = m.id;
			}
		}
	}
}
