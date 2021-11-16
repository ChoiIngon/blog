using System;
using System.Collections;
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
        public ConnectionState state = ConnectionState.Close;

        private Timeout timeout = new Timeout();
        private System.Timers.Timer timer;
        private int timeoutInterval = 5000; // 비동기 connect 실패 시간. 5초

        private byte[] receiveBytes = new byte[MAX_BUFFER_SIZE];
        private Buffer receiveBuffer = new Buffer();


        private List<Packet> sendQueue = new List<Packet>();
        private int sendQueueIndex;
        private UInt32 sendPacketSeq = 0;
        private Connector connector;
        public IEnumerator enumerator;

        public Dictionary<uint, Async.AsyncReceive> async_receives;
        public Session(UInt32 sessionKey)
        {
            this.session_key = sessionKey;
            this.async_receives = new Dictionary<uint, Async.AsyncReceive>();
        }

        public void AsyncConnect(string host, int port, int timeout_sec = 5)
        {
            this.connector = new Connector(this);
            this.connector.AsyncConnect(host, port, timeout_sec);
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

                receiveBuffer.Remove(packet.Length);
                receiveBuffer = new Buffer(receiveBuffer);

                OnPacket(packet);
            }

            AsyncReceive();
        }
        #endregion

        public void Error(System.Exception e)
        {
            ErrorEvent evt = new ErrorEvent(this);
            evt.exception = e;
            lock (this)
            {
                EventLoop.EnqueuEvent(evt);
            }
        }

        public void Disconnect()
        {
            if (ConnectionState.Close == this.state)
            {
                return;
            }

            if (null == socket)
            {
                return;
            }

            Debug.Log($"[Session.Disconnect] session state:{state.ToString()}");
            try
            {
                state = ConnectionState.Close;
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
                EventLoop.EnqueuEvent(evt);
            }
            catch (SocketException e)
            {
                Debug.Log("[Session.Callback_Disconnect] session_state:" + state.ToString() + ", exception:" + e.ToString());
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
                Debug.Log("[Session.Callback_Disconnect] session_state:" + state.ToString() + ", exception:" + e.ToString());
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

        protected virtual void OnPacket(Packet packet)
        {
            throw new System.NotImplementedException("Session.OnPacket is not implemented");
        }
    }
}
