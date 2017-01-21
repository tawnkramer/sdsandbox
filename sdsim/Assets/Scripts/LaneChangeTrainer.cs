using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneChangeTrainer : MonoBehaviour {

	//how many lanes in total to change to/from
	public int numLanes = 5;

	//how far side ways to the next lane
	public float laneDist = 4.0f;

	//which lane do we begin? Far right is zero.
	public int currentLane = 0;

	//over how long should we take to move to the next lane
	public float transitionDist = 20.0f;

	//How long to stay in this lane before changing
	public float laneKeepDist = 20.0f;

	//How much to smooth path. 0.0 - 1.0f
	public float pathSmoothFactor = 0.5f;

	public void ModifyPath(ref CarPath path)
	{
		float curLaneDist = 0.0f;
		Vector3 start = path.nodes[0].pos;
		int changeLaneDir = 1;
		float curTransDist = 0.0f;
		bool initLaneChange = false;
	
		Vector3 offset = Vector3.zero;
		Vector3 laneChangeDp = Vector3.zero;

		string activity = "keep_lane";

		for(int iN = 1; iN < path.nodes.Count; iN++)
		{
			PathNode n = path.nodes[iN];
			Vector3 dp = n.pos - path.nodes[iN - 1].pos;

			if(curLaneDist < laneKeepDist)
			{
				n.activity = activity;

				activity = "keep_lane";

				//stay in current lane.
				curLaneDist += dp.magnitude;
				initLaneChange = false;
			}
			else if(!initLaneChange)
			{
				initLaneChange = true;
				laneChangeDp = new Vector3(-1 * NewLaneDir(changeLaneDir) * laneDist, 0.0f, laneKeepDist);

				GetActivity(ref changeLaneDir, ref activity);

				offset = laneChangeDp.normalized * dp.magnitude;
				offset.z = 0.0f;
				OffsetRemainingPath(ref path, iN, offset);
				curTransDist += dp.magnitude;

				n.activity = activity;
			}
			else
			{
				offset = laneChangeDp.normalized * dp.magnitude;
				offset.z = 0.0f;
				OffsetRemainingPath(ref path, iN, offset);
				curTransDist += dp.magnitude;

				n.activity = activity;

				if(curTransDist > transitionDist)
				{
					OnEnterNewLane(ref changeLaneDir);

					n = path.nodes[iN];

					//do a small correction to make sure we are in the absolute center of the lane.
					float lanePosAbsoluteX = (-1 * currentLane * laneDist) + start.x;
					float errorX = lanePosAbsoluteX - n.pos.x;
					Vector3 error = new Vector3(errorX, 0.0f, 0.0f);
					OffsetRemainingPath(ref path, iN, error);

					//switch back to driving in the lane.
					curLaneDist = 0.0f;
					curTransDist = 0.0f;
				}
			}
		}

		OffsetLabelsBack(ref path);
		OffsetLabelsBack(ref path);

		path.SmoothPath(pathSmoothFactor);
	}

	void OffsetLabelsBack(ref CarPath path)
	{
		//we are setting our activity labels one node too late. Try setting them a bit early. And removing them one early too.
		for(int iN = 1; iN < path.nodes.Count; iN++)
		{
			PathNode p = path.nodes[iN - 1];
			PathNode c = path.nodes[iN];

			if(p.activity == null || c.activity == null)
				continue;

			if(c.activity != p.activity)
			{
				if(c.activity.StartsWith("cl_"))
				{
					p.activity = c.activity;
				}
				else if(p.activity.StartsWith("cl_"))
				{
					p.activity = c.activity;
				}
			}
		}
	}

	void OffsetRemainingPath(ref CarPath path, int iStart, Vector3 offset)
	{
		for(int iN = iStart; iN < path.nodes.Count; iN++)
		{
			PathNode n = path.nodes[iN];
			n.pos += offset;
		}
	}

	int NewLaneDir(int changeLaneDir)
	{
		if(currentLane == numLanes - 1)
		{
			return -1;
		}
		else if(currentLane == 0)
		{
			return 1;
		}

		return changeLaneDir;
	}

	void GetActivity(ref int changeLaneDir, ref string activity)
	{
		if(changeLaneDir > 0)
			activity = "cl_left";
		else
			activity = "cl_right";
	}

	void OnEnterNewLane(ref int changeLaneDir)
	{
		//choose a dir, left or right.
		currentLane += changeLaneDir;

		if(currentLane == numLanes - 1)
		{
			changeLaneDir = -1;

		}
		else if(currentLane == 0)
		{
			changeLaneDir = 1;
		}
	}
}
