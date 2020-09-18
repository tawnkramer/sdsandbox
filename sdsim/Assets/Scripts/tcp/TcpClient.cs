using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;

namespace tk
{
    public class TcpClient : MonoBehaviour
    {
        public bool debug = false;

        private Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private byte[] _recieveBuffer = new byte[8142];
        private TcpServer _server = null;

        public delegate void OnDataRecv(byte[] data);

        public OnDataRecv onDataRecvCB;

        public delegate void OnConnected();

        public OnConnected onConnectedCB;

        // Flag to let us know a connection has dropped.
        private bool dropped = false;
        public float time_check_dropped = 0.0f;
        public float time_check_dropped_freq = 3.0f;

        /// <summary>
        /// Connect will establish a new TCP socket connection to a remote ip, port. This method is the first method
        /// called to start using this object.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Connect(string ip, int port)
        {
            if (_clientSocket.Connected)
                return false;

            try
            {
                IPAddress address = IPAddress.Parse(ip);
                _clientSocket.Connect(new IPEndPoint(address, port));
            }
            catch (SocketException ex)
            {
                Debug.Log(ex.Message);
                return false;
            }

            dropped = false;
            _clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);

            return true;
        }

        /// <summary>
        /// OnServerAccept is an alternate form of client initialization that occurs when a server has accepted a client
        /// connection already and passes that socket in clientSock in a connected state. This also then passes a pointer
        /// to the server which can be used by the clients to broadcast messages to all the peers.
        /// </summary>
        /// <param name="clientSock"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public bool OnServerAccept(Socket clientSock, TcpServer server)
        {
            if (!clientSock.Connected)
                return false;

            _clientSocket = clientSock;
            _server = server;
            dropped = false;

            _clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);

            return true;
        }

        public void ClientFinishedConnect()
        {
            if(onConnectedCB != null)
                onConnectedCB.Invoke();
        }

        public void ReleaseServer()
        {
            _server = null;
        }

        public void Disconnect()
        {
            try
            {
                if (_clientSocket.Connected)
                {
                    _clientSocket.Shutdown(SocketShutdown.Both);

                    if (!IsDropped())
                    {
                        _clientSocket.Disconnect(true);
                    }
                }
            }
            catch(SocketException e)
            {
                Debug.Log(e.ToString());
            }
            finally
            {
                _clientSocket.Close();
            }

            if (_server != null)
                _server.RemoveClient(this);
        }

        void OnDestroy()
        {
            Disconnect();
        }

        public bool IsDropped()
        {
            return dropped;
        }

        public void Drop()
        {
            dropped = true;
        }

        public string GetIPAddress()
        {
            string ip = ((IPEndPoint)(_clientSocket.RemoteEndPoint)).Address.ToString();
            return ip;
        }

        public void Update()
        {
            // Update our drop detection...
            if(_clientSocket != null && !IsDropped())
            {
                time_check_dropped += Time.deltaTime;

                if(!_clientSocket.Connected)
                {
                    dropped = true;
                }
                else if(time_check_dropped > time_check_dropped_freq)
                {
                    time_check_dropped = 0.0f;

                    try
                    {
                        // this is the minimal form of message for a JsonTCPClient
                        string msg = "{\"msg_type\" : \"ping\"}\n";
                        System.Text.Encoding encoding = System.Text.Encoding.Default;
                        _clientSocket.Send(encoding.GetBytes(msg));
                    }
                    catch(SocketException e)
                    {
                        Debug.LogWarning("connection dropped.");
                        dropped = true;
                    }
                }
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            //Check how much bytes are recieved and call EndRecieve to finalize handshake
            int recieved = 0;

            try
            {
                recieved = _clientSocket.EndReceive(AR);
            }
            catch(SocketException e)
            {
                recieved = 0;
                dropped = true;
                Debug.LogWarning("Exception on recv. Connection dropped.");
            }
            

            if (recieved <= 0)
                return;

            //Copy the recieved data into new buffer , to avoid null bytes
            byte[] recData = new byte[recieved];
            Buffer.BlockCopy(_recieveBuffer, 0, recData, 0, recieved);

            if (debug)
            {
                Debug.Log("recv:" + System.Text.Encoding.Default.GetString(recData));
            }

            //Process data here the way you want , all your bytes will be stored in recData
            if (onDataRecvCB != null)
                onDataRecvCB.Invoke(recData);

            // Reset our drop connection test timer, since we just got data.
            time_check_dropped = 0.0f;

            //Start receiving again
            _clientSocket.BeginReceive(_recieveBuffer, 0, _recieveBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
        }

        public bool SendData(byte[] data)
        {
            if (!_clientSocket.Connected)
                return false;

            SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
            socketAsyncData.SetBuffer(data, 0, data.Length);
            _clientSocket.SendAsync(socketAsyncData);

            if (debug)
            {
                Debug.Log("sent:" + System.Text.Encoding.Default.GetString(data));
            }

            return true;
        }

        public bool SendDataToPeers(byte[] data)
        {
            if (!_clientSocket.Connected || _server == null)
                return false;

            _server.SendData(data, this);
            return true;
        }

        public void SetDebug(bool _debug)
        {
            debug = _debug;
        }

    }

} //end namepace tk
