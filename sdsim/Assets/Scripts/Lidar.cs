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
    public float rx;
    public float ry;

	public LidarPoint(Vector3 p, float _rx, float _ry)
	{
		x = p.x;
		y = p.y;
		z = p.z;
        rx = _rx;
        ry = _ry;
	}
}

[Serializable]
public class V3
{
    public float x;
    public float y;
    public float z;

    public V3(Vector3 p)
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

	public V3 lidarPos;

	public V3 lidarOrientation;

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
	public float degAngDelta = -1f;

	//how many complete 360 sweeps
	public int numSweepsLevels = 25;

	//what it max distance we will register a hit
	public float maxRange = 50f;

	//how large radius to use when rendering debug display
	public float gizmoSize = 0.1f;

	//what is the scalar on the perlin noise applied to point position
	public float noise = 0.2f;

	public bool DisplayDebugInScene = false;

    // are there layers we don't want to collide with?
    public string[] layerMaskNames;

    int collMask = 0;

	void Awake()
	{
		pointArr = new LidarPointArray();
		pointArr.Init(360 / degPerSweepInc * numSweepsLevels);

        int v = 0;

        foreach (string layerName in layerMaskNames)
        {
            int layer = LayerMask.NameToLayer(layerName);
            v |= 1 << layer;
        }

        collMask |= ~v;

    }

    public void SetConfig(float offset_x, float offset_y, float offset_z, float rot_x,
        int _degPerSweepInc, float _degAngDown, float _degAngDelta, float _maxRange, float _noise, int _numSweepsLevels)
    {
        degPerSweepInc = _degPerSweepInc;
        degAngDown = _degAngDown;
        degAngDelta = _degAngDelta;
        maxRange = _maxRange;
        noise = _noise;
        numSweepsLevels = _numSweepsLevels;

        if (offset_x != 0.0f || offset_y != 0.0f || offset_z != 0.0f)
            transform.localPosition = new Vector3(offset_x, offset_y, offset_z);

        if (rot_x != 0.0f)
            transform.localEulerAngles = new Vector3(rot_x, 0.0f, 0.0f);

        pointArr = new LidarPointArray();
        pointArr.Init(360 / degPerSweepInc * numSweepsLevels);
    }

    public JSONObject GetOutputAsJson()
    {
        LidarPointArray points = GetOutput();
        JSONObject json = JSONObject.Create();
        foreach(LidarPoint p in points.points)
        {
            JSONObject vec = JSONObject.Create();
            try
            {
                vec.AddField("x", p.x);
                vec.AddField("y", p.y);
                vec.AddField("z", p.z);
                vec.AddField("rx", p.rx);
                vec.AddField("ry", p.ry);
                json.Add(vec);
            }
            catch
            {
                // just ignore points that don't resolve.
            }
        }
        
        return json;
    }
    
	public LidarPointArray GetOutput()
	{
		pointArr = new LidarPointArray();
		pointArr.Init(360 / degPerSweepInc * numSweepsLevels);

		pointArr.lidarPos = new V3(transform.position);
		pointArr.lidarOrientation = new V3(transform.rotation.eulerAngles);
		
		Ray ray = new Ray();

		ray.origin = this.transform.position;
		ray.direction = this.transform.forward;

		//start out pointing a bit down.
		Quaternion rotDown = Quaternion.AngleAxis(degAngDown, transform.right);
		ray.direction = rotDown * ray.direction;

		int numSweep = 360 / degPerSweepInc;

		Quaternion rotSide = Quaternion.AngleAxis(degPerSweepInc, transform.up);
		Quaternion rotUp = Quaternion.AngleAxis(degAngDelta, transform.right);

		RaycastHit hit;

		//Sample the output texture to create rays.
		int iP = 0;
        float rx = 0.0f;
        float ry = 0.0f;

		for(int iS = 0; iS < numSweepsLevels; iS++)
		{
			for(int iA = 0; iA < numSweep; iA++)
			{               
				if(Physics.Raycast(ray, out hit, maxRange, collMask))
				{
					//sample that ray at the depth given by the pixel.
					Vector3 pos = ray.GetPoint(hit.distance);

                    //make points relative to my pos.
                    pos = pos - transform.position;

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
					pointArr.points[iP] = new LidarPoint(pos, rx, ry);

					ray.direction = rotSide * ray.direction;

					iP++;
				}

                rx += degPerSweepInc;
            }

			ray.direction = rotUp * ray.direction;
            ry += degAngDelta;
            rx = 0.0f;
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

            //make points global space for drawing
            pos = transform.position + pos;

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
