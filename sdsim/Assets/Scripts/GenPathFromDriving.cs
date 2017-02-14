using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GenPathFromDriving : MonoBehaviour {

	public GameObject carObj;
	public ICar car;
	private StreamWriter writer;
	public string outputFilename;
	public float sampleDist = 2.0f;
	Vector3 lastSample = Vector3.zero;

	void Awake()
	{
		car = carObj.GetComponent<ICar>();

		if(car != null)
		{
			string filename = Application.dataPath + outputFilename;

			writer = new StreamWriter(filename);

			Debug.Log("Opening file for path at: " + filename);
		}

	}
	
	// Update is called once per frame
	void Update () 
	{
		if(writer != null)
		{
			Vector3 p = carObj.transform.position;

			if((p - lastSample).magnitude > sampleDist)
			{
				lastSample = p;
				writer.WriteLine(string.Format("{0},{1},{2}", p.x, p.y, p.z));
			}
		}
	}

	public void Shutdown()
	{
		if(writer != null)
		{
			writer.Close();
			writer = null;
		}
	}

	void OnDestroy()
	{
		Shutdown();
	}

}
