using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathNode
{
	public Vector3 pos;
	public CarModel cm;
	public string activity_prefix; //when this node is active, prefix images w this..
}

public class CarPath 
{
	public List<PathNode> nodes;
	public int iActiveSpan = 0;


	public CarPath()
	{
		nodes = new List<PathNode>();

		ResetActiveSpan();
	}

	public void ResetActiveSpan()
	{
		iActiveSpan = 0;
	}

	public PathNode GetActiveNode()
	{
		if (iActiveSpan < nodes.Count)
			return nodes[iActiveSpan];

		return null;
	}

	public bool GetCrossTrackErr(Vector3 pos, ref float err)
	{
		if(iActiveSpan >= nodes.Count - 2)
			return false;

		PathNode a = nodes[iActiveSpan];
		PathNode b = nodes[iActiveSpan + 1];

		//2d path.
		pos.y = a.pos.y;

		LineSeg3d pathSeg = new LineSeg3d(ref a.pos, ref b.pos);

		pathSeg.Draw( Color.green);		

		LineSeg3d.SegResult segRes = new LineSeg3d.SegResult();

		Vector3 closePt = pathSeg.ClosestPointOnSegmentTo(ref pos, ref segRes);  

		Debug.DrawLine(a.pos, closePt, Color.blue);

		if(segRes == LineSeg3d.SegResult.GreaterThanEnd)
		{
			iActiveSpan++;
		}
		else if(segRes == LineSeg3d.SegResult.LessThanOrigin)
		{
			if(iActiveSpan > 0)
				iActiveSpan--;
		}

		Vector3 errVec = pathSeg.ClosestVectorTo( ref pos );

		Debug.DrawRay(closePt, errVec, Color.white);

		float sign = 1.0f;

		Vector3 cp = Vector3.Cross(pathSeg.m_dir.normalized, errVec.normalized);

		if(cp.y > 0.0f)
			sign = -1f;

		err = errVec.magnitude * sign;
		return true;
	}

}