using SocketIO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(SocketIOComponent))]
public class SocketIODriveClient : MonoBehaviour {

    public GameObject carObj;
    public ICar car;
    public Camera camSensor;
    private SocketIOComponent _socket;
    bool collectData = false;

    public Text ai_steering;

    bool runThread = false;
    Thread thread;
    Dictionary<string, string> data;


    // Use this for initialization
    void Start()
    {
        _socket = GetComponent<SocketIOComponent>();
        _socket.On("open", OnOpen);
        _socket.On("steer", OnSteer);
        _socket.On("manual", onManual);

        car = carObj.GetComponent<ICar>();
    }

    private void OnEnable()
    {
        runThread = true;
        thread = new Thread(SendThread);
        thread.Start();
    }

    private void OnDisable()
    {
        car.RequestFootBrake(1.0f);

        runThread = false;
        thread.Abort();
    }

    //sending from the main thread was really slowing things down. Not sure why.
    //Sending from this thread changed the framerate from 5fps to 60
    public void SendThread()
    {
        while (runThread)
        {
            lock (this)
            {
                if(data != null)
                {
                    _socket.Emit("telemetry", new JSONObject(data));

                    data = null;
                }
            }
        }
    }


    private void Update()
    {
        if (collectData)
        {
            collectData = false;

            // Collect Data from the Car
            lock (this)
            {
                data = new Dictionary<string, string>();

                data["steering_angle"] = car.GetSteering().ToString("N4");
                data["throttle"] = car.GetThrottle().ToString("N4");
                data["speed"] = car.GetVelocity().magnitude.ToString("N4");
                data["image"] = System.Convert.ToBase64String(CameraHelper.CaptureFrame(camSensor));
            }
        }
    }

    void OnOpen(SocketIOEvent obj)
    {
        Debug.Log("Connection Open");
        EmitTelemetry(obj);
    }

    void onManual(SocketIOEvent obj)
    {
        EmitTelemetry(obj);
    }

    void OnSteer(SocketIOEvent obj)
    {
        JSONObject jsonObject = obj.data;

        float steering = float.Parse(jsonObject.GetField("steering_angle").str);
		float throttle = float.Parse(jsonObject.GetField("throttle").str);

        car.RequestSteering(steering);
		car.RequestThrottle(throttle);
		car.RequestFootBrake(0.0f);
		car.RequestHandBrake(0.0f);

        if(ai_steering != null)
			ai_steering.text = string.Format("NN: {0} {1}", steering, throttle);

        EmitTelemetry(obj);
    }

    void EmitTelemetry(SocketIOEvent obj)
    {
        collectData = true;
    }
}
