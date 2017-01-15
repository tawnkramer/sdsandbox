using UnityEngine;
using System.Collections;

public class CarModel 
{
	public Vector3 pos = Vector3.zero;
	public float length = 1f;
	public float orientation = 0f; //radians rot about Y

	public void SetLength(float val)
	{
		length = val;
	}

	public void Set(Vector3 p, float o)
	{
		pos = p;
		orientation = o;
	}

	public CarModel move(float distMove, float angleTurn)
	{
		CarModel newMod = new CarModel();
		newMod.pos = pos;
		newMod.length = length;
		newMod.orientation = orientation;
		float theta = orientation;

		if(angleTurn < 0.001f)
		{
			newMod.pos.x += distMove * Mathf.Cos(theta);
			newMod.pos.z += distMove * Mathf.Sin(theta);
		}
		else
		{
			float Beta = distMove / length * Mathf.Tan( angleTurn );
			float R = distMove / Beta;
			float cx = pos.x - Mathf.Sin(theta) * R;
			float cz = pos.z + Mathf.Cos(theta) * R;
			newMod.pos.x = cx + Mathf.Sin(theta + Beta) * R;
			newMod.pos.z = cz - Mathf.Cos(theta + Beta) * R;
			newMod.orientation = theta + Beta;
		}

		return newMod;
	}
}
