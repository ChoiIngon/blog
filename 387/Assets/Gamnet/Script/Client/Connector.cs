using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using UnityEngine;

namespace Gamnet.Client
{
    public partial class Session : Gamnet.Session
    {
        public class Connector
        {
            private Session session;
            private Timer timer;
            public IPEndPoint endpoint { get; private set; }

            public Connector(Session session)
            {
                this.session = session;
                this.timer = new Timer();
            }

            public void Connect(string host, int port, int expireTime = 5)
            {
                IPAddress ipAddress = null;
                try
                {
                    ipAddress = IPAddress.Parse(host);
                }
                catch (System.FormatException)
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(host);
                    if (hostEntry.AddressList.Length > 0)
                    {
                        ipAddress = hostEntry.AddressList[0];
                    }
                }

                timer.Interval = expireTime * 1000;
                timer.AutoReset = false;
                timer.Elapsed += delegate { OnTimeout(); };
                endpoint = new IPEndPoint(ipAddress, port);

                Reconnect();
            }

            public void Reconnect()
            {
                timer.Start();
                session.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                session.socket.Connect(endpoint);
                timer.Stop();
                session.socket.ReceiveBufferSize = Gamnet.Buffer.MAX_BUFFER_SIZE;
                session.socket.SendBufferSize = Gamnet.Buffer.MAX_BUFFER_SIZE;
                //_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 10000);
                //_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10000);
                session.BeginReceive();
                Session clientSession = session as Session;
                if (false == clientSession.link_establish)
                {
                    clientSession.Send_EstablishSessionLink_Req();
                }
                else
                {
                    clientSession.Send_RecoverSessionLink_Req();
                }
            }

            public void AsyncConnect(string host, int port, int expireTime = 5)
            {
                IPAddress ipAddress = null;
                try
                {
                    ipAddress = IPAddress.Parse(host);
                }
                catch (System.FormatException)
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(host);
                    if (hostEntry.AddressList.Length > 0)
                    {
                        ipAddress = hostEntry.AddressList[0];
                    }
                }

                timer.Interval = expireTime * 1000;
                timer.AutoReset = false;
                timer.Elapsed += delegate { OnTimeout(); };
                endpoint = new IPEndPoint(ipAddress, port);

                AsyncReconnect();
            }

            public void AsyncReconnect()
            {
                timer.Start();
                session.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                session.socket.BeginConnect(endpoint, new AsyncCallback((IAsyncResult result) => {
                    Gamnet.Session.EventLoop.EnqueuEvent(new EndConnectEvent(session, result));
                }), session.socket);
            }

            class EndConnectEvent : Gamnet.Session.SessionEvent
            {
                private IAsyncResult result;
                public EndConnectEvent(Session session, IAsyncResult result) : base(session)
                {
                    this.result = result;
                }

                public override void OnEvent()
                {
                    Session clientSession = session as Session;
                    clientSession.connector.OnEndConnect(result);
                }
            }

            private void OnEndConnect(IAsyncResult result)
            {
                Debug.Assert(Gamnet.Util.Debug.IsMainThread());
                Socket socket = (Socket)result.AsyncState;
                timer.Stop();
                try
                {
                    socket.EndConnect(result);
                    socket.ReceiveBufferSize = Gamnet.Buffer.MAX_BUFFER_SIZE;
                    socket.SendBufferSize = Gamnet.Buffer.MAX_BUFFER_SIZE;
                    //_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 10000);
                    //_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10000);
                    session.BeginReceive();
                    Session clientSession = session as Session;
                    if (false == clientSession.link_establish)
                    {
                        clientSession.Send_EstablishSessionLink_Req();
                    }
                    else
                    {
                        clientSession.Send_RecoverSessionLink_Req();
                    }
                }
                catch (SocketException e)
                {
                    session.Error(e);
                }
            }

            private void OnTimeout()
            {
                Gamnet.Session.EventLoop.EnqueuEvent(new Gamnet.Session.ErrorEvent(session, new TimeoutException()));
            }
        }
    }
}
