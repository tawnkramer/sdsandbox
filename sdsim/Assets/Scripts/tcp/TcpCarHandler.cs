using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine.UI;
using System.Globalization;
using UnityEngine.SceneManagement;


namespace tk
{
    [RequireComponent(typeof(tk.JsonTcpClient))]

    public class TcpCarHandler : MonoBehaviour {

        public GameObject carObj;
        public ICar car;

        public PathManager pm;
        public CameraSensor camSensor;
        private tk.JsonTcpClient client;
        float connectTimer = 1.0f;
        float timer = 0.0f;
        public Text ai_text;
        
        public float limitFPS = 20.0f;
        float timeSinceLastCapture = 0.0f;

        float steer_to_angle = 16.0f;

        float ai_steering = 0.0f;
        float ai_throttle = 0.0f;
        float ai_brake = 0.0f;

        bool asynchronous = true;
        float time_step = 0.1f;
        bool bResetCar = false;

        public enum State
        {
            UnConnected,
            SendTelemetry
        }        

        public State state = State.UnConnected;
        State prev_state = State.UnConnected;

        void Awake()
        {
            car = carObj.GetComponent<ICar>();
            client = GetComponent<tk.JsonTcpClient>();
		    pm = GameObject.FindObjectOfType<PathManager>();

            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            GameObject go = CarSpawner.getChildGameObject(canvas.gameObject, "AISteering");
            if (go != null)
                ai_text = go.GetComponent<Text>();
        }

        void Start()
        {
            Initcallbacks();
        }

        void Initcallbacks()
        {
            client.dispatcher.Register("control", new tk.Delegates.OnMsgRecv(OnControlsRecv));
            client.dispatcher.Register("exit_scene", new tk.Delegates.OnMsgRecv(OnExitSceneRecv));
            client.dispatcher.Register("reset_car", new tk.Delegates.OnMsgRecv(OnResetCarRecv));
            client.dispatcher.Register("settings", new tk.Delegates.OnMsgRecv(OnSettingsRecv));
            client.dispatcher.Register("quit_app", new tk.Delegates.OnMsgRecv(OnQuitApp));
        }

        bool Connect()
        {
            return client.Connect();
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

        void SendTelemetry()
        {
            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("msg_type", "telemetry");

            json.AddField("steering_angle", car.GetSteering() / steer_to_angle);
            json.AddField("throttle", car.GetThrottle());
            json.AddField("speed", car.GetVelocity().magnitude);
            json.AddField("image", System.Convert.ToBase64String(camSensor.GetImageBytes()));
            
            json.AddField("hit", car.GetLastCollision());
            car.ClearLastCollision();

            Transform tm = car.GetTransform();
            json.AddField("pos_x", tm.position.x);
            json.AddField("pos_y", tm.position.y);
            json.AddField("pos_z", tm.position.z);

            json.AddField("time", Time.timeSinceLevelLoad);

            if(pm != null)
            {
                float cte = 0.0f;
                if(pm.path.GetCrossTrackErr(tm.position, ref cte))
                {
                    json.AddField("cte", cte);
                }
                else
                {
                    pm.path.ResetActiveSpan();
                    json.AddField("cte", 0.0f);
                }
            }

            client.SendMsg( json );
        }

        void SendCarLoaded()
        {
            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("msg_type", "car_loaded");
            client.SendMsg( json );
        }

        void OnControlsRecv(JSONObject json)
        {
            try
            {
                ai_steering = float.Parse(json["steering"].str, CultureInfo.InvariantCulture.NumberFormat) * steer_to_angle;
                ai_throttle = float.Parse(json["throttle"].str, CultureInfo.InvariantCulture.NumberFormat);
                ai_brake = float.Parse(json["brake"].str, CultureInfo.InvariantCulture.NumberFormat);

                car.RequestSteering(ai_steering);
                car.RequestThrottle(ai_throttle);
                car.RequestFootBrake(ai_brake);
            }
            catch(Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        void OnExitSceneRecv(JSONObject json)
        {
            SceneManager.LoadSceneAsync(0);
        }

        void OnResetCarRecv(JSONObject json)
        {
            bResetCar = true;            
        }

        void OnSettingsRecv(JSONObject json)
        {
            string step_mode = json.GetField("step_mode").str;
            float _time_step = float.Parse(json.GetField("time_step").str);

            Debug.Log("got settings");

            if(step_mode == "synchronous")
            {
                Debug.Log("setting mode to synchronous");
                asynchronous = false;
                this.time_step = _time_step;
                Time.timeScale = 0.0f;
            }
            else
            {
                Debug.Log("setting mode to asynchronous");
                asynchronous = true;
            }
        }
    
        void OnQuitApp(JSONObject json)
        {
            Application.Quit();
        }
        
        // Update is called once per frame
        void Update () 
        {    
            if(state == State.UnConnected)
            {
                timer += Time.deltaTime;

                if(timer > connectTimer)
                {
                    timer = 0.0f;

                    if(Connect())
                    {
                        SendCarLoaded();
                        state = State.SendTelemetry;
                    }
                }
            }
            else if(state == State.SendTelemetry)
            {
                if (bResetCar)
                {
                    car.RestorePosRot();
                    pm.path.ResetActiveSpan();
                    bResetCar = false;
                }


                timeSinceLastCapture += Time.deltaTime;

                if (timeSinceLastCapture > 1.0f / limitFPS)
                {
                    timeSinceLastCapture -= (1.0f / limitFPS);
                    SendTelemetry();
                }
                
                if(ai_text != null)
                    ai_text.text = string.Format("NN: {0} : {1}", ai_steering, ai_throttle);
                    
            }
        }
    }
}
