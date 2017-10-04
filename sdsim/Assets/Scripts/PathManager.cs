using UnityEngine;
using System.Collections;

public class PathManager : MonoBehaviour {

	public CarPath path;

	public GameObject prefab;

	public Transform startPos;

	Vector3 span = Vector3.zero;

	public float spanDist = 5f;

	public int numSpans = 100;

	public float turnInc = 1f;

	public bool sameRandomPath = true;

	public int randSeed = 2;

	public bool doMakeRandomPath = true;

	public bool doLoadScriptPath = false;

	public bool doLoadPointPath = false;

	public bool doBuildRoad = false;

	public bool doChangeLanes = false;

	public int smoothPathIter = 0;

	public bool doShowPath = false;

    public string pathToLoad = "none";

	public RoadBuilder roadBuilder;
	public RoadBuilder semanticSegRoadBuilder;

	public LaneChangeTrainer laneChTrainer;

	void Awake () 
	{
		if(sameRandomPath)
			Random.InitState(randSeed);

		InitNewRoad();			
	}

	public void InitNewRoad()
	{
		if(doMakeRandomPath)
		{
			MakeRandomPath();
		}
		else if (doLoadScriptPath)
		{
			MakeScriptedPath();
		}
		else if(doLoadPointPath)
		{
			MakePointPath();
		}

		if(smoothPathIter > 0)
			SmoothPath();

		//Should we build a road mesh along the path?
		if(doBuildRoad && roadBuilder != null)
			roadBuilder.InitRoad(path);

		if(doBuildRoad && semanticSegRoadBuilder != null)
			semanticSegRoadBuilder.InitRoad(path);

		if(laneChTrainer != null && doChangeLanes)
		{
			laneChTrainer.ModifyPath(ref path);
		}

		if(doShowPath && path != null)
		{
			for(int iN = 0; iN < path.nodes.Count; iN++)
			{
				Vector3 np = path.nodes[iN].pos;
				GameObject go = Instantiate(prefab, np, Quaternion.identity) as GameObject;
				go.tag = "pathNode";
				go.transform.parent = this.transform;
			}
		}
	}

	public void DestroyRoad()
	{
		GameObject[] prev = GameObject.FindGameObjectsWithTag("pathNode");

		foreach(GameObject g in prev)
			Destroy(g);

		if(roadBuilder != null)
			roadBuilder.DestroyRoad();
	}

    public Vector3 GetPathStart()
    {
        return startPos.position;
    }

    public Vector3 GetPathEnd()
    {
        int iN = path.nodes.Count - 1;

        if(iN < 0)
            return GetPathStart();

        return path.nodes[iN].pos;
    }

	void SmoothPath()
	{
		while(smoothPathIter > 0)
		{
			path.SmoothPath();
			smoothPathIter--;
		}
	}

	void MakePointPath()
	{
		string filename = pathToLoad;

		TextAsset bindata = Resources.Load(filename) as TextAsset;

		if(bindata == null)
			return;

		string[] lines = bindata.text.Split('\n');

		Debug.Log(string.Format("found {0} path points. to load", lines.Length));

		path = new CarPath();

		Vector3 np = Vector3.zero;

		float offsetY = -0.1f;

		foreach(string line in lines)
		{
			string[] tokens = line.Split(',');

			if (tokens.Length != 3)
				continue;
			np.x = float.Parse(tokens[0]);
			np.y = float.Parse(tokens[1]) + offsetY;
			np.z = float.Parse(tokens[2]);
			PathNode p = new PathNode();
			p.pos = np;
			path.nodes.Add(p);
		}
			
	}

	void MakeScriptedPath()
	{
		TrackScript script = new TrackScript();

		if(script.Read(pathToLoad))
		{
			path = new CarPath();
			TrackParams tparams = new TrackParams();
			tparams.numToSet = 0;
			tparams.rotCur = Quaternion.identity;
			tparams.lastPos = startPos.position;

			float dY = 0.0f;
			float turn = 0f;

			Vector3 s = startPos.position;
			s.y = 0.5f;
			span.x = 0f;
			span.y = 0f;
			span.z = spanDist;
			float turnVal = 10.0f;

			foreach(TrackScriptElem se in script.track)
			{
				if(se.state == TrackParams.State.AngleDY)
				{
					turnVal = se.value;
				}
				else if(se.state == TrackParams.State.CurveY)
				{
					turn = 0.0f;
					dY = se.value * turnVal;
				}
				else
				{
					dY = 0.0f;
					turn = 0.0f;
				}

				for(int i = 0; i < se.numToSet; i++)
				{

					Vector3 np = s;
					PathNode p = new PathNode();
					p.pos = np;
					path.nodes.Add(p);

					turn = dY;

					Quaternion rot = Quaternion.Euler(0.0f, turn, 0f);
					span = rot * span.normalized;
					span *= spanDist;
					s = s + span;
				}
					
			}
		}
	}

	void MakeRandomPath()
	{
		path = new CarPath();

		Vector3 s = startPos.position;
		float turn = 0f;
		s.y = 0.5f;

		span.x = 0f;
		span.y = 0f;
		span.z = spanDist;

		for(int iS = 0; iS < numSpans; iS++)
		{
			Vector3 np = s;
			PathNode p = new PathNode();
			p.pos = np;
			path.nodes.Add(p);

			float t = Random.Range(-1.0f * turnInc, turnInc);

			turn += t;

			Quaternion rot = Quaternion.Euler(0.0f, turn, 0f);
			span = rot * span.normalized;

			if(SegmentCrossesPath( np + (span.normalized * 100.0f), 90.0f ))
			{
				//turn in the opposite direction if we think we are going to run over the path
				turn *= -0.5f;
				rot = Quaternion.Euler(0.0f, turn, 0f);
				span = rot * span.normalized;
			}

			span *= spanDist;

			s = s + span;
		}
	}

	public bool SegmentCrossesPath(Vector3 posA, float rad)
	{
		foreach(PathNode pn in path.nodes)
		{
			float d = (posA - pn.pos).magnitude;

			if(d < rad)
				return true;
		}

		return false;
	}

	public void SetPath(CarPath p)
	{
		path = p;

		GameObject[] prev = GameObject.FindGameObjectsWithTag("pathNode");

		Debug.Log(string.Format("Cleaning up {0} old nodes. {1} new ones.", prev.Length, p.nodes.Count));

		DestroyRoad();

		foreach(PathNode pn in path.nodes)
		{
			GameObject go = Instantiate(prefab, pn.pos, Quaternion.identity) as GameObject;
			go.tag = "pathNode";
		}
	}
}
