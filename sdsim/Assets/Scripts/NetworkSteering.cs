using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine.UI;

public class NetworkSteering : MonoBehaviour {

	public GameObject carObj;
	public ICar car;
	public CameraSensor camSensor;
	public string nnIPAddress = "127.0.0.1";
	public int nnPort = 9090;
	public NetworkClient client;
	public float connectTimer = 3.0f;
	float timer = 0.0f;
	public bool doSend = true;
	public Text ai_steering;
	public float timeInCurrentState = 0.0f;
	public bool doThrottle = false;
	public float throttleScale = 1.0f / 255.0f;
	public RawImage sensorPreview;

	//profile the resonsiveness of the nn
	public float time_asked = 0.0f;
	public float total_req_time = 0.0f;
	public float num_requests = 0.0f;
	public float avg_req_time = 0.0f;

	public enum State
	{
		UnConnected,
		Idle,
		WaitingForServerReadyImage,
		ReadyToSendImage,
		WaitingForSteering,
		ProcessSteering
	}

	[Serializable]
	public class ImageHeader
	{
		public int num_bytes;
		public int width;
		public int height;
		public int num_channels;
		public string format;
		public int flip_y;
	}

	[Serializable]
	public class ServerMessage
	{
		public float steering;
		public float throttle;
	}

	public State state = State.UnConnected;
	State prev_state = State.UnConnected;

	ServerMessage controlMsg = new ServerMessage();

	void Awake()
	{
		car = carObj.GetComponent<ICar>();
	}

	void Start()
	{
		Initcallbacks();
	}

	void Initcallbacks()
	{
		client.onDataRecvCB += new NetworkClient.OnDataRecv(OnDataRecv);
	}

	bool Connect()
	{
		return client.Connect(nnIPAddress, nnPort);
	}

	void Disconnect()
	{
		client.Disconnect();
	}

	void Reconnect()
	{
		Disconnect();
		Connect();
	}

	void SendNewImageHeader()
	{
		time_asked = Time.realtimeSinceStartup;
		num_requests += 1;

		ImageHeader im = new ImageHeader();
		im.height = camSensor.height;
		im.width = camSensor.width;
		im.num_channels = 3;
		im.num_bytes = im.height * im.width * im.num_channels;
		im.flip_y = 1;
		im.format = "array_of_pixels";
		
		string header = JsonUtility.ToJson(im);

		byte[] bytes = System.Text.Encoding.UTF8.GetBytes(header);

		client.SendData( bytes );

		state = State.WaitingForServerReadyImage;
	}

	void UpdateRequesProfiler()
	{
		float delta = Time.realtimeSinceStartup - time_asked;
		total_req_time += delta;
		avg_req_time = total_req_time / num_requests;

		if(num_requests == 120)
		{
			num_requests = 0.0f;
			total_req_time = 0.0f;
		}
	}

	void OnDataRecv(byte[] bytes)
	{
		string str = System.Text.Encoding.UTF8.GetString(bytes);

		if(state == State.WaitingForServerReadyImage)
		{
			if(str == "{ 'response' : 'ready_for_image' }" )
			{
				state = State.ReadyToSendImage;
			}
			else
			{
				Debug.LogWarning("unexpected reply: " + str);
				state = State.Idle;
			}
		}
		else if(state == State.WaitingForSteering)
		{
			try
			{
				controlMsg = JsonUtility.FromJson<ServerMessage>(str);
			}
			catch(Exception e)
			{
				Debug.Log(e.ToString());
			}

			state = State.ProcessSteering;
		}
		else
		{
			if(str == "{ 'response' : 'ready_for_image' }" )
			{
				state = State.ReadyToSendImage;
			}
			else
			{
				Debug.LogWarning("unexpected reply: " + str);
				state = State.Idle;
			}
		}	
	}

	void SendNewImage()
	{
		if(camSensor != null)
		{
			Texture2D image = camSensor.GetImage();

			byte[] bytes = image.GetRawTextureData();

			client.SendData(bytes);

			state = State.WaitingForSteering;

			if(sensorPreview != null)
			{
				sensorPreview.texture = image;
			}
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!doSend)
			return;

		//when state changes, reset our timer.
		if(prev_state != state)
		{
			timeInCurrentState = 0.0f;
			prev_state = state;
		}
		else
		{
			timeInCurrentState += Time.deltaTime;
		}
			
		if(state == State.UnConnected)
		{
			timer += Time.deltaTime;

			if(timer > connectTimer)
			{
				timer = 0.0f;

				if(Connect())
					state = State.Idle;
			}
		}
		else if(state == State.Idle)
		{
			SendNewImageHeader();
		}
		else if(state == State.ReadyToSendImage)
		{
			SendNewImage();
		}
		else if(state == State.ProcessSteering)
		{
			UpdateRequesProfiler();

			car.RequestSteering(controlMsg.steering);

			if(doThrottle)
			{
				float val = controlMsg.throttle * throttleScale;

				if(val > 0.0f)
					car.RequestThrottle(val);
				else
					car.RequestFootBrake(-val);
			}

			if(ai_steering != null)
				ai_steering.text = string.Format("NN: {0}", controlMsg.steering /* avg_req_time */);

			state = State.Idle;
		}
		else if(state == State.WaitingForServerReadyImage && timeInCurrentState > 2.0f)
		{
			Debug.LogWarning("Stuck in " + state.ToString());
			state = State.Idle;
			Reconnect();
		}
		else if(state == State.WaitingForSteering && timeInCurrentState > 2.0f)
		{
			Debug.LogWarning("Stuck in " + state.ToString());
			state = State.Idle;
			Reconnect();
		}
	}
}
