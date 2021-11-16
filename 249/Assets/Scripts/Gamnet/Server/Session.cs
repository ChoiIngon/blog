using System;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Gamnet.Server
{
    public class Session : Gamnet.Session
    {
        static UInt32 SESSION_KEY = 0;
        public ISessionManager session_manager;
        public IDispatcher dispatcher;
        public Session() : base(++SESSION_KEY)
        {
        }

        public override void OnReceive(Packet packet)
        {
            dispatcher.OnReceive(this, packet);
        }

        public override void OnAccept()
        {
        }

        public override void OnClose()
        {

        }

        protected override void OnPacket(Packet packet)
        {
            ReceiveEvent evt = new ReceiveEvent(this, packet);
            EventLoop.EnqueuEvent(evt);
        }
    }
}
