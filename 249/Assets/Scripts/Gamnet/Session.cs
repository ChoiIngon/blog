using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace Gamnet
{
	public partial class Session
    {
        public enum State
        {
            Close,          // 연결이 되어 있지 않은 상태
            OnConnecting,   // 비동기 연결 시도 중. 아직 완료 안됨
            Connected,      // 연결 완료 상태
            Pause,          // 모바일에서 앱이 백그라운드로 넘어가 일시적으로 접속이 끊긴 상태
            Handover        // 모바일에서 이동 등으로 인해 접속이 잠시 끊긴 상태
        }

        public const int MAX_BUFFER_SIZE = 1024;
        public Socket socket;
        public uint session_key { get; protected set; }
        public State state = State.Close;
        public Receiver receiver;
        public IEnumerator current_coroutine;
        public Dictionary<uint, Async.AsyncReceive> async_receives;
        public bool link_establish { get; protected set; }
        //private Timeout timeout = new Timeout();
        //private System.Timers.Timer timer;
        //private int timeoutInterval = 5000; // 비동기 connect 실패 시간. 5초

        protected List<Packet> send_queue = new List<Packet>();
        protected int send_queue_index;
        private UInt32 send_seq = 0;
        private UInt32 recv_seq = 0;

        public Session()
        {
            this.link_establish = false;
            this.async_receives = new Dictionary<uint, Async.AsyncReceive>();
            this.receiver = new Receiver(this);
        }

        public void BeginReceive()
        {
            receiver.BeginReceive();
        }

        public virtual void Send(Packet packet)
        {
            Debug.Assert(Gamnet.Util.Debug.IsMainThread());

            packet.Seq = ++send_seq;

            send_queue.Add(packet);

            if (null == socket)
            {
                Debug.LogError($"{GetType().Namespace}.{GetType().Name}(session_key:{this.session_key})");
                return;
            }

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
            socket.BeginSend(bufferToBeSend.ToByteArray(), 0, packetToBeSent.Length, 0, new AsyncCallback((IAsyncResult result) => {
                Session.EventLoop.EnqueuEvent(new EndSendEvent(this, result));
            }), null);
        }

        protected void Resend()
        {
            List<Packet> unsentQueue = new List<Packet>();
            foreach (Packet unsentPacket in send_queue)
            {
                unsentQueue.Add(unsentPacket);
            }

            send_queue.Clear();
            send_queue_index = 0;

            foreach (Packet unsentPacket in unsentQueue)
            {
                Send(unsentPacket);
            }
        }

        protected void SendSystemPacket(Packet packet)
        {
            if (null == socket)
            {
                Debug.LogError($"{GetType().Namespace}.{GetType().Name}(session_key:{this.session_key})");
                return;
            }

            if (false == socket.Connected)
            {
                return;
            }

            Buffer bufferToBeSend = packet.buffer;
            socket.BeginSend(bufferToBeSend.ToByteArray(), 0, packet.Length, 0, new AsyncCallback((IAsyncResult result) => {}), null);
        }

        protected void ReSend()
        {
            List<Packet> resendQueue = new List<Packet>();
            foreach (Packet packet in send_queue)
            {
                resendQueue.Add(packet);
            }

            send_queue_index = 0;
            send_queue.Clear();

            foreach (Packet packet in resendQueue)
            {
                Send(packet);
            }
        }

        class EndSendEvent : Gamnet.Session.SessionEvent
        {
            private IAsyncResult result;
            public EndSendEvent(Session session, IAsyncResult result) : base(session)
            {
                this.result = result;
            }

            public override void OnEvent()
            {
                session.OnEndSend(result);
            }
        }

        private void OnEndSend(IAsyncResult result)
        {
            Debug.Assert(Gamnet.Util.Debug.IsMainThread());
            try
            {
                if (null == socket)
                {
                    return;
                }
                int writtenBytes = socket.EndSend(result);

                Packet packet = send_queue[send_queue_index];
                if (false == packet.buffer.Remove(writtenBytes))
                {
                    throw new System.OverflowException();
                }

                if (0 < packet.buffer.Size())
                {
                    socket.BeginSend(packet.buffer.ToByteArray(), packet.buffer.read_index, packet.buffer.Size(), 0, new AsyncCallback((IAsyncResult r) =>
                    {
                        Session.EventLoop.EnqueuEvent(new EndSendEvent(this, r));
                    }), null);
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
                    socket.BeginSend(bufferToBeSend.ToByteArray(), 0, bufferToBeSend.Size(), 0, new AsyncCallback((IAsyncResult r) =>
                    {
                        Session.EventLoop.EnqueuEvent(new EndSendEvent(this, r));
                    }), null);
                }
            }
            catch (ObjectDisposedException e)
            {
                Debug.Log($"[{Gamnet.Util.Debug.__FUNC__()}] {this.GetType().Name}, Exception:{e.GetType().Name}, Message:{ e.Message}");
                Debug.Log($"[{Gamnet.Util.Debug.__FUNC__()}] fail to send:{send_queue[send_queue_index].Id}");
            }
            catch (SocketException e)
            {
                Debug.Log($"[{Gamnet.Util.Debug.__FUNC__()}] {state.ToString()}, exception:{e.ToString()}");
            }
        }

        public virtual void Close()
        {
            throw new System.NotImplementedException();
        }

    }
}