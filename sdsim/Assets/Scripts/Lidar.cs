using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LidarPoint
{
    public float x;
    public float y;
    public float z;

	public LidarPoint(Vector3 p)
	{
		x = p.x;
		y = p.y;
		z = p.z;
	}
}

[Serializable]
public class LidarPointArray
{
	//All coordinates in World coordinate system

	public LidarPoint lidarPos;

	public LidarPoint lidarOrientation;

    public LidarPoint[] points;

    public void Init(int numPoints)
    {
        points = new LidarPoint[numPoints];
    }
}

public class Lidar : MonoBehaviour {


	LidarPointArray pointArr;

	//as the ray sweeps around, how many degrees does it advance per sample
	public int degPerSweepInc = 2;

	//what is the starting angle for the initial sweep compared to the forward vector
	public float degAngDown = 25f;

	//what angle change between sweeps 
	public float defAngDelta = -1f;

	//how many complete 360 sweeps
	public int numSweepsLevels = 25;

	//what it max distance we will register a hit
	public float maxRange = 50f;

	//how large radius to use when rendering debug display
	public float gizmoSize = 0.1f;

	//what is the scalar on the perlin noise applied to point position
	public float noise = 0.2f;

	public bool DisplayDebugInScene = false;

	void Awake()
	{
		pointArr = new LidarPointArray();
		pointArr.Init(360 / degPerSweepInc * numSweepsLevels);
	}

    
	public LidarPointArray GetOutput()
	{
		pointArr = new LidarPointArray();
		pointArr.Init(360 / degPerSweepInc * numSweepsLevels);

		pointArr.lidarPos = new LidarPoint(transform.position);
		pointArr.lidarOrientation = new LidarPoint(transform.rotation.eulerAngles);
		
		Ray ray = new Ray();

		ray.origin = this.transform.position;
		ray.direction = this.transform.forward;

		//start out pointing a bit down.
		Quaternion rotDown = Quaternion.AngleAxis(degAngDown, transform.right);
		ray.direction = rotDown * ray.direction;

		int numSweep = 360 / degPerSweepInc;

		Quaternion rotSide = Quaternion.AngleAxis(degPerSweepInc, transform.up);
		Quaternion rotUp = Quaternion.AngleAxis(defAngDelta, transform.right);

		RaycastHit hit;

		//Sample the output texture to create rays.
		int iP = 0;

		for(int iS = 0; iS < numSweepsLevels; iS++)
		{
			for(int iA = 0; iA < numSweep; iA++)
			{
				if(Physics.Raycast(ray, out hit, maxRange))
				{
					//sample that ray at the depth given by the pixel.
					Vector3 pos = ray.GetPoint(hit.distance);

					//shouldn't hit this unless user is messing around in the interface with things running.
					if(iP >= pointArr.points.Length)
						break;

					float noiseX = Mathf.PerlinNoise(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
					float noiseY = Mathf.PerlinNoise(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
					float noiseZ = Mathf.PerlinNoise(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
					pos.x += noise * noiseX;
					pos.y += noise * noiseY;
					pos.z += noise * noiseZ;

					//set iPoint
					pointArr.points[iP] = new LidarPoint(pos);

					ray.direction = rotSide * ray.direction;

					iP++;
				}
			}

			ray.direction = rotUp * ray.direction; 
		}

		return pointArr;
	}

	void OnDrawGizmosSelected() 
	{		
		Gizmos.color = Color.red;

		LidarPointArray arr = GetOutput();

		if(arr == null)
			return;
		
		Vector3 pos = Vector3.zero;

		foreach(LidarPoint p in arr.points)
		{
			if(p == null)
				continue;
			
			pos.x = p.x;
			pos.y = p.y;
			pos.z = p.z;

			Gizmos.DrawSphere(pos, gizmoSize);
		}
	}

	static Material lineMaterial;
	static void CreateLineMaterial ()
	{
		if (!lineMaterial)
		{
			// Unity has a built-in shader that is useful for drawing
			// simple colored things.
			Shader shader = Shader.Find ("Hidden/Internal-Colored");
			lineMaterial = new Material (shader);
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			// Turn on alpha blending
			lineMaterial.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			lineMaterial.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			// Turn backface culling off
			lineMaterial.SetInt ("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
			// Turn off depth writes
			lineMaterial.SetInt ("_ZWrite", 0);
		}
	}

	// Will be called after all regular rendering is done
	public void OnRenderObject ()
	{

		if(DisplayDebugInScene == false)
			return;
		
		CreateLineMaterial ();
		// Apply the line material
		lineMaterial.SetPass (0);

		GL.PushMatrix ();
		// Set transformation matrix for drawing to
		// match our transform
		//GL.MultMatrix (transform.localToWorldMatrix);
		GL.MultMatrix(Matrix4x4.identity);
		// Draw lines
		GL.Begin (GL.LINES);

		LidarPointArray arr = GetOutput();

		if(arr == null)
			return;
		
		foreach(LidarPoint p in arr.points)
		{
			if(p == null)
				continue;

			//red
			GL.Color (new Color (1, 0, 0, 0.8F));
			GL.Vertex3(p.x, p.y, p.z);
			GL.Vertex3(p.x, p.y + gizmoSize, p.z);
		}

		GL.End ();
		GL.PopMatrix ();
	}
}

