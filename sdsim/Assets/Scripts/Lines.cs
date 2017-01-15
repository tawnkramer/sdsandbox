using UnityEngine;
using System.Collections;

//3d line
public class Line3d
{
		
	public Line3d() {}
	
	public Line3d(ref Vector3 a, ref Vector3 b)
	{
		ConstructLine(ref a, ref b);
	}
	
	//allow a line to be recomputed
	public void ConstructLine(ref Vector3 a, ref Vector3 b)
	{
		m_origin = a;
		m_dir = a - b;
		m_dir.Normalize();
	}
	
	//produce a vector normal to this line passing through this point.
	public Vector3 ClosestVectorTo(ref Vector3 point)
	{
		Vector3 deltaPoint = m_origin - point;
		float dot = Vector3.Dot(deltaPoint, m_dir);
		return (m_dir * dot) - deltaPoint;
	}
	
	//transform the point by the normal vector that places it on the line
	public Vector3 ClosestPointOnLineTo(ref Vector3 point)
	{
		Vector3 vectorTo = ClosestVectorTo(ref point);
		return point - vectorTo;
	}
	
	public float AbsAngleBetween(ref Line3d l)
	{
		return Mathf.Abs(Vector3.Angle( m_dir, l.m_dir));
	}
	
	public Vector3 m_origin, m_dir;
};

public class LineSeg3d : Line3d
{
		
	public LineSeg3d(){}
	
	public LineSeg3d(ref Vector3 a, ref Vector3 b)
	{
		ConstructLineSeg(ref a, ref b);
	}
	
	public void ConstructLineSeg(ref Vector3 a, ref Vector3 b)
	{
		ConstructLine(ref a, ref b);
		m_end = b;
		m_length = (a - b).magnitude;
	}

	public enum SegResult
	{
		OnSpan,
		LessThanOrigin,
		GreaterThanEnd,
	}
	
	//find the closest point, clamping it to the ends
	public Vector3 ClosestPointOnSegmentTo(ref Vector3 point, ref SegResult res)
	{
		Vector3 deltaPoint = m_origin - point;
		float dot = Vector3.Dot(deltaPoint, m_dir);

		//clamp to the ends of the line segment
		if(dot <= 0.0f)
		{
			res = SegResult.LessThanOrigin;
			return m_origin;
		}

		if(dot >= m_length)
		{
			res = SegResult.GreaterThanEnd;
			return m_end;
		}

		res = SegResult.OnSpan;
		Vector3 vectorTo = (m_dir * dot) - deltaPoint;
		return point - vectorTo;
	}

	public void Draw(Color c)
	{
		Debug.DrawLine(m_origin, m_end, c);
	}
	
	public float m_length;
	public Vector3 m_end;
};