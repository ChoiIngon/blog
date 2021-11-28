using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace Gamnet
{
	public partial class Session
    {
        public Socket socket;
        public uint session_key { get; protected set; }
        public string session_token { get; protected set; }
        public Receiver receiver;
        public IEnumerator current_coroutine;
        public Dictionary<uint, Async.AsyncReceive> async_receives;
        public bool link_establish { get; protected set; }
        //private Timeout timeout = new Timeout();
        //private System.Timers.Timer timer;
        //private int timeoutInterval = 5000; // 비동기 connect 실패 시간. 5초

        protected List<Packet> send_queue;
        protected int send_queue_index;
        protected UInt32 send_seq;
        protected UInt32 recv_seq;

        public Session()
        {
        }

        public virtual void Clear()
        {
            session_key = 0;
            session_token = "";
            link_establish = false;
            async_receives = new Dictionary<uint, Async.AsyncReceive>();
            receiver = new Receiver(this);
            send_queue = new List<Packet>();
            send_queue_index = 0;
            send_seq = 0;
            recv_seq = 0;
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
                Debug.LogWarning($"{GetType().Namespace}.{GetType().Name}(session_key:{this.session_key})");
                return;
            }

            if (false == socket.Connected)
            {
                Debug.LogWarning($"{GetType().Namespace}.{GetType().Name} disconnected (session_key:{this.session_key})");
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
            }), socket);
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
                Socket socket = (Socket)result.AsyncState;
                if (false == socket.Connected)
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
                    }), socket);
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
                    }), socket);
                }
            }
            catch (ObjectDisposedException e)
            {
                Debug.Log($"[{Gamnet.Util.Debug.__FUNC__()}] {this.GetType().Name}, Exception:{e.GetType().Name}, Message:{ e.Message}");
                Debug.Log($"[{Gamnet.Util.Debug.__FUNC__()}] fail to send:{send_queue[send_queue_index].Id}");
            }
            catch (SocketException e)
            {
                Debug.Log($"[{Gamnet.Util.Debug.__FUNC__()}] session_key:{session_key}, exception:{e.ToString()}");
            }
        }

        public virtual void Close()
        {
            throw new System.NotImplementedException();
        }

        protected void RemoveSentPacket(uint msg_seq)
        {
            while (0 < send_queue.Count)
            {
                if (send_queue[0].Seq > msg_seq)
                {
                    break;
                }
                if (true == send_queue[0].IsReliable)
                {
                    send_queue_index--;
                }
                send_queue.RemoveAt(0);
            }
        }

        private void SendReliableAckNtf()
        {
            SystemPacket.Msg_ReliableAck_Ntf ntf = new SystemPacket.Msg_ReliableAck_Ntf();
            ntf.recv_seq = recv_seq;

            Packet packet = new Packet();
            packet.Id = SystemPacket.Msg_ReliableAck_Ntf.MSG_ID;
            packet.Serialize(ntf);
            Send(packet);
        }
    }
}