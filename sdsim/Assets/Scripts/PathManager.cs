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

	public bool doBuildRoad = false;

	public bool doChangeLanes = false;

	public bool doShowPath = false;

	public RoadBuilder roadBuilder;

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

		//Should we build a road mesh along the path?
		if(doBuildRoad && roadBuilder != null)
			roadBuilder.InitRoad(path);

		if(laneChTrainer != null && doChangeLanes)
		{
			laneChTrainer.ModifyPath(ref path);
		}

		if(doShowPath)
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

	void MakeScriptedPath()
	{
		TrackScript script = new TrackScript();

		if(script.Read("racePi2"))
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
