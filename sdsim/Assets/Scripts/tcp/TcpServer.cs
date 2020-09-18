using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;

namespace tk
{   
    public class TcpServer : MonoBehaviour
    {
        // register for OnClientConnected to handle the game specific creation of TcpClients with a MonoBehavior
        public delegate TcpClient OnClientConnected();
        public OnClientConnected onClientConntedCB;

        // register for OnClientDisconnected to have an opportunity to handle dropped clients
        public delegate void OnClientDisconnected(TcpClient client);
        public OnClientDisconnected onClientDisconntedCB;

        // Server listener socket
        Socket listener = null;

        // Accept thread
        Thread thread = null;

        // Thread signal.  
        public ManualResetEvent allDone = new ManualResetEvent(false);

        // All connected clients
        List<TcpClient> clients = new List<TcpClient>();

        // All new clients that need a onClientConntedCB callback
        List<Socket> new_clients = new List<Socket>();

        // Lock object to protect access to new_clients
        readonly object _locker = new object();

        // Verbose messages
        public bool debug = false;

        public List<string> blocked = new List<string>();


        // Call the Run method to start the server. The ip address is typically 127.0.0.1 to accept only local connections.
        // Or 0.0.0.0 to bind to all incoming connections for this NIC.
        public void Run(string ip, int port)
        {
            Bind(ip, port);

            // Poll for new connections in the ListenLoop
            thread = new Thread(ListenLoop);
            thread.Start();
        }

        // Stop the server. Will disconnect all clients and shutdown networking.
        public void Stop()
        {
            foreach( TcpClient client in clients)
            {
                client.ReleaseServer();
                client.Disconnect();
            }

            clients.Clear();

            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }

            if(listener != null)
            {
                listener.Close();
                listener = null;
                Debug.Log("Server stopped.");
            }
        }

        // When GameObject is deleted..
        void OnDestroy()
        {
            Stop();
        }

        // SendData will broadcast send to all peers
        public void SendData(byte[] data, TcpClient skip = null)
        {
            foreach (TcpClient client in clients)
            {
                if (client == skip)
                    continue;

                client.SendData(data);

                if(debug)
                {
                    Debug.Log("sent: " + System.Text.Encoding.Default.GetString(data));
                }
            }
        }

        public void Block(string ip)
        {
            blocked.Add(ip);
        }

        // Remove reference to TcpClient
        public void RemoveClient(TcpClient client)
        {
            clients.Remove(client);
        }

        public List<TcpClient> GetClients()
        {
            return clients;
        }

        public void Update()
        {
            lock (_locker)
            {
                // Because we might be creating GameObjects we need this callback to happen in the main
                // thread context. So we queue new sockets and then create their TcpClients from here.
                if (new_clients.Count > 0)
                {
                    if (onClientConntedCB != null)
                    {
                        foreach (Socket handler in new_clients)
                        {
                            TcpClient client = onClientConntedCB.Invoke();

                            if (client != null)
                            {
                                if(client.OnServerAccept(handler, this))
                                {
                                    clients.Add(client);
                                    client.SetDebug(debug);
                                    client.ClientFinishedConnect();
                                }
                            }
                        }
                    }

                    new_clients.Clear();
                }
            }

            //Poll for dropped connection.
            foreach(TcpClient client in clients)
            {
                if(client.IsDropped())
                {
                    onClientDisconntedCB.Invoke(client);
                }
            }
        }

        // Start listening for connections
        private void Bind(string ip, int port)
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            // Create a TCP/IP socket.  
            listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            //Bind to address
            listener.Bind(localEndPoint);
            listener.Listen(100);

            Debug.Log("Server Listening on: " + ip + ":" + port.ToString());
        }

        // Thread loop to wait for new connections
        private void ListenLoop()
        {
            while(true)
            {
                // Set the event to non-signaled state.  
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.  
                Debug.Log("Waiting for a connection...");
                listener.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    listener);

                // Wait until a connection is made before continuing.  
                allDone.WaitOne();
            }
        }

        // Callback to handle new connections
        private void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;

            try
            {
                Socket handler = listener.EndAccept(ar);

                Debug.Log("client connected.");

                lock (_locker)
                {
                   string ip = ((IPEndPoint)(handler.RemoteEndPoint)).Address.ToString();

                   foreach(string blockedip in blocked)
                   {
                       if(blockedip == ip)
                       {
                           Debug.LogWarning(ip + " is a blocked ip address.");
                           return;
                       }
                   }

                    // Add clients to this new_clients list.
                    // They will get a onClientConntedCB later on in the Update method.
                    new_clients.Add(handler);
                }
            }
            catch(SocketException e)
            {
                Debug.LogError(e.ToString());
            }
            
        }
    }

}