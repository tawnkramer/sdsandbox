using System.Collections.Generic;
using UnityEngine;
using System;

namespace tk
{
    // Wrap a tk.TcpClient and dispatcher to handle network events over a tcp connection.
    // Use Json for message contents. Assumes no newline characters in the json string contents,
    // and end each packet with a newline for separation.
    [RequireComponent(typeof(tk.TcpClient))]
    public class JsonTcpClient : MonoBehaviour {

        // Our reference to the required 'base' component
        private tk.TcpClient client;

        // This allows other objects to register for incoming json messages
        public tk.Dispatcher dispatcher;

        // Some messages need to be handled in the main thread. Unity object creation, etc..
        public bool dispatchInMainThread = false;

        // A list of raw json strings received from network and waiting to dispatched locally.
        private List<string> recv_packets;

        // Make sure to protect our recv_packets from race conditions
        readonly object _locker = new object();

        //required for stream parsing where client may recv multiple messages at once.
        const string packetTerminationChar = "\n";


        void Awake()
        {
            recv_packets = new List<string>();
            dispatcher = new tk.Dispatcher();
            dispatcher.Init();
            client = GetComponent<tk.TcpClient>();
            
            Initcallbacks();
        }

        // Interact with our base TcpClient to handle incoming data
        void Initcallbacks()
        {
            client.onDataRecvCB += new TcpClient.OnDataRecv(OnDataRecv);
            client.onConnectedCB += new TcpClient.OnConnected(OnConnected);
        }

        public void OnConnected()
        {
            recv_packets.Add("{\"msg_type\" : \"connected\"}");
        }

        // Close our socket connection
        public void Disconnect()
        {
            client.Disconnect();
        }


        // Send a json packet over our TCP socket asynchronously.
        public void SendMsg(JSONObject msg)
        {
            string packet = msg.ToString() + packetTerminationChar;

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(packet);

            client.SendData( bytes );
        }


        // Our callback from the TCPClient to get data.
        void OnDataRecv(byte[] bytes)
        {
            string str = System.Text.Encoding.UTF8.GetString(bytes);
            
            lock(_locker)
            {
                recv_packets.Add(str);
            }

            if(!dispatchInMainThread)
            {
                Dispatch();
            }
        }


        // Send each queued json packet to the recipient which registered
        // with our dispatcher.
        void Dispatch()
        {
            lock(_locker)
            {
                foreach(string str in recv_packets)
                {
                    // We have not had problems yet with message separation, but
                    // it could be added. Here we errors in case it becomes an issue.
                    try
                    {
                        JSONObject j = new JSONObject(str);

                        string msg_type = j["msg_type"].str;

                        Debug.Log("Got: " + msg_type);

                        dispatcher.Dipatch(msg_type, j);

                    }
                    catch(Exception e)
                    {
                        Debug.LogError(e.ToString());
                    }
                }

                recv_packets.Clear();
            }
        }


        // Optionally poll our dispatch queue in the main thread context
        void Update()
        {
            if (dispatchInMainThread)
            {
                Dispatch();
            }
        }
    }
}