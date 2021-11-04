using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Gamnet
{
    public class Server<T> : SessionManager where T : ServerSession, new()
    {
        private Socket tcp_socket;
        private Socket udp_socket;

        private Dispatcher<T> dispatcher = new Dispatcher<T>();

        public int MaxSessionCount;

        System.Timers.Timer timer = new System.Timers.Timer();
        public void Listen(int port, int maxSessionCount)
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

            T session = new T();
            session.socket = clientSocket;
            session.session_manager = this;

            Add(session);

            tcp_socket.BeginAccept(AcceptCallback, null);

            SessionEvent evt = new AcceptEvent(session);
            SessionEventQueue.Instance.EnqueuEvent(evt);
        }

        public void Update()
        {
            SessionEventQueue.Instance.Update();
        }

        public override void Dispatch(ServerSession session, Packet packet)
        {
            T session_t = session as T;
            dispatcher.OnReceive(session_t, packet);
        }
    }
}