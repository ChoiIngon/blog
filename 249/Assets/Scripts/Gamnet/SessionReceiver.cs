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
                    session.socket.BeginReceive(receiveBytes, 0, MAX_BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), null);
                }
                catch (SocketException e)
                {
                    session.Close();
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    Int32 recvBytesSize = session.socket.EndReceive(result);
                    if (0 == recvBytesSize)
                    {
                        session.Close();
                        return;
                    }
                    receiveBuffer.Append(this.receiveBytes, 0, recvBytesSize);
                }
                catch (ObjectDisposedException e)
                {
                    session.Close();
                    return;
                }
                catch (SocketException e)
                {
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

                    ReceiveEvent evt = new ReceiveEvent(session, packet);
                    EventLoop.EnqueuEvent(evt);
                }

                BeginReceive();
            }
        }
    }
}
