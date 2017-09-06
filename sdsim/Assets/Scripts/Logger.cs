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

    public int limitFPS = 30;

    float timeSinceLastCapture = 0.0f;

	class ImageSaveJob {
		public string filename;
		public byte[] bytes;
	}
		
	List<ImageSaveJob> imagesToSave;

    bool runThread = false;

    Thread thread;

	void Awake()
	{
		car = carObj.GetComponent<ICar>();

		imagesToSave = new List<ImageSaveJob>();
	}

    private void OnEnable()
    {
        runThread = true;
        thread = new Thread(SaverThread);
        thread.Start();
    }

    void KillThread()
    {
        runThread = false;

        if (thread != null)
        {
            thread.Abort();
            thread = null;
        }
    }

    private void OnDisable()
    {
        KillThread();
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

	string GetImageFileName()
    {
        float steering = car.GetSteering();
        float throttle = car.GetThrottle();
        return Application.dataPath + string.Format("/../log/frame_{0,6:D6}_st_{1}_th_{2}.jpg", 
            frameCounter, steering, throttle);
    }

    //Save the camera sensor to an image. Use the suffix to distinguish between cameras.
    void SaveCamSensor(CameraSensor cs, string prefix, string suffix)
    {
        if (cs != null)
        {
            Texture2D image = cs.GetImage();

            ImageSaveJob ij = new ImageSaveJob();
        
            ij.filename = GetImageFileName();

            ij.bytes = image.EncodeToJPG();
        
            lock (this)
            {
                imagesToSave.Add(ij);
            }
        }
    }

    public void SaverThread()
	{
		while(runThread)
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
        KillThread();

        bDoLog = false;
	}

	void OnDestroy()
	{
		Shutdown();
	}
}
