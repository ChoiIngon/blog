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
                    session.socket.BeginReceive(receiveBytes, 0, MAX_BUFFER_SIZE, 0, new AsyncCallback((IAsyncResult result) =>
                    {
                        Session.EventLoop.EnqueuEvent(new EndReceiveEvent(session, result));
                    }), session.socket);
                }
                catch (SocketException e)
                {
                    Debug.Log(e.Message);
                    session.Close();
                }
            }

            class EndReceiveEvent : Gamnet.Session.SessionEvent
            {
                private IAsyncResult result;
                public EndReceiveEvent(Session session, IAsyncResult result) : base(session)
                {
                    this.result = result;
                }

                public override void OnEvent()
                {
                    session.receiver.OnEndReceive(result);
                }
            }

            private void OnEndReceive(IAsyncResult result)
            {
                Debug.Assert(Gamnet.Util.Debug.IsMainThread());
                try
                {
                    Socket socket = (Socket)result.AsyncState;
                    Int32 recvBytesSize = socket.EndReceive(result);
                    if (0 == recvBytesSize)
                    {
                        session.Close();
                        return;
                    }
                    receiveBuffer.Append(this.receiveBytes, 0, recvBytesSize);
                }
                catch (ObjectDisposedException e)
                {
                    Debug.Log(e.Message);
                    session.Close();
                    return;
                }
                catch (SocketException e)
                {
                    Debug.Log(e.Message);
                    session.Close();
                    return;
                }

                while (Packet.HEADER_SIZE <= receiveBuffer.Size())
                {
                    Packet packet = new Packet(receiveBuffer);
                    if (packet.Length > Gamnet.Buffer.MAX_BUFFER_SIZE)
                    {
                        session.Close();
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

                    if (session.recv_seq < packet.Seq)
                    {
                        session.recv_seq = packet.Seq;
                        session.OnReceive(packet);

                        if (true == packet.IsReliable)
                        {
                            session.SendReliableAckNtf();
                        }
                    }
                }

                BeginReceive();
            }
        }
    }
}
