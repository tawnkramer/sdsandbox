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

    public class TcpMenuHandler : MonoBehaviour {

        public SceneLoader loader;

        private tk.JsonTcpClient client;
        float connectTimer = 1.0f;
        float timer = 0.0f;
        
        public enum State
        {
            UnConnected,
            Connected
        }        

        public State state = State.UnConnected;
        State prev_state = State.UnConnected;

        void Awake()
        {
            client = GetComponent<tk.JsonTcpClient>();
        }

        void Start()
        {
            Initcallbacks();
        }

        void Initcallbacks()
        {
            client.dispatcher.Register("load_scene", new tk.Delegates.OnMsgRecv(OnLoadScene));
            client.dispatcher.Register("get_protocol_version", new tk.Delegates.OnMsgRecv(OnProtocolVersion));
            client.dispatcher.Register("get_scene_names", new tk.Delegates.OnMsgRecv(OnGetSceneNames));
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
        
        void OnProtocolVersion(JSONObject msg)
        {
            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("msg_type", "protocol_version");
            json.AddField("version", "2");
            
            client.SendMsg( json );
        }

        void OnConnected()
        {
            SendFELoaded();
        }

        private void SendFELoaded()
        {
            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("msg_type", "scene_selection_ready");
            json.AddField("loaded", "1");        

            client.SendMsg( json );
        }

        void OnGetSceneNames(JSONObject jsonObject)
        {
            SendSceneNames();
        }

        private void SendSceneNames()
        {
            JSONObject scenes = new JSONObject(JSONObject.Type.ARRAY);

            scenes.Add("generated_road");
            scenes.Add("warehouse");
            scenes.Add("sparkfun_avc");
            scenes.Add("generated_track");

            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("scene_names", scenes);
            json.AddField("msg_type", "scene_names");

            client.SendMsg( json );
        }

        void OnLoadScene(JSONObject jsonObject)
        {
            //Set these flags to trigger an auto reconnect when we load the new scene.
            GlobalState.bAutoConnectToWebSocket = true;
            GlobalState.bAutoHideSceneMenu = true;

            string scene_name = jsonObject.GetField("scene_name").str;

            if(scene_name == "generated_road")
            {
                loader.LoadGenerateRoadScene();
            }
            else if (scene_name == "warehouse")
            {
                loader.LoadWarehouseScene();
            }
            else if (scene_name == "sparkfun_avc")
            {
                loader.LoadAVCScene();
            }
            else if (scene_name == "generated_track")
            {
                loader.LoadGeneratedTrackScene();
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
                        state = State.Connected;
                        OnConnected();
                    }
                }
            }
        }
    }
}
