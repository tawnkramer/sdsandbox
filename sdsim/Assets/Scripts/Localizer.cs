using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Measurement
{
	public int id;
	public int offsetX;
	public int offsetZ;
	
	public Measurement(int _id, int offX, int offZ)
	{
		id = _id;
		offsetX = offX;
		offsetZ = offZ;
	}
}


[System.Serializable]
public class Measurements
{
	public List<Measurement> m;

	public void Init(int count)
	{
		m = new List<Measurement>(count);
	}
}

[System.Serializable]
public class ProbMap
{
	public float[,] cells;
	public int numX;
	public int numZ;

	public void Init(int numXCells, int numZCells)
	{
		numX = numXCells;
		numZ = numZCells;
		cells = new float[numXCells, numZCells];
	}

	public void AllEqualProb()
	{
		int totalCells = numX * numZ;
		
		//At first we are equally likely to be in any cell.
		//And since we like the probability to add to 1, we
		///divide by total cells.
		float iniProb = 1.0f / totalCells;
		
		for(int iX = 0; iX < numX; iX++)
		{
			for(int iZ = 0; iZ < numZ; iZ++)
			{
				cells[iX, iZ] = iniProb;
			}
		}
	}

	public void Zero()
	{
		for(int iX = 0; iX < numX; iX++)
		{
			for(int iZ = 0; iZ < numZ; iZ++)
			{
				cells[iX, iZ] = 0.0f;
			}
		}
	}
}

public class ProbMapVisualizer
{
	public List<GameObject> cellMarkers;
	public List<Vector2> topCells;

	public void Init(int numToVisualize, GameObject prefab)
	{
		cellMarkers = new List<GameObject>(numToVisualize);
		topCells = new List<Vector2>(numToVisualize);

		for(int i = 0; i < numToVisualize; i++)
		{
			GameObject go = GameObject.Instantiate(prefab) as GameObject;
			cellMarkers.Add(go);
		}
	}

	public void Visualize(ProbMap pm, Map world)
	{
		topCells.Clear();
		int numX = pm.numX;
		int numZ = pm.numZ;

		float thresh = 0.05f;
		int iCM = 0;

		for(int iX = 0; iX < numX; iX++)
		{
			for(int iZ = 0; iZ < numZ; iZ++)
			{
				float p = pm.cells[iX, iZ];

				if(p > thresh)
				{
					Vector3 pos = Vector3.zero;
					pos = world.startPos;
					pos.x += world.dX * iX;
					pos.z += world.dZ * iZ;

					cellMarkers[iCM].transform.position = pos;

					Vector3 s = Vector3.one;
					s.y = p * 100f;
					s.x = world.dX;
					s.z = world.dZ;
					cellMarkers[iCM].transform.localScale = s;

					iCM++;

					if(iCM == cellMarkers.Count)
						break;
				}
			}
		}

		for(int iM = iCM; iM < cellMarkers.Count; iM++)
		{
			cellMarkers[iM].transform.position = Vector3.zero;
			cellMarkers[iM].transform.localScale = Vector3.zero;
		}
	}
}


public class MonteCarloLocalizer
{
	public Map world;
	public ProbMap probMap;

	//for memory efficiency, we will allocate
	//two maps and swap between them.
	bool useMapA;
	ProbMap mapA;
	ProbMap mapB;

	public void Init(Map worldMap)
	{
		world = worldMap;
		mapA = new ProbMap();
		mapA.Init(world.numX, world.numZ);
		mapA.AllEqualProb();
		useMapA = true;
		probMap = mapA;

		mapB = new ProbMap();
		mapB.Init(world.numX, world.numZ);
	}

	public void Move(int iMoveX, int iMoveZ, float probExact)
	{
		if(iMoveX == 0 && iMoveZ == 0)
			return;

		ProbMap newMap = useMapA ? mapB : mapA;
		newMap.Zero();

		int numX = probMap.numX;
		int numZ = probMap.numZ;

		for(int iX = 0; iX < numX; iX++)
		{
			for(int iZ = 0; iZ < numZ; iZ++)
			{
				int iTargetX = iX + iMoveX;
				int iTargetZ = iZ + iMoveZ;

				if(iTargetX < 0 || iTargetX >= numX)
					continue;

				if(iTargetZ < 0 || iTargetZ >= numZ)
					continue;

				//int trailX = iMoveX > 0 ? - 1 : (iMoveX == 0 ? 0 : 1);
				//int trailZ = iMoveZ > 0 ? - 1 : (iMoveZ == 0 ? 0 : 1);

				//most of old value moves to new
				float probTarget = probMap.cells[iX, iZ] * probExact;

				//some probablity that we are still in old cell.
				//float probTrail = probMap.cells[iX, iZ] * (1.0f - probExact);

				newMap.cells[iTargetX, iTargetZ] += probTarget;
				//newMap.cells[iTargetX + trailX, iTargetZ + trailZ] += probTrail;
			}
		}

		probMap = newMap;
		useMapA = !useMapA;
	}

	public void Sense(Measurements m, float probExact)
	{
		for(int iM = 0; iM < m.m.Count; iM++)
		{
			Measurement _m = m.m[iM];

			Sense(_m, probExact);
		}
	}	

	public void Sense(Measurement m, float probHit)
	{
		int numX = probMap.numX;
		int numZ = probMap.numZ;
		float probMiss = 1.0f - probHit;
		float t = 0f;

		for(int iX = 0; iX < numX; iX++)
		{
			for(int iZ = 0; iZ < numZ; iZ++)
			{
				float hit = 0f;
				int iSenseX = iX + m.offsetX;
				int iSenseZ = iZ + m.offsetZ;

				if(iSenseZ >= 0 && iSenseZ < numZ &&
				   iSenseX >= 0 && iSenseX < numX)
				{
					hit = (m.id == world.cells[iSenseX, iSenseZ]) ? 1.0f : 0.0f;
				}

				float p = probMap.cells[iX, iZ];

				float np = p * (hit * probHit + (1.0f - hit) * probMiss);

				probMap.cells[iX, iZ] = np;

				t += np;
			}
		}

		//Normalize.
		for(int iX = 0; iX < numX; iX++)
		{
			for(int iZ = 0; iZ < numZ; iZ++)
			{
				probMap.cells[iX, iZ] = probMap.cells[iX, iZ] / t;
			}
		}
	}

	public void MostLikelyCell(out int cellX, out int cellZ)
	{
		float p = 0f;
		int numX = probMap.numX;
		int numZ = probMap.numZ;
		cellX = 0;
		cellZ = 0;

		for(int iX = 0; iX < numX; iX++)
		{
			for(int iZ = 0; iZ < numZ; iZ++)
			{
				float pc = probMap.cells[iX, iZ];

				if(pc > p)
				{
					p = pc;
					cellX = iX;
					cellZ = iZ;
				}
			}
		}
	}

	public void MostLikelyPos(out Vector3 pos)
	{
		int iX = 0;
		int iZ = 0;
		MostLikelyCell(out iX, out iZ);

		pos = world.startPos;
		pos.x += world.dX * iX;
		pos.z += world.dZ * iZ;
	}
}

public class LocUnitTester
{
	public void Test(Map map, Measurements measurements, List<Vector2> moves,
	                 int iExpectedX, int iExpectedZ)
	{
		MonteCarloLocalizer loc = new MonteCarloLocalizer();

		loc.Init(map);
		int iX = (int)moves[0].x;
		int iY = (int)moves[0].y;
		loc.Move(iX, iY, 1f);

		loc.Sense(measurements, 1f);

		int mlX, mlZ;

		loc.MostLikelyCell(out mlX, out mlZ);

		if(iExpectedX == mlX && iExpectedZ == mlZ)
		{
			Debug.Log("Correct!");
		}
		else
		{
			Debug.LogError("Oops");
		}
	}

	public void TestA()
	{
		Map map = new Map();
		map.Init(3, 3);
		map.cells [1,1] = 1; //all the rest are zero.
		Measurement m = new Measurement(1, 0, 0);
		Measurements ma = new Measurements();
		ma.Init(1);
		ma.m.Add(m);
		List<Vector2> moves = new List<Vector2>();
		moves.Add(Vector2.zero);

		Test (map, ma, moves, 1, 1);
	}

	public void TestB()
	{
		Map map = new Map();
		map.Init(3, 3);
		map.cells [1,1] = 1; //all the rest are zero.
		Measurement m = new Measurement(1, 0, -1);
		Measurements ma = new Measurements();
		ma.Init(1);
		ma.m.Add(m);
		List<Vector2> moves = new List<Vector2>();
		moves.Add(Vector2.zero);
		
		Test (map, ma, moves, 1, 2);
	}
}


public class Localizer : MonoBehaviour 
{
	MonteCarloLocalizer loc;
	ProbMapVisualizer vis;

	public MapManager mm;
	int iPrevX = 0;
	int iPrevZ = 0;
	Vector3 prevPos;

	public float radiusSense = 100f;
	public float threshMove = 0.1f;
	public float probMoveExact = 0.8f;
	public float probSenseExact = 0.8f;

	public Transform likelyTM;

	// Use this for initialization
	void Start () 
	{
		LocUnitTester tester = new LocUnitTester();
		tester.TestB();

		loc = new MonteCarloLocalizer();
		loc.Init(mm.map);

		vis = new ProbMapVisualizer();
		vis.Init(25, likelyTM.gameObject );

		//when we move at least full a cell dist, then sense again.
		threshMove = mm.map.dX;
		prevPos = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 newPos = transform.position;
		int inewX = Mathf.RoundToInt(newPos.x / mm.map.dX);
		int inewZ = Mathf.RoundToInt(newPos.z / mm.map.dZ);
		if(inewX != iPrevX || inewZ != iPrevZ)
		{
			int iMoveX = inewX - iPrevX;
			int iMoveZ = inewZ - iPrevZ;
			SenseEnv(iMoveX, iMoveZ);

			iPrevX = inewX;
			iPrevZ = inewZ;
			prevPos = newPos;
		}

		vis.Visualize(loc.probMap, loc.world);
	}

	void SenseEnv(int iMoveX, int iMoveZ)
	{
		Marker[] allMarkers = GameObject.FindObjectsOfType<Marker>();
		Vector3 pos = transform.position;
		Measurements ma = new Measurements();
		ma.Init(100);

		foreach(Marker marker in allMarkers)
		{
			Vector3 delta = (marker.transform.position - pos);

			if(delta.magnitude < radiusSense)
			{
				int offX = Mathf.RoundToInt(delta.x / mm.map.dX);
				int offZ = Mathf.RoundToInt(delta.z / mm.map.dZ);

				Measurement m = new Measurement(marker.id, offX, offZ);

				ma.m.Add(m);
			}
		}


		//first move is huge and not needed.
		if(prevPos != Vector3.zero)
			loc.Move(iMoveX, iMoveZ, probMoveExact);

		loc.Sense(ma, probSenseExact);

		Vector3 likelyPos = Vector3.zero;
		likelyTM.position = likelyPos;
	}
}
