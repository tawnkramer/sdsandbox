using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System;
using UnityEngine.UI;

[Serializable]
public class MetaJson
{
    public string[] inputs;
    public string[] types;

    public void Init(string[] _inputs, string[] _types)
    {
        inputs = _inputs;
        types = _types;
    }
}

[Serializable]
public class DonkeyRecord
{
    public string cam_image_array;
    public float user_throttle;
    public float user_angle;
    public string user_mode;    

    public void Init(string image_name, float throttle, float angle, string mode)
    {
        cam_image_array = image_name;
        user_throttle = throttle;
        user_angle = angle;
        user_mode = mode;
    }

    public string AsString()
    {
        string json = JsonUtility.ToJson(this);

        //Can't name the variable names with a slash, so replace on output
        json = json.Replace("cam_image", "cam/image");
        json = json.Replace("user_throttle", "user/throttle");
        json = json.Replace("user_angle", "user/angle");
        json = json.Replace("user_mode", "user/mode");

        return json;
    }
}
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

    public int limitFPS = 30;

    float timeSinceLastCapture = 0.0f;

    //We can output our logs in the style that matched the output from the shark robot car platform - github/tawnkramer/shark
    public bool SharkStyle = true;

	//We can output our logs in the style that matched the output from the udacity simulator
	public bool UdacityStyle = false;

    //We can output our logs in the style that matched the output from the donkey robot car platform - donkeycar.com
    public bool DonkeyStyle = false;

    //Tub style as prefered by Donkey2
    public bool DonkeyStyle2 = false;

    public Text logDisplay;

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

            if(DonkeyStyle2)
            {
                MetaJson mjson = new MetaJson();
                string[] inputs = {"cam/image_array", "user/angle", "user/throttle", "user/mode"};
                string[] types = {"image_array", "float", "float", "str"};
                mjson.Init(inputs, types);
                string json = JsonUtility.ToJson(mjson);
				var f = File.CreateText(Application.dataPath + "/../log/meta.json");
				f.Write(json);
				f.Close();
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

        timeSinceLastCapture += Time.deltaTime;

        if (timeSinceLastCapture < 1.0f / limitFPS)
            return;

        timeSinceLastCapture -= (1.0f / limitFPS);

        string activity = car.GetActivity();

		if(writer != null)
		{
			if(UdacityStyle)
			{
				string image_filename = GetUdacityStyleImageFilename();
				float steering = car.GetSteering() / car.GetMaxSteering();
				writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", image_filename, "none", "none", steering.ToString(), car.GetThrottle().ToString(), "0", "0"));
			}
            else if(DonkeyStyle || SharkStyle)
            {

            }
            else if(DonkeyStyle2)
            {
                DonkeyRecord mjson = new DonkeyRecord();
                float steering = car.GetSteering() / car.GetMaxSteering();
                float throttle = car.GetThrottle();

                //training code like steering clamped between -1, 1
                steering = Mathf.Clamp(steering, -1.0f, 1.0f);

                mjson.Init(string.Format("{0}_cam-image_array_.jpg", frameCounter),
                    throttle, steering, "user");

                string json = mjson.AsString();
                string filename = string.Format("record_{0}.json", frameCounter);
				var f = File.CreateText(Application.dataPath + "/../log/" + filename);
				f.Write(json);
				f.Close();
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

        if (logDisplay != null)
            logDisplay.text = "Log:" + frameCounter;
	}

	string GetUdacityStyleImageFilename()
	{
		return Application.dataPath + string.Format("/../log/IMG/center_{0,8:D8}.jpg", frameCounter);
	}

    string GetDonkeyStyleImageFilename()
    {
        float steering = car.GetSteering() / 25.0f;
        float throttle = car.GetThrottle();
        return Application.dataPath + string.Format("/../log/frame_{0,6:D6}_ttl_{1}_agl_{2}_mil_0.0.jpg", 
            frameCounter, throttle, steering);
    }

	string GetSharkStyleImageFilename()
    {
        int steering = (int)(car.GetSteering() / 25.0f * 32768.0f);
        int throttle = (int)(car.GetThrottle() * 32768.0f);
        return Application.dataPath + string.Format("/../log/frame_{0,6:D6}_st_{1}_th_{2}.jpg", 
            frameCounter, steering, throttle);
    }

    string GetDonkey2StyleImageFilename()
    {
        return Application.dataPath + string.Format("/../log/{0}_cam-image_array_.jpg", frameCounter);
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
            else if (DonkeyStyle)
            {
                ij.filename = GetDonkeyStyleImageFilename();

                ij.bytes = image.EncodeToJPG();
            }
            else if (DonkeyStyle2)
            {
                ij.filename = GetDonkey2StyleImageFilename();

                ij.bytes = image.EncodeToJPG();
            }
			else if(SharkStyle)
            {
                ij.filename = GetSharkStyleImageFilename();

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

                //Debug.Log("saving: " + ij.filename);

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
