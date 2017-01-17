using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;

public class Logger : MonoBehaviour {

	public GameObject carObj;
	public ICar car;
	public CameraSensor camSensor;
	public int frameCounter = 0;

	public int maxFramesToLog = 14000;

	public bool bDoLog = true;
	string outputFilename = "/../log/log_car_controls.txt";
	private StreamWriter writer;

	class ImageSaveJob {
		public string filename;
		public byte[] bytes;
	}
		
	List<ImageSaveJob> imagesToSave;

	Thread thread;

	void Awake()
	{
		car = carObj.GetComponent<ICar>();

		if(bDoLog && car != null)
		{
			string filename = Application.dataPath + outputFilename;

			writer = new StreamWriter(filename);

			Debug.Log("Opening file for log at: " + filename);
		}

		imagesToSave = new List<ImageSaveJob>();

		thread = new Thread(SaverThread);
		thread.Start();
	}
		
	// Update is called once per frame
	void Update () 
	{
		if(writer != null)
		{
			writer.WriteLine(string.Format("{0},{1},{2}", frameCounter.ToString(), car.GetSteering().ToString(), car.GetThrottle().ToString()));
		}

		if(camSensor != null)
		{
			Texture2D image = camSensor.GetImage();

			ImageSaveJob ij = new ImageSaveJob();
			ij.filename = Application.dataPath +  string.Format("/../log/image{0,8:D8}.png", frameCounter);
			ij.bytes = image.EncodeToPNG();

			lock(this)
			{
				imagesToSave.Add(ij);
			}

			if(maxFramesToLog != -1 && frameCounter >= maxFramesToLog)
			{
				Shutdown();
				this.gameObject.SetActive(false);
			}
		}

		frameCounter = frameCounter + 1;
	}

	public void SaverThread()
	{
		while(true)
		{
			int count = 0;

			lock(this)
			{
				count = imagesToSave.Count; 
			}

			if(count > 0)
			{
				ImageSaveJob ij = imagesToSave[0];

				//Debug.Log("saving image " + ij.filename);
		
				File.WriteAllBytes(ij.filename, ij.bytes);

				lock(this)
				{
					imagesToSave.RemoveAt(0);
				}
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

		if(thread != null)
		{
			thread.Abort();
			thread = null;
		}
	}

	void OnDestroy()
	{
		Shutdown();
	}
}
