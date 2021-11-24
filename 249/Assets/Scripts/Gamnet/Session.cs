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
        public State state = State.Close;
        public Receiver receiver;
        public IEnumerator current_coroutine;
        public Dictionary<uint, Async.AsyncReceive> async_receives;
        public bool establish_link { get; protected set; }
        //private Timeout timeout = new Timeout();
        //private System.Timers.Timer timer;
        //private int timeoutInterval = 5000; // 비동기 connect 실패 시간. 5초

        private byte[] receiveBytes = new byte[MAX_BUFFER_SIZE];
        private Buffer receiveBuffer = new Buffer();

        private List<Packet> send_queue = new List<Packet>();
        private int send_queue_index;
        private UInt32 send_seq = 0;
        private UInt32 recv_seq = 0;

        public Session()
        {
            this.establish_link = false;
            this.async_receives = new Dictionary<uint, Async.AsyncReceive>();
            this.receiver = new Receiver(this);
        }

        public void BeginReceive()
        {
            receiver.BeginReceive();
        }
        public void Send(Packet packet)
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
                socket.BeginSend(bufferToBeSend.ToByteArray(), 0, packetToBeSent.Length, 0, new AsyncCallback(SendCallback), null);
            }
        }

        private void SendCallback(IAsyncResult result)
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
                        socket.BeginSend(packet.buffer.ToByteArray(), packet.buffer.read_index, packet.buffer.Size(), 0, new AsyncCallback(SendCallback), null);
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
                        socket.BeginSend(bufferToBeSend.ToByteArray(), 0, bufferToBeSend.Size(), 0, new AsyncCallback(SendCallback), null);
                    }
                }
            }
            catch (SocketException e)
            {
                Debug.Log("[Session.Callback_Disconnect] session_state:" + state.ToString() + ", exception:" + e.ToString());
                Close();
            }
        }

        public virtual void Close()
        {
            throw new System.NotImplementedException();
        }

    }
}