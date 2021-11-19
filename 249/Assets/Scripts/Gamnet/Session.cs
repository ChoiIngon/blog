using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

namespace Gamnet
{
    public class Session
    {
        public enum State
        {
            Close,          // 연결이 되어 있지 않은 상태
            OnConnecting,   // 비동기 연결 시도 중. 아직 완료 안됨
            Connected,      // 연결 완료 상태
            Pause,          // 모바일에서 앱이 백그라운드로 넘어가 일시적으로 접속이 끊긴 상태
            Handover        // 모바일에서 이동 등으로 인해 접속이 잠시 끊긴 상태
        }

        public readonly UInt32 session_key;
        public const int MAX_BUFFER_SIZE = 1024;

        public Socket socket;
        public State state = State.Close;

        public IEnumerator current_coroutine;
        public Dictionary<uint, Async.AsyncReceive> async_receives;

        //private Timeout timeout = new Timeout();
        //private System.Timers.Timer timer;
        //private int timeoutInterval = 5000; // 비동기 connect 실패 시간. 5초

        private byte[] receiveBytes = new byte[MAX_BUFFER_SIZE];
        private Buffer receiveBuffer = new Buffer();

        private List<Packet> send_queue = new List<Packet>();
        private int send_queue_index;
        private UInt32 send_seq = 0;
        private UInt32 recv_seq = 0;

        public Session(UInt32 sessionKey)
        {
            this.session_key = sessionKey;
            this.async_receives = new Dictionary<uint, Async.AsyncReceive>();
        }

        #region AsyncReceive
        public void AsyncReceive()
        {
            if (false == socket.Connected)
            {
                return;
            }
            try
            {
                socket.BeginReceive(receiveBytes, 0, MAX_BUFFER_SIZE, 0, new AsyncCallback(AsyncReceiveCallback), null);
            }
            catch (SocketException e)
            {
                Error(e);
                Close();
            }
        }

        private void AsyncReceiveCallback(IAsyncResult result)
        {
            try
            {
                Int32 recvBytesSize = socket.EndReceive(result);
                if (0 == recvBytesSize)
                {
                    Close();
                    return;
                }
                receiveBuffer.Append(this.receiveBytes, 0, recvBytesSize);
            }
            catch (System.ObjectDisposedException)
            {
            }
            catch (SocketException e)
            {
                Error(e);
                Close();
                return;
            }

            while (Packet.HEADER_SIZE <= receiveBuffer.Size())
            {
                Packet packet = new Packet(receiveBuffer);
                if (packet.Length > Gamnet.Buffer.MAX_BUFFER_SIZE)
                {
                    Error(new System.OverflowException("The packet length is greater than the buffer max length."));
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

                ReceiveEvent evt = new ReceiveEvent(this, packet);
                EventLoop.EnqueuEvent(evt);
            }

            AsyncReceive();
        }
        #endregion

        public void Error(System.Exception e)
        {
            ErrorEvent evt = new ErrorEvent(this, e);
            evt.exception = e;
            EventLoop.EnqueuEvent(evt);
        }

        public void Close()
        {
            if (State.Connected != this.state)
            {
                return;
            }

            if (null == socket)
            {
                return;
            }

            try
            {
                state = State.Close;
                //timer.Stop();

                socket.BeginDisconnect(false, new AsyncCallback(CloseCallback), socket);
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

        private void CloseCallback(IAsyncResult result)
        {
            try
            {
                socket.EndDisconnect(result);
                socket.Close();

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
            packet.Seq = ++send_seq;

            lock (this)
            {
                send_queue.Add(packet);

                if (false == socket.Connected)
                {
                    return;
                }

                if (1 != send_queue.Count - send_queue_index)
                {
                    return;
                }

                Packet packetToBeSent = send_queue[send_queue_index];
                Buffer bufferToBeSend = packetToBeSent.buffer;
                socket.BeginSend(bufferToBeSend.ToByteArray(), 0, packetToBeSent.Length, 0, new AsyncCallback(AsyncSendCallback), null);
            }
        }

        private void AsyncSendCallback(IAsyncResult result)
        {
            try
            {
                int writtenBytes = socket.EndSend(result);

                lock (this)
                {
                    Packet packet = send_queue[send_queue_index];
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
                        send_queue_index++;
                    }
                    else
                    {
                        send_queue.RemoveAt(send_queue_index);
                    }

                    if (send_queue_index < send_queue.Count)
                    {
                        Packet packetToBeSent = send_queue[send_queue_index];
                        Buffer bufferToBeSend = packetToBeSent.buffer;
                        socket.BeginSend(bufferToBeSend.ToByteArray(), 0, bufferToBeSend.Size(), 0, new AsyncCallback(AsyncSendCallback), null);
                    }
                }
            }
            catch (SocketException e)
            {
                Debug.Log("[Session.Callback_Disconnect] session_state:" + state.ToString() + ", exception:" + e.ToString());
                Error(e);
                Close();
            }
        }

        #region SessionEvent
        public abstract class SessionEvent
        {
            protected Session session;
            public SessionEvent(Session session)
            {
                this.session = session;
            }
            public abstract void OnEvent();
        };

        public class AcceptEvent : SessionEvent
        {
            public AcceptEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                session.AsyncReceive();
                session.OnAccept();
            }
        }

        protected virtual void OnAccept()
        {
            throw new System.NotImplementedException("Session.OnAccept is not implemented");
        }

        public class ConnectEvent : SessionEvent
        {
            public ConnectEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                session.AsyncReceive();
                session.OnConnect();
            }
        }

        protected virtual void OnConnect()
        {
            throw new System.NotImplementedException("Session.OnConnect is not implemented");
        }

        public class ReconnectEvent : SessionEvent
        {
            public ReconnectEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                session.AsyncReceive();
                session.OnReconnect();
            }
        }

        protected virtual void OnReconnect()
        {
            throw new System.NotImplementedException("Session.OnReconnect is not implemented");
        }

        public class PauseEvent : SessionEvent
        {
            public PauseEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                session.OnPause();
            }
        }

        protected virtual void OnPause()
        {
            throw new System.NotImplementedException("Session.OnPause is not implemented");
        }

        public class ResumeEvent : SessionEvent
        {
            public ResumeEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                session.AsyncReceive();
                session.OnResume();
            }
        }

        protected virtual void OnResume()
        {
            throw new System.NotImplementedException("Session.OnResume is not implemented");
        }

        public class CloseEvent : SessionEvent
        {
            public CloseEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                foreach (var pair in session.async_receives)
                {
                    Async.AsyncReceive asyncReceive = pair.Value;
                    asyncReceive.Cancel();
                }

                session.async_receives.Clear();
                session.current_coroutine = null;
                session.OnClose();
            }
        }

        protected virtual void OnClose()
        {
            throw new System.NotImplementedException("Session.OnClose is not implemented");
        }

        public class ErrorEvent : SessionEvent
        {
            public ErrorEvent(Session session, System.Exception exception) : base(session)
            {
                this.exception = exception;
            }
            public System.Exception exception;
            public override void OnEvent()
            {
                session.OnError(exception);
            }
        }

        protected virtual void OnError(System.Exception e)
        {
            throw new System.NotImplementedException("Session.OnError is not implemented");
        }

        public class ReceiveEvent : SessionEvent
        {
            private Packet packet;
            public ReceiveEvent(Session session, Packet packet) : base(session)
            {
                this.packet = packet;
            }

            public override void OnEvent()
            {
                session.OnReceive(this.packet);
            }
        }

        protected virtual void OnReceive(Packet packet)
        {
            throw new System.NotImplementedException("Session.OnReceive is not implemented");
        }
        #endregion
    }
}
