using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace Gamnet
{
    public partial class Session
    {
        public class Receiver
        {
            public const int MAX_BUFFER_SIZE = 1024;

            public Session session;
            private byte[] receiveBytes = new byte[MAX_BUFFER_SIZE];
            private Buffer receiveBuffer = new Buffer();
            public DateTime last_recv_time { get; private set; }

            public Receiver(Session session)
            {
                this.session = session;
            }

            public void BeginReceive()
            {
                if (false == session.socket.Connected)
                {
                    return;
                }
                try
                {
                    session.socket.BeginReceive(receiveBytes, 0, MAX_BUFFER_SIZE, 0, new AsyncCallback(OnEndReceive), session.socket);
                }
                catch (SocketException e)
                {
                    Debug.Log(e.Message);
                    session.Close();
                }
            }

            public class CloseEvent : SessionEvent
            {
                public CloseEvent(Session session) : base(session) { }
                public override void OnEvent()
                {
                    session.Close();
                }
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
                    if (false == packet.IsReliable)
                    {
                        session.OnReceive(packet);
                    }
                    else
                    {
                        if (session.recv_seq < packet.Seq)
                        {
                            session.recv_seq = packet.Seq;
                            session.OnReceive(packet);
                            session.SendReliableAckNtf();
                        }
                    }
                }
            }

            private void OnEndReceive(IAsyncResult result)
            {
                last_recv_time = DateTime.Now;
                try
                {
                    Socket socket = (Socket)result.AsyncState;
                    Int32 recvBytesSize = socket.EndReceive(result);
                    if (0 == recvBytesSize)
                    {
                        EventLoop.EnqueuEvent(new CloseEvent(session));
                        return;
                    }
                    receiveBuffer.Append(this.receiveBytes, 0, recvBytesSize);
                }
                catch (ObjectDisposedException e)
                {
                    Debug.Log(e.Message);
                    //EventLoop.EnqueuEvent(new CloseEvent(session));
                    return;
                }
                catch (SocketException e)
                {
                    Debug.Log(e.Message);
                    EventLoop.EnqueuEvent(new CloseEvent(session));
                    return;
                }

                while (Packet.HEADER_SIZE <= receiveBuffer.Size())
                {
                    Packet packet = new Packet(receiveBuffer);
                    if (packet.Length > Gamnet.Buffer.MAX_BUFFER_SIZE)
                    {
                        EventLoop.EnqueuEvent(new CloseEvent(session));
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

                    EventLoop.EnqueuEvent(new ReceiveEvent(session, packet));
                }

                BeginReceive();
            }
        }
    }
}
