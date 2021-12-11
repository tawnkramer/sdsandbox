using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class PathNode
{
    public Vector3 pos;
    public Quaternion rotation;
    public string activity;
}

public class CarPath
{
    public List<PathNode> nodes;
    public List<PathNode> centerNodes;
    public NavMeshPath navMeshPath;
    public int iActiveSpan = 0;


    public CarPath()
    {
        nodes = new List<PathNode>();
        centerNodes = new List<PathNode>();
        navMeshPath = new NavMeshPath();
    }

    public int GetClosestSpanIndex(Vector3 carPos)
    {
        float minDistance = float.MaxValue;
        int minDistanceIndex = -1;
        for (int i = 0; i < nodes.Count; i++)
        {
            float dist = Vector3.Distance(nodes[i].pos, carPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                minDistanceIndex = i;
            }
        }
        return minDistanceIndex;
    }

    public PathNode GetNode(int index)
    {
        if (index < nodes.Count)
            return nodes[index];

        return null;
    }

    public void SmoothPath(float factor = 0.5f)
    {
        LineSeg3d.SegResult segRes = new LineSeg3d.SegResult();

        for (int iN = 1; iN < nodes.Count - 2; iN++)
        {
            PathNode p = nodes[iN - 1];
            PathNode c = nodes[iN];
            PathNode n = nodes[iN + 1];

            LineSeg3d seg = new LineSeg3d(ref p.pos, ref n.pos);
            Vector3 closestP = seg.ClosestPointOnSegmentTo(ref c.pos, ref segRes);
            Vector3 dIntersect = closestP - c.pos;
            c.pos += dIntersect.normalized * factor;
        }
    }
    public double getDistance(Vector3 currentPosition, Vector3 target)
    {
        double distance = 0.0;
        if (NavMesh.CalculatePath(currentPosition, target, NavMesh.AllAreas, this.navMeshPath))
        {
            if (this.navMeshPath.corners.Length > 5)
            {
                //ignore the first corners -> more stable
                distance += (this.navMeshPath.corners[2] - currentPosition).magnitude;
                for (int i = 3; i < this.navMeshPath.corners.Length; i++)
                {
                    Vector3 start = this.navMeshPath.corners[i - 1];
                    Vector3 end = this.navMeshPath.corners[i];
                    distance += (end - start).magnitude;
                }
            }
        }
        return distance;
    }

    public bool GetCrossTrackErr(Vector3 pos, ref int iActiveSpan, ref float err, int lookAhead = 1)
    {
        int nextIActiveSpan = (iActiveSpan + 1) % (nodes.Count);
        int aheadIActiveSpan = (iActiveSpan + lookAhead) % (nodes.Count);

        PathNode a = nodes[iActiveSpan];
        PathNode b = nodes[nextIActiveSpan];
        PathNode c = nodes[aheadIActiveSpan];

        //2d path.
        pos.y = a.pos.y;

        LineSeg3d pathSeg = new LineSeg3d(ref a.pos, ref c.pos);
        LineSeg3d.SegResult segRes = new LineSeg3d.SegResult();
        Vector3 closePt = pathSeg.ClosestPointOnSegmentTo(ref pos, ref segRes);
        Vector3 errVec = pathSeg.ClosestVectorTo(ref pos);

        pathSeg.Draw(Color.green);
        Debug.DrawLine(a.pos, closePt, Color.blue);
        Debug.DrawRay(closePt, errVec, Color.white);

        float sign = 1.0f;

        Vector3 cp = Vector3.Cross(pathSeg.m_dir.normalized, errVec.normalized);

        if (cp.y > 0.0f)
            sign = -1f;

        err = errVec.magnitude * sign;

        int oldActiveSpan = iActiveSpan ; 

        float dista = Vector3.Distance(a.pos, pos);
        float distb = Vector3.Distance(b.pos, pos);
        if (dista > distb)
        {
            iActiveSpan = (iActiveSpan + 1) % (nodes.Count);
        }

        // if (iActiveSpan - oldActiveSpan <= 0) { return true; } // we lapped
        return false; // we are on the same lap
    }

    public (float xmin, float xmax, float ymin, float ymax, float zmin, float zmax) GetPathBounds()
    {
        (float xmin, float xmax, float ymin, float ymax, float zmin, float zmax) bounds;
        bounds.xmin = bounds.ymin = bounds.zmin = float.MaxValue;
        bounds.xmax = bounds.ymax = bounds.zmax = float.MinValue;

        foreach (PathNode node in centerNodes)
        {

            Vector3 pos = node.pos;
            float x = pos.x;
            float y = pos.y;
            float z = pos.z;

            if (x < bounds.xmin)
                bounds.xmin = x;
            if (x > bounds.xmax)
                bounds.xmax = x;

            if (y < bounds.ymin)
                bounds.ymin = y;
            if (y > bounds.ymax)
                bounds.ymax = y;

            if (z < bounds.zmin)
                bounds.zmin = z;
            if (z > bounds.zmax)
                bounds.zmax = z;

        }
        return bounds;
    }
}