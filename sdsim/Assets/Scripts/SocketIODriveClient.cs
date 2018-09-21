using SocketIO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent (typeof(SocketIOComponent))]
public class SocketIODriveClient : MonoBehaviour {

    public GameObject carObj;
    public ICar car;

    public PathManager pm;
    public Camera camSensor;
    private SocketIOComponent _socket;
    bool collectData = false;

    public Text ai_steering;

    bool runThread = false;
    Thread thread;
   
    List<SimMessage> messages;

    public bool scaleSteeringInput = false;
    private bool connected = false;

    private bool asynchronous = true;

    private float time_step = 0.1f;


    // Use this for initialization
    void Start()
    {
        OnLoaded();
    }

    private void OnEnable()
    {
        Init();

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

    private void Init()
    {
        if (messages != null)
            return;

        _socket = GetComponent<SocketIOComponent>();
        _socket.On("open", OnOpen);
        _socket.On("Steer", OnSteer);
        _socket.On("RequestTelemetry", onRequestTelemetry);
        _socket.On("ExitScene", onExitScene);
        _socket.On("QuitApp", onQuitApp);
        _socket.On("ResetCar", onResetCar);
        _socket.On("Settings", onSettings);

        messages = new List<SimMessage>();

        car = carObj.GetComponent<ICar>();
    }

    //sending from the main thread was really slowing things down. Not sure why.
    //Sending from this thread changed the framerate from 5fps to 60
    public void SendThread()
    {
        while (runThread)
        {
            lock (this)
            {
                if(messages.Count != 0 && connected)
                {
                    foreach(SimMessage m in messages)
                    {
                        _socket.Emit(m.messageId, m.json);
                    }

                    messages.Clear();
                }
            }
        }
    }

    public void QueueMessage(SimMessage m)
    {
        lock (this)
        {
            messages.Add(m);
        }
    }

    private void Update()
    {
        if (collectData)
        {
            collectData = false;

            SimMessage m = new SimMessage();
            m.json = new JSONObject(JSONObject.Type.OBJECT);
            m.messageId = "Telemetry";

            m.json.AddField("steering_angle", car.GetSteering());
            m.json.AddField("throttle", car.GetThrottle());
            m.json.AddField("speed", car.GetVelocity().magnitude);
            m.json.AddField("image", System.Convert.ToBase64String(CameraHelper.CaptureFrame(camSensor)));
            
            m.json.AddField("hit", car.GetLastCollision());
            car.ClearLastCollision();

            Transform tm = car.GetTransform();
            m.json.AddField("pos_x", tm.position.x);
            m.json.AddField("pos_y", tm.position.y);
            m.json.AddField("pos_z", tm.position.z);

            m.json.AddField("time", Time.timeSinceLevelLoad);

            if(pm != null)
            {
                float cte = 0.0f;
                if(pm.path.GetCrossTrackErr(tm.position, ref cte))
                {
                    m.json.AddField("cte", cte);
                }
                else
                {
                    pm.path.ResetActiveSpan();
                    m.json.AddField("cte", 0.0f);
                }
            }
            
            QueueMessage(m);
        }
    }

    void OnLoaded()
    {
        SimMessage m = new SimMessage();
        m.json = new JSONObject(JSONObject.Type.OBJECT);
        m.messageId = "SceneLoaded";
        m.json.AddField("none", "none");

        car.SavePosRot();

        QueueMessage(m);
    }

    void OnOpen(SocketIOEvent obj)
    {
        Debug.Log("Connection Open");
        connected = true;
        EmitTelemetry();
    }

    void onRequestTelemetry(SocketIOEvent obj)
    {
        EmitTelemetry();
    }

    void OnSteer(SocketIOEvent obj)
    {
        JSONObject jsonObject = obj.data;

        float steering = float.Parse(jsonObject.GetField("steering_angle").str);
		float throttle = float.Parse(jsonObject.GetField("throttle").str);

        if(scaleSteeringInput)
            steering = steering * car.GetMaxSteering();

        car.RequestSteering(steering);
		car.RequestThrottle(throttle);
		car.RequestFootBrake(0.0f);
		car.RequestHandBrake(0.0f);

        if(ai_steering != null)
			ai_steering.text = string.Format("NN: {0} {1}", steering, throttle);

        if(asynchronous)
        {
            EmitTelemetry();
        }
        else
        {
            Time.timeScale = 1.0f;
            //Debug.Log(Time.timeScale);
            StartCoroutine(WaitCollectTelemThenPause());
        }
    }

    IEnumerator WaitCollectTelemThenPause()
    {
        yield return new WaitForSeconds(time_step);
        EmitTelemetry();
        Time.timeScale = 0.0f;
        //Debug.Log(Time.timeScale);
    }

    void EmitTelemetry()
    {
        collectData = true;
    }

    void onExitScene(SocketIOEvent obj)
    {
        SceneManager.LoadSceneAsync(0);
    }

    void onResetCar(SocketIOEvent obj)
    {
        car.RestorePosRot();
        EmitTelemetry();
    }

    void onSettings(SocketIOEvent obj)
    {
        JSONObject jsonObject = obj.data;

        string step_mode = jsonObject.GetField("step_mode").str;
		float _time_step = float.Parse(jsonObject.GetField("time_step").str);

        Debug.Log("got settings");

        if(step_mode == "synchronous")
        {
            Debug.Log("setting mode to synchronous");
            asynchronous = false;
            this.time_step = _time_step;
            Time.timeScale = 0.0f;
            //Debug.Log(Time.timeScale);
        }
        else
        {
            Debug.Log("setting mode to asynchronous");
            asynchronous = true;
        }
    }

    void onQuitApp(SocketIOEvent obj)
    {
        Application.Quit();
    }
}
