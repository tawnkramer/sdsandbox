using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathPlanner : MonoBehaviour {

	public int numStepsAhead = 10;

	public Car car;

	public PathManager pm;

	public PIDController controller;

	public DistanceSensor[] distSensors;
	public CollisionSensor colSensor;

	float turn = 0.0f;

	// Use this for initialization
	void Start () 
	{
		Plan ();

		foreach(DistanceSensor ds in distSensors)
		{
			ds.onDetectCB += OnNewBarrierSensed;
		}
	}

	void OnNewBarrierSensed()
	{
		//throw out the old, make a new one.
		if(ShouldDiscard())
			Plan();
	}

	bool ShouldDiscard()
	{
		if (pm.path == null)
			return true;

		foreach (PathNode pn in pm.path.nodes) 
		{
			if (CollidesWWorld(pn.cm))
				return true;
		}

		return false;
	}

	public void Plan()
	{
		CarPath path = new CarPath();

		CarModel cm = new CarModel();
		cm.length = car.length;

		cm.Set(car.transform.position, car.GetOrient());

		PathNode pn = new PathNode();
		pn.pos = cm.pos;
		pn.cm = cm;
		path.nodes.Add(pn);

		float moveDist = pm.spanDist / 2f;

		CarModel prev = cm;

		CarModel res = cm.move(moveDist, turn);

		int iStep = 0;

		while(iStep++ < numStepsAhead)
		{
			if(CollidesWWorld(res))
			{
				float dt = 0.1f;

				CarModel next = null;

				while(dt < 0.3f)
				{
					CarModel tr = prev.move(moveDist, turn + dt);
					CarModel tl = prev.move(moveDist, turn - dt);

					if(!CollidesWWorld(tr))
					{
						next = tr;
						turn = turn + dt;
						break;
					}
					else if(!CollidesWWorld(tl))
					{
						next = tl;
						turn = turn - dt;
						break;
					}

					dt += 0.1f;
				}

				if(next == null)
				{
					int iLast = path.nodes.Count - 1;

					if(iLast > 3)
					{
						path.nodes.RemoveAt(iLast);
						path.nodes.RemoveAt(iLast - 1);
						path.nodes.RemoveAt(iLast - 2);
						next = path.nodes[iLast - 3].cm;
						turn += 0.6f;
						next = next.move(moveDist, turn);
					}
					else
					{
						turn = 0.6f;
						next = res.move(moveDist, turn);
					}
				}

				res = next;
			}
			else
			{
				float nr = 0.01f;
				turn = turn * 0.3f + Random.Range(-nr, nr);
			}

			if(res == null)
				continue;

			pn = new PathNode();
			pn.pos = res.pos;
			pn.cm = res;
			path.nodes.Add(pn);

			prev = res;
			res = res.move(moveDist, turn);
		}

		pm.SetPath(path);

		controller.StartDriving();
	}

	void Update()
	{
		if(pm.path != null && pm.path.iActiveSpan > 5)
			Plan();
	}

	bool CollidesWWorld(CarModel cm)
	{
		if (cm == null)
			return false;

		float thresh = cm.length;

		if(cm.pos.x > 95f || cm.pos.x < 5f)
			return true;

		if(cm.pos.z > 95f || cm.pos.z < 5f)
			return true;

		BarrierPiece[] barriers = GameObject.FindObjectsOfType<BarrierPiece>();

		foreach(BarrierPiece b in barriers)
		{
			//ignore those we haven't sensed.
			if(!b.gameObject.activeInHierarchy)
				continue;

			if((b.transform.position - cm.pos).magnitude < thresh)
				return true;
		}

		return false;
	}
}
