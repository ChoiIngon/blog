﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamnet
{
    public class Connector
    {
        private Session session;
        public EndPoint endPoint;
        public Connector(Session session)
        {
            this.session = session;
        }

        public void AsyncConnect(string host, int port, int timeout_sec = 5)
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

            session.state = Session.ConnectionState.OnConnecting;
            endPoint = new IPEndPoint(ipAddress, port);
            session.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            session.socket.BeginConnect(endPoint, new AsyncCallback(AsyncConnectCallback), null);
        }

        private void AsyncConnectCallback(IAsyncResult result)
        {
            session.socket.EndConnect(result);
            session.socket.ReceiveBufferSize = Gamnet.Session.MAX_BUFFER_SIZE;
            session.socket.SendBufferSize = Gamnet.Session.MAX_BUFFER_SIZE;
            session.state = Session.ConnectionState.Connected;
            //_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 10000);
            //_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10000);

            ConnectEvent evt = new ConnectEvent(session);
            SessionEventQueue.Instance.EnqueuEvent(evt);
        }
    }
}