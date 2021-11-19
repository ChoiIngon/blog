using System;
using System.Collections.Generic;

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

        protected override void OnReceive(Packet packet)
        {
            dispatcher.OnReceive(this, packet);
        }

        protected override void OnAccept()
        {
        }

        protected override void OnClose()
        {
        }
    }
}
