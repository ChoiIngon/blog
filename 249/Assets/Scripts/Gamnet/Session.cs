using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Gamnet
{
    public class Session
    {
        public readonly UInt32 session_key;
        public enum ConnectionState
        {
            Close,          // 연결이 되어 있지 않은 상태
            OnConnecting,   // 비동기 연결 시도 중. 아직 완료 안됨
            Connected,      // 연결 완료 상태
            Pause,          // 모바일에서 앱이 백그라운드로 넘어가 일시적으로 접속이 끊긴 상태
            Handover        // 모바일에서 이동 등으로 인해 접속이 잠시 끊긴 상태
        }

        public const int MAX_BUFFER_SIZE = 1024;

        public Socket socket;
        private IPEndPoint endPoint; // 자동 재접속을 위해
        private ConnectionState connectionState = ConnectionState.Close;

        private Timeout timeout = new Timeout();
        private System.Timers.Timer timer;
        private int timeoutInterval = 5000; // 비동기 connect 실패 시간. 5초

        private byte[] receiveBytes = new byte[MAX_BUFFER_SIZE];
        private Buffer receiveBuffer = new Buffer();


        private List<Packet> sendQueue = new List<Packet>();
        private int sendQueueIndex;
        private UInt32 sendPacketSeq = 0;

        public ConnectionState State
        {
            get { return connectionState; }
        }

        public Session(UInt32 sessionKey)
        {
            this.session_key = sessionKey;
        }

        #region Receive

        public void AsyncReceive()
        {
            try
            {
                socket.BeginReceive(receiveBytes, 0, MAX_BUFFER_SIZE, 0, new AsyncCallback(AsyncReceiveCallback), null);
            }
            catch (SocketException e)
            {
                Debug.LogError($"[Session.Receive] exception:{e.ToString()}");
                Error(e);
                Disconnect();
            }
        }
        void AsyncReceiveCallback(IAsyncResult result)
        {
            try
            {
                Int32 recvBytesSize = socket.EndReceive(result);
                if (0 == recvBytesSize)
                {
                    Disconnect();
                    return;
                }
                receiveBuffer.Append(this.receiveBytes, 0, recvBytesSize);
            }
            catch (SocketException e)
            {
                Debug.LogError($"[Session.AsyncReceiveCallback] exception:{e.ToString()}");
                Error(e);
                Disconnect();
                return;
            }

            while (Packet.HEADER_SIZE <= receiveBuffer.Size())
            {
                Packet packet = new Packet(receiveBuffer);
                if (packet.Length > Gamnet.Buffer.MAX_BUFFER_SIZE)
                {
                    Debug.LogError("[Session.OnReceive] The packet length is greater than the buffer max length.");
                    Error(new System.Exception("The packet length is greater than the buffer max length."));
                    return;
                }

                if (packet.Length > receiveBuffer.Size())
                {
                    // not enough
                    AsyncReceive();
                    return;
                }

                timeout.UnsetTimeout(packet.Seq);
                receiveBuffer.Remove(packet.Length);
                receiveBuffer = new Buffer(receiveBuffer);

                ReceiveEvent evt = new ReceiveEvent(this, packet);
                SessionEventQueue.Instance.EnqueuEvent(evt);
            }

            AsyncReceive();
        }
        #endregion

        public void AsyncConnect(string host, int port, int timeout_sec = 5)
        {
            IPAddress ipAddress = null;
            try
            {
                ipAddress = IPAddress.Parse(host);
            }
            catch (System.FormatException)
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(host);
                if (hostEntry.AddressList.Length > 0)
                {
                    ipAddress = hostEntry.AddressList[0];
                }
            }

            connectionState = ConnectionState.OnConnecting;
            endPoint = new IPEndPoint(ipAddress, port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.BeginConnect(endPoint, new AsyncCallback(AsyncConnectCallback), null);
        }

        private void AsyncConnectCallback(IAsyncResult result)
        {
            try
            {
                socket.EndConnect(result);
                socket.ReceiveBufferSize = Gamnet.Session.MAX_BUFFER_SIZE;
                socket.SendBufferSize = Gamnet.Session.MAX_BUFFER_SIZE;
                connectionState = ConnectionState.Connected;
                //_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 10000);
                //_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10000);

                ConnectEvent evt = new ConnectEvent(this);
                SessionEventQueue.Instance.EnqueuEvent(evt);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Session.Callback_Connect] exception:" + e.ToString());
            }
        }

        public void Error(System.Exception e)
        {
            ErrorEvent evt = new ErrorEvent(this);
            evt.exception = e;
            lock (this)
            {
                SessionEventQueue.Instance.EnqueuEvent(evt);
            }
        }

        public void Disconnect()
        {
            if (ConnectionState.Close == this.State)
            {
                return;
            }

            if (null == socket)
            {
                return;
            }

            Debug.Log($"[Session.Disconnect] session state:{State.ToString()}");
            try
            {
                connectionState = ConnectionState.Close;
                timer.Stop();

                socket.BeginDisconnect(false, new AsyncCallback(DisconnectCallback), socket);
            }
            catch (SocketException e)
            {
                Debug.LogError("[Session.Disconnect] exception:" + e.ToString());
                Error(e);
            }
            catch (ObjectDisposedException e)
            {
                Debug.LogError("[Session.Disconnect] exception:" + e.ToString());
            }
        }

        private void DisconnectCallback(IAsyncResult result)
        {
            try
            {
                socket.EndDisconnect(result);

                CloseEvent evt = new CloseEvent(this);
                SessionEventQueue.Instance.EnqueuEvent(evt);
            }
            catch (SocketException e)
            {
                Debug.Log("[Session.Callback_Disconnect] session_state:" + State.ToString() + ", exception:" + e.ToString());
            }
        }

        public void AsyncSend(Packet packet)
        {
            /*
            if (null == endPoint)
            {
                Debug.LogError("[Session.Reconnect] invalid destination address");
                return;
            }
            */

            packet.Seq = ++sendPacketSeq;
            sendQueue.Add(packet);
            if (false == socket.Connected)
            {
                return;
            }

            if (1 != sendQueue.Count - sendQueueIndex)
            {
                return;
            }

            Packet packetToBeSent = sendQueue[sendQueueIndex];
            Buffer bufferToBeSend = packetToBeSent.buffer;
            socket.BeginSend(bufferToBeSend.ToByteArray(), 0, packetToBeSent.Length, 0, new AsyncCallback(AsyncSendCallback), null);
        }
        public void AsyncSend<T>(T msg) where T : Message
        {
            BinaryFormatter bf = new BinaryFormatter();
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            bf.Serialize(ms, msg);
            Packet packet = new Packet();
            packet.Id = msg.Id;
            packet.Write(ms.GetBuffer());
            AsyncSend(packet);
        }

        private void AsyncSendCallback(IAsyncResult result)
        {
            try
            {
                int writtenBytes = socket.EndSend(result);

                Packet packet = sendQueue[sendQueueIndex];
                if (false == packet.buffer.Remove(writtenBytes))
                {
                    throw new System.OverflowException();
                }

                if (0 < packet.buffer.Size())
                {
                    socket.BeginSend(packet.buffer.ToByteArray(), packet.buffer.read_index, packet.buffer.Size(), 0, new AsyncCallback(AsyncSendCallback), null);
                    return;
                }

                if (true == packet.IsReliable)
                {
                    sendQueueIndex++;
                }
                else
                {
                    sendQueue.RemoveAt(sendQueueIndex);
                }

                if (sendQueueIndex < sendQueue.Count)
                {
                    Packet packetToBeSent = sendQueue[sendQueueIndex];
                    Buffer bufferToBeSend = packetToBeSent.buffer;
                    socket.BeginSend(bufferToBeSend.ToByteArray(), 0, bufferToBeSend.Size(), 0, new AsyncCallback(AsyncSendCallback), null);
                }
            }
            catch (SocketException e)
            {
                Debug.Log("[Session.Callback_Disconnect] session_state:" + State.ToString() + ", exception:" + e.ToString());
                Error(e);
                Disconnect();
            }
        }
        public virtual void OnReceive(Packet packet)
        {
        }
        public virtual void OnAccept()
        {
            throw new System.NotImplementedException("Session.OnAccept is not implemented");
        }

        public virtual void OnConnect()
        {
            throw new System.NotImplementedException("Session.OnConnect is not implemented");
        }

        public virtual void OnReconnect()
        {
            throw new System.NotImplementedException("Session.OnReconnect is not implemented");
        }

        public virtual void OnPause()
        {
            throw new System.NotImplementedException("Session.OnPause is not implemented");
        }

        public virtual void OnResume()
        {
            throw new System.NotImplementedException("Session.OnResume is not implemented");
        }
        public virtual void OnClose()
        {
            throw new System.NotImplementedException("Session.OnClose is not implemented");
        }
        public virtual void OnError(System.Exception e)
        {
            throw new System.NotImplementedException("Session.OnError is not implemented");
        }
    }
}
