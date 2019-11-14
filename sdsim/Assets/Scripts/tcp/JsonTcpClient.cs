using System.Collections.Generic;
using UnityEngine;
using System;

namespace tk
{

    [Serializable]
    public class NetPacket
    {
        public NetPacket(string m, string data)
        {
            msg = m;
            payload = data;
        }

        public string msg;
        public string payload;
    }

    
    //Wrap a tk.TcpClient and dispatcher to handle network events over a tcp connection.
    //We create a NetPacket header to wrap all sends and recv. Should be pretty portable
    //over languages.
    [RequireComponent(typeof(tk.TcpClient))]
    public class JsonTcpClient : MonoBehaviour {

        private tk.TcpClient client;

        public tk.Dispatcher dispatcher;

        public bool dispatchInMainThread = false;

        private List<string> recv_packets;

        readonly object _locker = new object();

        void Awake()
        {
            recv_packets = new List<string>();
            dispatcher = new tk.Dispatcher();
            dispatcher.Init();
            client = GetComponent<tk.TcpClient>();
            
            Initcallbacks();
        }

        void Initcallbacks()
        {
            client.onDataRecvCB += new TcpClient.OnDataRecv(OnDataRecv);
        }

        public void Disconnect()
        {
            client.Disconnect();
        }

        public void SendMsg(JSONObject msg)
        {
            string packet = msg.ToString() + "\n";

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(packet);

            client.SendData( bytes );
        }

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

        void Dispatch()
        {
            lock(_locker)
            {
                foreach(string str in recv_packets)
                {
                    try
                    {
                        JSONObject j = new JSONObject(str);

                        string msg_type = j["msg_type"].str;

                        dispatcher.Dipatch(msg_type, j);

                    }
                    catch(Exception e)
                    {
                        Debug.Log(e.ToString());
                    }
                }

                recv_packets.Clear();
            }

        }

        void Update()
        {
            if (dispatchInMainThread)
            {
                Dispatch();
            }
        }
    }
}