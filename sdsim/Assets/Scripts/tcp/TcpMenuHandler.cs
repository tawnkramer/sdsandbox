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
    
    public class TcpMenuHandler : MonoBehaviour {

        public SceneLoader loader;
        public string[] scene_names;
        private tk.JsonTcpClient client;

        public void Init(tk.JsonTcpClient _client)
        {
            _client.dispatchInMainThread = true;

            client = _client;
            client.dispatcher.Register("load_scene", new tk.Delegates.OnMsgRecv(OnLoadScene));
            client.dispatcher.Register("get_protocol_version", new tk.Delegates.OnMsgRecv(OnProtocolVersion));
            client.dispatcher.Register("get_scene_names", new tk.Delegates.OnMsgRecv(OnGetSceneNames));
            client.dispatcher.Register("quit_app", new tk.Delegates.OnMsgRecv(OnQuitApp));
            client.dispatcher.Register("connected", new tk.Delegates.OnMsgRecv(OnConnected));
        }

        public void Start()
        {
        }

        public void OnDestroy()
        {
            if(client)
                client.dispatcher.Reset();
        }

        void Disconnect()
        {
            client.Disconnect();
        }
      
        void OnProtocolVersion(JSONObject msg)
        {
            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("msg_type", "protocol_version");
            json.AddField("version", "2");
            
            client.SendMsg( json );
        }

        void OnConnected(JSONObject msg)
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

            foreach (string scene_name in scene_names)
            {
                scenes.Add(scene_name);
            }

            JSONObject json = new JSONObject(JSONObject.Type.OBJECT);
            json.AddField("scene_names", scenes);
            json.AddField("msg_type", "scene_names");

            client.SendMsg( json );
        }

        void OnLoadScene(JSONObject jsonObject)
        {
            GlobalState.bAutoHideSceneMenu = true;

            // since we know this is called only from a network client,
            // we can also infer that we don't want to auto create 
            GlobalState.bCreateCarWithoutNetworkClient = false;

            string scene_name = jsonObject.GetField("scene_name").str;
            LoadScene(scene_name);
        }
        
        public void LoadScene(string scene_name)
        {
            // check wether the scene_name is in the scene_names list, if so, load it
            if(Array.Exists(scene_names, element => element == scene_name))
            {
                loader.LoadScene(scene_name);
                Debug.Log("loaded scene");
            }
        }

        void OnQuitApp(JSONObject json)
        {
            Application.Quit();
        }        
    }
}
