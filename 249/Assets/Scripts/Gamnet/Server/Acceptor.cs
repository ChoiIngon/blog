using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Gamnet.Server
{
    public class Acceptor<SESSION_T> where SESSION_T : Server.Session, new()
    {
        private Socket tcp_socket;
        private Socket udp_socket;

        private SessionManager<SESSION_T> session_manager = new SessionManager<SESSION_T>();
        private Dispatcher<SESSION_T> dispatcher = new Dispatcher<SESSION_T>();

        public int MaxSessionCount;

        System.Timers.Timer timer = new System.Timers.Timer();
        public void Init(int port, int maxSessionCount)
        {
            dispatcher.Init();

            tcp_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            udp_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

            tcp_socket.Bind(endPoint);
            tcp_socket.Listen(maxSessionCount/2);
            tcp_socket.BeginAccept(AcceptCallback, null);

            udp_socket.Bind(endPoint);
        }

        void AcceptCallback(IAsyncResult ar)
        {
            Socket clientSocket = tcp_socket.EndAccept(ar);

            SESSION_T session = new SESSION_T();
            session.socket = clientSocket;
            session.session_manager = session_manager;
            session.dispatcher = dispatcher;

            session_manager.Add(session);

            tcp_socket.BeginAccept(AcceptCallback, null);

            SessionEvent evt = new AcceptEvent(session);
            EventLoop.EnqueuEvent(evt);
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
    }
}