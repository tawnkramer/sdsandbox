using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;

public class Logger : MonoBehaviour {

	public GameObject carObj;
	public ICar car;
	public CameraSensor camSensor;
    public CameraSensor optionlB_CamSensor;
	public Lidar lidar;

	//what's the current frame index
    public int frameCounter = 0;

	//is there an upper bound on the number of frames to log
	public int maxFramesToLog = 14000;

	//should we log when we are enabled
	public bool bDoLog = true;

	//We can output our logs in the style that matched the output from the udacity simulator
	public bool UdacityStyle = false;

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
			if(UdacityStyle)
			{
				outputFilename = "/../log/driving_log.csv";
			}

			string filename = Application.dataPath + outputFilename;

			writer = new StreamWriter(filename);

			Debug.Log("Opening file for log at: " + filename);

			if(UdacityStyle)
			{
				writer.WriteLine("center,left,right,steering,throttle,brake,speed");
			}
		}

		imagesToSave = new List<ImageSaveJob>();

		thread = new Thread(SaverThread);
		thread.Start();
	}
		
	// Update is called once per frame
	void Update () 
	{
		if(!bDoLog)
			return;
		
		string activity = car.GetActivity();

		if(writer != null)
		{
			if(UdacityStyle)
			{
				string image_filename = GetUdacityStyleImageFilename();
				float steering = car.GetSteering() / 25.0f;
				writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", image_filename, "none", "none", steering.ToString(), car.GetThrottle().ToString(), "0", "0"));
			}
			else
			{
				writer.WriteLine(string.Format("{0},{1},{2},{3}", frameCounter.ToString(), activity, car.GetSteering().ToString(), car.GetThrottle().ToString()));
			}
		}

		if(lidar != null)
		{
			LidarPointArray pa = lidar.GetOutput();

			if(pa != null)
			{
				string json = JsonUtility.ToJson(pa);
				var filename = string.Format("/../log/lidar_{0}_{1}.txt", frameCounter.ToString(), activity);
				var f = File.CreateText(Application.dataPath + filename);
				f.Write(json);
				f.Close();
			}
		}

        if (optionlB_CamSensor != null)
        {
            SaveCamSensor(camSensor, activity, "_a");
            SaveCamSensor(optionlB_CamSensor, activity, "_b");
        }
        else
        {
            SaveCamSensor(camSensor, activity, "");
        }

        if (maxFramesToLog != -1 && frameCounter >= maxFramesToLog)
        {
            Shutdown();
            this.gameObject.SetActive(false);
        }

        frameCounter = frameCounter + 1;
	}

	string GetUdacityStyleImageFilename()
	{
		return Application.dataPath + string.Format("/../log/IMG/center_{0,8:D8}.jpg", frameCounter);
	}

    //Save the camera sensor to an image. Use the suffix to distinguish between cameras.
    void SaveCamSensor(CameraSensor cs, string prefix, string suffix)
    {
        if (cs != null)
        {
            Texture2D image = cs.GetImage();

            ImageSaveJob ij = new ImageSaveJob();

			if(UdacityStyle)
			{
				ij.filename = GetUdacityStyleImageFilename();

				ij.bytes = image.EncodeToJPG();
			}
			else
			{
            	ij.filename = Application.dataPath + string.Format("/../log/{0}_{1,8:D8}{2}.png", prefix, frameCounter, suffix);

            	ij.bytes = image.EncodeToPNG();
			}

            lock (this)
            {
                imagesToSave.Add(ij);
            }
        }
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

		bDoLog = false;
	}

	void OnDestroy()
	{
		Shutdown();
	}
}
