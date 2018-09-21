using SocketIO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class SimMessage
{
    public JSONObject json;
    public string messageId;
}

[RequireComponent (typeof(SocketIOComponent))]
public class SocketIOMenuClient : MonoBehaviour {

    private SocketIOComponent _socket;
    
    bool runThread = false;
    Thread thread;
   
    List<SimMessage> messages;

    public SceneLoader loader;

    public bool bVerbose = false;

    public bool bConnected = false;


    // Use this for initialization
    void Start()
    {
        if(bVerbose)
            Debug.Log("Start.");

        SendProtocolVersion();
        SendFELoaded();
    }

    private void OnEnable()
    {
        Init();

        if(bVerbose)
            Debug.Log("OnEnable.");
        runThread = true;
        thread = new Thread(SendThread);
        thread.Start();
    }

    private void OnDisable()
    {
        if(bVerbose)
            Debug.Log("OnDisable.");
        runThread = false;
        thread.Abort();
    }

    private void Init()
    {
        if(messages != null)
            return;

        if(bVerbose)
            Debug.Log("Init.");
        _socket = GetComponent<SocketIOComponent>();
        _socket.On("open", OnOpen);
        _socket.On("GetProtocolVersion", OnProtocolVersion);
        _socket.On("GetSceneNames", OnGetSceneNames);
        _socket.On("LoadScene", OnLoadScene);
        _socket.On("QuitApp", onQuitApp);

        messages = new List<SimMessage>();
    }

    //sending from the main thread was really slowing things down. Not sure why.
    //Sending from this thread changed the framerate from 5fps to 60
    public void SendThread()
    {
        while (runThread)
        {
            lock (this)
            {
                if(messages.Count != 0 && bConnected)
                {
                    if(bVerbose)
                        Debug.Log("We have messages to send.");

                    foreach(SimMessage m in messages)
                    {
                        if(bVerbose)
                            Debug.Log("Sending message: " + m.messageId);

                        _socket.Emit(m.messageId, m.json);
                    }

                    messages.Clear();
                }
            }
        }

        if(bVerbose)
            Debug.Log("Thread is exiting.");
    }

    public void QueueMessage(SimMessage m)
    {
        lock (this)
        {
            if(bVerbose)
                Debug.Log("Queueing message: " + m.messageId);

            if(messages != null)
            {
                messages.Add(m);
            }
            else
            {
                Debug.LogWarning("message queue not inited yet.");
            }
        }
    }

    void OnOpen(SocketIOEvent obj)
    {
        Debug.Log("Connection Open");
        bConnected = true;
    }

    void OnGetSceneNames(SocketIOEvent obj)
    {
        SendSceneNames();
    }

    void OnProtocolVersion(SocketIOEvent obj)
    {
        SendProtocolVersion();
    }

    private void SendProtocolVersion()
    {
        SimMessage m = new SimMessage();
        m.json = new JSONObject(JSONObject.Type.OBJECT);
        m.messageId = "ProtocolVersion";
        m.json.AddField("version", "1");
            
        QueueMessage(m);
    }

    private void SendFELoaded()
    {
        SimMessage m = new SimMessage();
        m.json = new JSONObject(JSONObject.Type.OBJECT);
        m.messageId = "SceneSelectionReady";
        m.json.AddField("loaded", "1");        

        QueueMessage(m);
    }

    private void SendSceneNames()
    {
        SimMessage m = new SimMessage();
        m.messageId = "SceneNames";

        JSONObject scenes = new JSONObject(JSONObject.Type.ARRAY);
        scenes.Add("generated_road");
        scenes.Add("warehouse");
        scenes.Add("sparkfun_avc");

        m.json = new JSONObject(JSONObject.Type.OBJECT);
        m.json.AddField("scene_names", scenes);

        QueueMessage(m);
    }


    void OnLoadScene(SocketIOEvent obj)
    {
        JSONObject jsonObject = obj.data;

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
    }

    void onQuitApp(SocketIOEvent obj)
    {
        loader.QuitApplication();
    }

}
