using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
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

        public IEnumerator current_coroutine;
        public Dictionary<uint, Async.AsyncReceive> async_receives;

        //private Timeout timeout = new Timeout();
        //private System.Timers.Timer timer;
        //private int timeoutInterval = 5000; // 비동기 connect 실패 시간. 5초

        private byte[] receiveBytes = new byte[MAX_BUFFER_SIZE];
        public Buffer receiveBuffer = new Buffer();

        private List<Packet> send_queue = new List<Packet>();
        private int send_queue_index;
        private UInt32 send_seq = 0;
        private UInt32 recv_seq = 0;

        public Session()
        {
            this.async_receives = new Dictionary<uint, Async.AsyncReceive>();
        }

        #region AsyncReceive
        public void BeginReceive()
        {
            if (false == socket.Connected)
            {
                return;
            }
            try
            {
                socket.BeginReceive(receiveBytes, 0, MAX_BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), null);
            }
            catch (SocketException e)
            {
                Close();
            }
        }

        // 요 부분을 서버와 클라이언트 분리해서, 서버에서는 소켓 closse가발생하면 바로 close하지 않고 Pause 이벤트 날리고,
        // timeout 되거나, 클라이언트로 부터 명시적 close가 날아 오면 세션을 destroy하는 걸로 분리
        private void ReceiveCallback(IAsyncResult result)
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
            catch (ObjectDisposedException e)
            {
                Close();
                return;
            }
            catch (SocketException e)
            {
                Close();
                return;
            }

            while (Packet.HEADER_SIZE <= receiveBuffer.Size())
            {
                Packet packet = new Packet(receiveBuffer);
                if (packet.Length > Gamnet.Buffer.MAX_BUFFER_SIZE)
                {
                    Close();
                    return;
                }

                if (packet.Length > receiveBuffer.Size())
                {
                    // not enough
                    BeginReceive();
                    return;
                }

                receiveBuffer.Remove(packet.Length);
                receiveBuffer = new Buffer(receiveBuffer);

                ReceiveEvent evt = new ReceiveEvent(this, packet);
                EventLoop.EnqueuEvent(evt);
            }

            BeginReceive();
        }
        #endregion

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
                Close();
            }
        }

        public virtual void Close()
        {
            throw new System.NotImplementedException();
        }
    }
}
