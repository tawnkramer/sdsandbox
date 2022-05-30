using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Globalization;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;
using UnityEngine.AI;
using System.Collections.Generic;

namespace tk
{

    public class TcpCarHandler : MonoBehaviour
    {

        public GameObject carObj;
        public ICar car;
        public CarSpawner carSpawner;
        public PathManager pm;
        public CarConfig conf;

        // Sensors
        public CameraSensor camSensor;
        public CameraSensor camSensorB;
        public Lidar lidar;
        public Odometry[] odom;

        private tk.JsonTcpClient client;
        public Text ai_text;

        public float limitFPS = 20.0f;
        float timeSinceLastCapture = 0.0f;
        public float timeSinceLastMoved = 0.0f;
        public Vector3 lastPos = Vector3.zero;
        public float lastDistanceTraveled = 0.0f;

        float steer_to_angle = 16.0f;

        float ai_steering = 0.0f;
        float ai_throttle = 0.0f;
        float ai_brake = 0.0f;

        int iActiveSpan = 0;

        bool asynchronous = true;
        float time_step = 0.1f;
        bool bResetCar = false;
        bool bExitScene = false;

        public enum State
        {
            UnConnected,
            SendTelemetry
        }

        public State state = State.UnConnected;

        void Awake()
        {
            car = carObj.GetComponent<ICar>();
            conf = carObj.GetComponent<CarConfig>();
            pm = GameObject.FindObjectOfType<PathManager>();
            carSpawner = GameObject.FindObjectOfType<CarSpawner>();
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            GameObject go = CarSpawner.getChildGameObject(canvas.gameObject, "AISteering");
            if (go != null)
                ai_text = go.GetComponent<Text>();

            if (pm != null && carObj != null)
            {
                iActiveSpan = pm.carPath.GetClosestSpanIndex(carObj.transform.position);
            }
        }

        public void Init(tk.JsonTcpClient _client)
        {
            client = _client;

            if (client == null)
                return;

            client.dispatchInMainThread = false; //too slow to wait.
            client.dispatcher.Register("get_protocol_version", new tk.Delegates.OnMsgRecv(OnProtocolVersion));
            client.dispatcher.Register("control", new tk.Delegates.OnMsgRecv(OnControlsRecv));
            client.dispatcher.Register("exit_scene", new tk.Delegates.OnMsgRecv(OnExitSceneRecv));
            client.dispatcher.Register("reset_car", new tk.Delegates.OnMsgRecv(OnResetCarRecv));
            client.dispatcher.Register("step_mode", new tk.Delegates.OnMsgRecv(OnStepModeRecv));
            client.dispatcher.Register("quit_app", new tk.Delegates.OnMsgRecv(OnQuitApp));
            client.dispatcher.Register("regen_road", new tk.Delegates.OnMsgRecv(OnRegenRoad));
            client.dispatcher.Register("car_config", new tk.Delegates.OnMsgRecv(OnCarConfig));
            client.dispatcher.Register("cam_config", new tk.Delegates.OnMsgRecv(OnCamConfig));
            client.dispatcher.Register("cam_config_b", new tk.Delegates.OnMsgRecv(OnCamConfigB));
            client.dispatcher.Register("lidar_config", new tk.Delegates.OnMsgRecv(OnLidarConfig));
            client.dispatcher.Register("set_position", new tk.Delegates.OnMsgRecv(OnSetPosition));
            client.dispatcher.Register("node_position", new tk.Delegates.OnMsgRecv(OnNodePositionRecv));
        }

        public void Start()
        {
            SendCarLoaded();
            state = State.SendTelemetry;
        }

        public tk.JsonTcpClient GetClient()
        {
            return client;
        }

        public void OnDestroy()
        {
            if (client != null && client.dispatcher != null)
                client.dispatcher.Reset();
        }

        void Disconnect()
        {
            client.Disconnect();
        }

        public void Boot()
        {
            if (carSpawner != null)
            {
                if (client != null) { Disconnect(); }
                carSpawner.RemoveCar(client);
            }
        }

        void OnProtocolVersion(JSONObject msg)
        {
            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("msg_type", "protocol_version");
            json.AddField("version", "2");

            client.SendMsg(json);
        }

        void SendTelemetry()
        {
            if (client == null)
                return;

            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("msg_type", "telemetry");

            json.AddField("steering_angle", car.GetSteering() / steer_to_angle);
            json.AddField("throttle", car.GetThrottle());

            json.AddField("image", Convert.ToBase64String(camSensor.GetImageBytes()));

            if (camSensorB != null && camSensorB.gameObject.activeInHierarchy)
            {
                json.AddField("imageb", Convert.ToBase64String(camSensorB.GetImageBytes()));
            }

            if (lidar != null && lidar.gameObject.activeInHierarchy)
            {
                json.AddField("lidar", lidar.GetOutputAsJson());
            }

            foreach (Odometry o in odom)
            {
                json.AddField(o.Label, o.GetNumberRotations());
            }


            json.AddField("hit", car.GetLastCollision());
            car.ClearLastCollision();
            json.AddField("time", Time.timeSinceLevelLoad);

            Vector3 velocity = car.GetVelocity() / 8.0f;
            json.AddField("speed", velocity.magnitude);

            Vector3 accel = car.GetAccel() / 8.0f;
            json.AddField("accel_x", accel.x);
            json.AddField("accel_y", accel.y);
            json.AddField("accel_z", accel.z);

            Vector3 gyro = car.GetGyro();
            json.AddField("gyro_x", gyro.x);
            json.AddField("gyro_y", gyro.y);
            json.AddField("gyro_z", gyro.z);

            Transform tm = car.GetTransform();
            Vector3 eulerAngles = tm.rotation.eulerAngles;
            json.AddField("pitch", eulerAngles.x);
            json.AddField("yaw", eulerAngles.y);
            json.AddField("roll", eulerAngles.z);

            if (pm != null)
            {
                float cte = 0.0f;
                pm.carPath.GetCrossTrackErr(tm.position, ref iActiveSpan, ref cte); // get distance to closest node
                if (GlobalState.extendedTelemetry) { json.AddField("cte", cte); }

                json.AddField("activeNode", iActiveSpan);
                json.AddField("totalNodes", pm.carPath.nodes.Count);
            }

            // not intended to use in races, just to train 
            if (GlobalState.extendedTelemetry)
            {
                Vector3 pos = tm.position / 8.0f;
                json.AddField("pos_x", pos.x);
                json.AddField("pos_y", pos.y);
                json.AddField("pos_z", pos.z);

                json.AddField("vel_x", velocity.x);
                json.AddField("vel_y", velocity.y);
                json.AddField("vel_z", velocity.z);
            }
            client.SendMsg(json);
        }

        void SendCarLoaded()
        {
            if (client == null)
                return;

            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("msg_type", "car_loaded");
            client.SendMsg(json);
            Debug.Log("car loaded.");
        }

        float clamp(float val, float low, float high)
        {
            float ret = val;
            if (val > high)
                ret = high;
            else if (val < low)
                ret = low;
            return ret;
        }

        void OnControlsRecv(JSONObject json)
        {
            try
            {
                ai_steering = float.Parse(json["steering"].str, CultureInfo.InvariantCulture.NumberFormat);
                ai_throttle = float.Parse(json["throttle"].str, CultureInfo.InvariantCulture.NumberFormat);
                ai_brake = float.Parse(json["brake"].str, CultureInfo.InvariantCulture.NumberFormat);

                ai_steering = clamp(ai_steering, -1.0f, 1.0f);
                ai_throttle = clamp(ai_throttle, -1.0f, 1.0f);
                ai_brake = clamp(ai_brake, 0.0f, 1.0f);

                ai_steering *= steer_to_angle;

                car.RequestSteering(ai_steering);
                car.RequestThrottle(ai_throttle);
                car.RequestFootBrake(ai_brake);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        void OnExitSceneRecv(JSONObject json)
        {
            bExitScene = true;
        }

        void ExitScene()
        {
            SceneManager.LoadSceneAsync(0);
        }

        void OnResetCarRecv(JSONObject json)
        {
            bResetCar = true;
        }

        void OnRegenRoad(JSONObject json)
        {
            //This causes the track to be regenerated with the given settings.
            //This only works in scenes that have random track generation enabled.
            //For now that is only in scene road_generator.
            //An index into our track options. 5 in scene RoadGenerator.
            int road_style = int.Parse(json.GetField("road_style").str);
            int rand_seed = int.Parse(json.GetField("rand_seed").str);
            float turn_increment = float.Parse(json.GetField("turn_increment").str, CultureInfo.InvariantCulture);

            //We get this callback in a worker thread, but need to make mainthread calls.
            //so use this handy utility dispatcher from
            // https://github.com/PimDeWitte/UnityMainThreadDispatcher
            UnityMainThreadDispatcher.Instance().Enqueue(RegenRoad(road_style, rand_seed, turn_increment));
        }

        IEnumerator RegenRoad(int road_style, int rand_seed, float turn_increment)
        {
            TrainingManager train_mgr = GameObject.FindObjectOfType<TrainingManager>();
            PathManager path_mgr = GameObject.FindObjectOfType<PathManager>();

            if (train_mgr != null)
            {
                if (turn_increment != 0.0 && path_mgr != null)
                {
                    path_mgr.turnInc = turn_increment;
                }

                UnityEngine.Random.InitState(rand_seed);
                train_mgr.SetRoadStyle(road_style);
                train_mgr.OnMenuRegenTrack();
            }

            yield return null;
        }

        void OnCarConfig(JSONObject json)
        {
            Debug.Log("Got car config message");

            string body_style = json.GetField("body_style").str;
            int body_r = int.Parse(json.GetField("body_r").str);
            int body_g = int.Parse(json.GetField("body_g").str);
            int body_b = int.Parse(json.GetField("body_b").str);
            string car_name = json.GetField("car_name").str;
            int font_size = 100;

            if (json.GetField("font_size") != null)
                font_size = int.Parse(json.GetField("font_size").str);

            if (carObj != null && car_name != "Racer Name")
                UnityMainThreadDispatcher.Instance().Enqueue(SetCarConfig(body_style, body_r, body_g, body_b, car_name, font_size));
        }

        IEnumerator SetCarConfig(string body_style, int body_r, int body_g, int body_b, string car_name, int font_size)
        {
            CarConfig conf = carObj.GetComponent<CarConfig>();

            if (conf)
            {
                conf.SetStyle(body_style, body_r, body_g, body_b, car_name, font_size);
            }

            yield return null;
        }

        void OnCamConfig(JSONObject json)
        {
            ParseCamConfig(json, 0);
        }

        void OnCamConfigB(JSONObject json)
        {
            ParseCamConfig(json, 1);
        }

        void ParseCamConfig(JSONObject json, int iCamera)
        {
            float fov = float.Parse(json.GetField("fov").str, CultureInfo.InvariantCulture.NumberFormat);
            float offset_x = float.Parse(json.GetField("offset_x").str, CultureInfo.InvariantCulture.NumberFormat);
            float offset_y = float.Parse(json.GetField("offset_y").str, CultureInfo.InvariantCulture.NumberFormat);
            float offset_z = float.Parse(json.GetField("offset_z").str, CultureInfo.InvariantCulture.NumberFormat);
            float rot_x = float.Parse(json.GetField("rot_x").str, CultureInfo.InvariantCulture.NumberFormat);
            float rot_y = 0f;
            float rot_z = 0f;
            if (json.HasField("rot_y")) { rot_y = float.Parse(json.GetField("rot_y").str, CultureInfo.InvariantCulture.NumberFormat); }
            if (json.HasField("rot_z")) { rot_z = float.Parse(json.GetField("rot_z").str, CultureInfo.InvariantCulture.NumberFormat); }
            float fish_eye_x = float.Parse(json.GetField("fish_eye_x").str, CultureInfo.InvariantCulture.NumberFormat);
            float fish_eye_y = float.Parse(json.GetField("fish_eye_y").str, CultureInfo.InvariantCulture.NumberFormat);
            int img_w = int.Parse(json.GetField("img_w").str);
            int img_h = int.Parse(json.GetField("img_h").str);
            int img_d = int.Parse(json.GetField("img_d").str);
            string img_enc = json.GetField("img_enc").str;

            if (carObj != null)
                UnityMainThreadDispatcher.Instance().Enqueue(SetCamConfig(iCamera, fov, offset_x, offset_y, offset_z, rot_x, rot_y, rot_z, img_w, img_h, img_d, img_enc, fish_eye_x, fish_eye_y));
        }

        IEnumerator SetCamConfig(int iCamera, float fov, float offset_x, float offset_y, float offset_z, float rot_x,
            float rot_y, float rot_z, int img_w, int img_h, int img_d, string img_enc, float fish_eye_x, float fish_eye_y)
        {
            CameraSensor cam = null;

            if (iCamera == 0)
                cam = camSensor;
            else
            {
                cam = camSensorB;

                if (cam != null && !cam.gameObject.activeInHierarchy)
                {
                    cam.gameObject.SetActive(true);
                }
            }

            if (cam)
            {
                cam.SetConfig(fov, offset_x, offset_y, offset_z, rot_x, rot_y, rot_z, img_w, img_h, img_d, img_enc);

                Fisheye fe = cam.gameObject.GetComponent<Fisheye>();

                if (fe != null && (fish_eye_x != 0.0f || fish_eye_y != 0.0f))
                {
                    fe.enabled = true;
                    fe.strengthX = fish_eye_x;
                    fe.strengthY = fish_eye_y;
                }
            }

            yield return null;
        }

        void OnSetPosition(JSONObject json)
        {
            if (GlobalState.extendedTelemetry)
            {
                float pos_x = float.Parse(json.GetField("pos_x").str, CultureInfo.InvariantCulture.NumberFormat);
                float pos_y = float.Parse(json.GetField("pos_y").str, CultureInfo.InvariantCulture.NumberFormat);
                float pos_z = float.Parse(json.GetField("pos_z").str, CultureInfo.InvariantCulture.NumberFormat);
                Quaternion rot = Quaternion.identity;
                if (json.GetField("Qx") != null && json.GetField("Qy") != null && json.GetField("Qz") != null && json.GetField("Qw") != null)
                {
                    float qx = float.Parse(json.GetField("Qx").str, CultureInfo.InvariantCulture.NumberFormat);
                    float qy = float.Parse(json.GetField("Qy").str, CultureInfo.InvariantCulture.NumberFormat);
                    float qz = float.Parse(json.GetField("Qz").str, CultureInfo.InvariantCulture.NumberFormat);
                    float qw = float.Parse(json.GetField("Qw").str, CultureInfo.InvariantCulture.NumberFormat);

                    rot.x = qx;
                    rot.y = qy;
                    rot.z = qz;
                    rot.w = qw;
                }

                UnityMainThreadDispatcher.Instance().Enqueue(setCarPosition(pos_x, pos_y, pos_z, rot));
            }
        }

        IEnumerator setCarPosition(float pos_x, float pos_y, float pos_z, Quaternion rot)
        {
            carObj.transform.position = new Vector3(pos_x, pos_y, pos_z);
            carObj.transform.rotation = rot;
            yield return null;
        }

        void OnLidarConfig(JSONObject json)
        {
            float offset_x = float.Parse(json.GetField("offset_x").str, CultureInfo.InvariantCulture.NumberFormat);
            float offset_y = float.Parse(json.GetField("offset_y").str, CultureInfo.InvariantCulture.NumberFormat);
            float offset_z = float.Parse(json.GetField("offset_z").str, CultureInfo.InvariantCulture.NumberFormat);
            float rot_x = float.Parse(json.GetField("rot_x").str, CultureInfo.InvariantCulture.NumberFormat);

            float degPerSweepInc = float.Parse(json.GetField("degPerSweepInc").str, CultureInfo.InvariantCulture.NumberFormat);
            float degAngDown = float.Parse(json.GetField("degAngDown").str, CultureInfo.InvariantCulture.NumberFormat);
            float degAngDelta = float.Parse(json.GetField("degAngDelta").str, CultureInfo.InvariantCulture.NumberFormat);
            float maxRange = float.Parse(json.GetField("maxRange").str, CultureInfo.InvariantCulture.NumberFormat);
            float noise = float.Parse(json.GetField("noise").str, CultureInfo.InvariantCulture.NumberFormat);
            int numSweepsLevels = int.Parse(json.GetField("numSweepsLevels").str);


            if (carObj != null)
                UnityMainThreadDispatcher.Instance().Enqueue(SetLidarConfig(offset_x, offset_y, offset_z, rot_x, degPerSweepInc, degAngDown, degAngDelta, maxRange, noise, numSweepsLevels));
        }

        IEnumerator SetLidarConfig(float offset_x, float offset_y, float offset_z, float rot_x,
            float degPerSweepInc, float degAngDown, float degAngDelta, float maxRange, float noise, int numSweepsLevels)
        {
            if (lidar != null)
            {
                if (!lidar.gameObject.activeInHierarchy)
                    lidar.gameObject.SetActive(true);

                lidar.SetConfig(offset_x, offset_y, offset_z, rot_x, degPerSweepInc, degAngDown, degAngDelta, maxRange, noise, numSweepsLevels);
            }

            yield return null;
        }

        void OnStepModeRecv(JSONObject json)
        {
            string step_mode = json.GetField("step_mode").str;
            float _time_step = float.Parse(json.GetField("time_step").str);

            Debug.Log("got settings");

            if (step_mode == "synchronous")
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

        public IEnumerator SendCollisionWithStartingLine(int startingLineIndex, float timeStamp)
        {
            if (client != null)
            {
                JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
                json.AddField("msg_type", "collision_with_starting_line");
                json.AddField("starting_line_index", startingLineIndex);
                json.AddField("timeStamp", timeStamp);
                client.SendMsg(json);
            }

            yield return null;
        }

        public void OnNodePositionRecv(JSONObject json)
        {
            int index = int.Parse(json.GetField("index").str);
            if (pm != null && index >= 0 && index < pm.carPath.nodes.Count)
            {
                PathNode node = pm.carPath.nodes[index];
                UnityMainThreadDispatcher.Instance().Enqueue(SendNodePosition(index, node));
            }
        }

        public IEnumerator SendNodePosition(int index, PathNode node)
        {

            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("msg_type", "node_position");
            json.AddField("index", index);
            json.AddField("pos_x", node.pos.x);
            json.AddField("pos_y", node.pos.y);
            json.AddField("pos_z", node.pos.z);
            json.AddField("Qx", node.rotation.x);
            json.AddField("Qy", node.rotation.y);
            json.AddField("Qz", node.rotation.z);
            json.AddField("Qw", node.rotation.w);
            client.SendMsg(json);
            yield return null;
        }

        void OnQuitApp(JSONObject json)
        {
            Application.Quit();
        }

        void FixedUpdate()
        {
            if (bExitScene)
            {
                bExitScene = false;
                ExitScene();
            }

            if (state == State.SendTelemetry)
            {
                if (bResetCar)
                {
                    car.RestorePosRot();

                    if (carObj != null)
                    {
                        //reset last controls
                        car.RequestSteering(0.0f);
                        car.RequestThrottle(0.0f);
                        car.RequestFootBrake(10.0f);

                        // Reset closest point of car path
                        if (pm)
                            iActiveSpan = 0;
                    }

                    bResetCar = false;
                }

                timeSinceLastCapture += Time.fixedDeltaTime;
                if (timeSinceLastCapture >= 1.0f / limitFPS)
                {
                    timeSinceLastCapture -= (1.0f / limitFPS);
                    SendTelemetry();
                }

                if (ai_text != null)
                    ai_text.text = string.Format("NN: {0} : {1}", ai_steering, ai_throttle);

                Vector3 currentPos = car.GetTransform().position;
                float distance = Vector3.Distance(currentPos, lastPos);

                if (distance < 1f)
                {
                    timeSinceLastMoved += Time.fixedDeltaTime;
                }
                else
                {
                    timeSinceLastMoved = 0.0f;
                    lastPos = currentPos;
                }
                if (timeSinceLastMoved >= GlobalState.timeOut && carSpawner != null)
                {
                    Boot();
                }
            }
        }

        public bool IsGhostCar()
        {
            if (client == null) { return true; }
            else { return false; }
        }
    }
}
