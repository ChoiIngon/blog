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

        // 요 부분을 서버와 클라이언트 분리해서, 서버에서는 소켓 closse가발생하면 바로 close하지 않고 Pause 이벤트 날리고,
        // timeout 되거나, 클라이언트로 부터 명시적 close가 날아 오면 세션을 destroy하는 걸로 분리
        private void AsyncReceiveCallback(IAsyncResult result)
        {
            try
            {
                Int32 recvBytesSize = socket.EndReceive(result);
                if (0 == recvBytesSize)
                {
                    SessionEvent evt = new PassiveCloseEvent(this);
                    Session.EventLoop.EnqueuEvent(evt);
                    return;
                }
                receiveBuffer.Append(this.receiveBytes, 0, recvBytesSize);
            }
            catch (ObjectDisposedException e)
            {
                Error(e);
                return;
            }
            catch (SocketException e)
            {
                Error(e);
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

        // 요 부분도 서버 클라 분리해서 서버에서는 close하면 바로 socket close하고 destroy 까지
        // 클라이언트는 서버에게 명시적으로 close 메시지를 보내는 걸로 분리
        // abstract Session 클래스 같은걸 하나 만들어야 겠다.
        // 단순 tcp연결 말고도 handover 과정도 처리 할 수 있도록..ㅇㅇ
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
#if UNITY_EDITOR || USE_DEBUGGING
            public readonly string CallStack;
#endif
            public SessionEvent(Session session)
            {
                this.session = session;
#if UNITY_EDITOR || USE_DEBUGGING
                CallStack = StackTraceUtility.ExtractStackTrace();
#endif
            }

            public abstract void OnEvent();
        };

        public class CreateEvent : SessionEvent
        {
            public CreateEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                session.OnCreate();
            }
        }

        protected virtual void OnCreate()
        {
            throw new System.NotImplementedException("Session.OnCreate is not implemented");
        }

        public class DestoryEvent : SessionEvent
        {
            public DestoryEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                session.OnDestory();
            }
        }

        protected virtual void OnDestory()
        {
            throw new System.NotImplementedException("Session.OnDestory is not implemented");
        }

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
                Log.Write(Log.LogLevel.ERR, CallStack);
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

        public class PassiveCloseEvent : SessionEvent
        {
            public PassiveCloseEvent(Session session) : base(session)
            {
            }

            public override void OnEvent()
            {
                session.OnPassiveClose();
            }
        }
        protected virtual void OnPassiveClose()
        {
        }

        protected virtual void OnReliableMode(bool flag)
        {
            throw new System.NotImplementedException("Session.OnReliableMode is not implemented");
        }
#endregion

        public class EventLoop
        {
            private ConcurrentQueue<Session.SessionEvent> eventQueue = new ConcurrentQueue<Session.SessionEvent>();
            static private EventLoop instance = new EventLoop();

            static public void Update()
            {
                Session.SessionEvent evt;
                while (true == instance.eventQueue.TryDequeue(out evt))
                {
                    try
                    {
                        evt.OnEvent();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"{e.GetType().Name}(Event:{evt.GetType().Name}, Message:{e.Message})");
#if UNITY_EDITOR || USE_DEBUGGING
                        Debug.Log($"[Async Debug Info] Event Caller Location :\n{evt.CallStack}");
#endif
                    }
                }
            }

            public static void EnqueuEvent(Session.SessionEvent evt)
            {
                instance.eventQueue.Enqueue(evt);
            }
        }
    }
}
