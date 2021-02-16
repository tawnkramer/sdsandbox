using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tk;
using System.Net;
using System.Net.Sockets;
using System;

[RequireComponent(typeof(tk.TcpServer))]
public class SandboxServer : MonoBehaviour
{
    public string host;
    public int port;

    tk.TcpServer _server = null;

    public GameObject clientTemplateObj = null;
    public Transform spawn_pt;
    public bool spawnCarswClients = true;
    public bool privateAPI = false;

    public void CheckCommandLineConnectArgs()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--host")
            {
                host = args[i + 1];
            }
            else if (args[i] == "--port")
            {
                port = int.Parse(args[i + 1]);
            }
            else
            {
                if (privateAPI)
                {
                    host = GlobalState.host;
                    port = GlobalState.portPrivateAPI;
                }
                else
                {
                    host = GlobalState.host;
                    port = GlobalState.port;
                }
            }
        }
    }

    private void Awake()
    {
        _server = GetComponent<tk.TcpServer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        CheckCommandLineConnectArgs();

        Debug.Log("SDSandbox Server starting.");
        _server.onClientConntedCB += new tk.TcpServer.OnClientConnected(OnClientConnected);
        _server.onClientDisconntedCB += new tk.TcpServer.OnClientDisconnected(OnClientDisconnected);

        _server.Run(host, port);
    }

    // It's our responsibility to create a GameObject with a TcpClient
    // and return it to the server.
    public tk.TcpClient OnClientConnected()
    {
        if (clientTemplateObj == null)
        {
            Debug.LogError("client template object was null.");
            return null;
        }

        if (_server.debug)
            Debug.Log("creating client obj");

        GameObject go = GameObject.Instantiate(clientTemplateObj) as GameObject;

        go.transform.parent = this.transform;

        if (spawn_pt != null)
            go.transform.position = spawn_pt.position + UnityEngine.Random.insideUnitSphere * 2;

        tk.TcpClient client = go.GetComponent<tk.TcpClient>();

        InitClient(client);

        return client;
    }

    private void InitClient(tk.TcpClient client)
    {
        if (privateAPI) // private API client server
        {
            PrivateAPI privateAPIHandler = GameObject.FindObjectOfType<PrivateAPI>();
            if (privateAPIHandler != null)
            {
                if (_server.debug)
                    Debug.Log("init private API handler.");

                privateAPIHandler.Init(client.gameObject.GetComponent<tk.JsonTcpClient>());
            }
        }

        else // normal client server
        {
            if (spawnCarswClients) // we are on in a track scene
            {
                CarSpawner spawner = GameObject.FindObjectOfType<CarSpawner>();
                if (spawner != null)
                {
                    if (_server.debug)
                        Debug.Log("spawning car.");

                    spawner.Spawn(client.gameObject.GetComponent<tk.JsonTcpClient>());
                }
            }
            else //we are in the menu
            {

                tk.TcpMenuHandler handler = GameObject.FindObjectOfType<TcpMenuHandler>();
                if (handler != null)
                {
                    if (_server.debug)
                        Debug.Log("init menu handler.");

                    handler.Init(client.gameObject.GetComponent<tk.JsonTcpClient>());
                }
            }
        }
    }


    public void OnSceneLoaded(bool bFrontEnd)
    {
        spawnCarswClients = !bFrontEnd;

        List<tk.TcpClient> clients = _server.GetClients();

        foreach (tk.TcpClient client in clients)
        {
            if (_server.debug)
                Debug.Log("init network client.");

            InitClient(client);
        }

        if (GlobalState.bCreateCarWithoutNetworkClient && !bFrontEnd && clients.Count == 0)
        {
            CarSpawner spawner = GameObject.FindObjectOfType<CarSpawner>();

            if (spawner)
            {
                if (_server.debug)
                    Debug.Log("spawning car.");

                spawner.Spawn(null);
            }
        }
    }

    public void OnClientDisconnected(tk.TcpClient client)
    {
        if (privateAPI)
        {
        }
        else
        {
            CarSpawner spawner = GameObject.FindObjectOfType<CarSpawner>();
            if (spawner)
            {
                spawner.RemoveCar(client.gameObject.GetComponent<tk.JsonTcpClient>());
            }
        }
        GameObject.Destroy(client.gameObject);
    }
}
