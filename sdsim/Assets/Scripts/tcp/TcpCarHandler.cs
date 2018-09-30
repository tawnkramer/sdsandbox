using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine.UI;
using System.Globalization;

namespace tk
{
    [RequireComponent(typeof(tk.JsonTcpClient))]

    public class TcpCarHandler : MonoBehaviour {

        public GameObject carObj;
        public ICar car;

        public PathManager pm;
        public CameraSensor camSensor;
        private tk.JsonTcpClient client;
        public float connectTimer = 3.0f;
        float timer = 0.0f;
        public Text ai_steering;
        public RawImage sensorPreview;

        //profile the resonsiveness of the nn
        public float time_asked = 0.0f;
        public float total_req_time = 0.0f;
        public float num_requests = 0.0f;
        public float avg_req_time = 0.0f;

        float steer_to_angle = 16.0f;

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
        }

        void Start()
        {
            Initcallbacks();
        }

        void Initcallbacks()
        {
            client.dispatcher.Register("control", new tk.Delegates.OnMsgRecv(OnControlsRecv));
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

        void OnControlsRecv(JSONObject json)
        {
            try
            {
                float steering = float.Parse(json["steering"].str, CultureInfo.InvariantCulture.NumberFormat) * steer_to_angle;
                float throttle = float.Parse(json["throttle"].str, CultureInfo.InvariantCulture.NumberFormat);
                //Debug.Log(steering.ToString());

                car.RequestSteering(steering);
                car.RequestThrottle(throttle);
                car.RequestFootBrake(json["brake"].f);            

                if(ai_steering != null)
                    ai_steering.text = string.Format("NN: {0}", steering /* avg_req_time */);
            }
            catch(Exception e)
            {
                Debug.Log(e.ToString());
            }
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
                        state = State.SendTelemetry;
                }
            }
            else if(state == State.SendTelemetry)
            {
                SendTelemetry();
            }
        }
    }
}
