using System;

namespace Gamnet.Server
{
    public class Session : Gamnet.Session
    {
        public static UInt32 SESSION_KEY = 0;
        public static SessionManager session_manager = new SessionManager();
        public IDispatcher dispatcher;
        public string session_token;

        public Session() : base(++SESSION_KEY)
        {
        }

        protected override void OnReceive(Packet packet)
        {
            dispatcher.OnReceive(this, packet);
        }

        protected override void OnPassiveClose()
        {
            OnPause();
        }

        public new class CreateEvent : SessionEvent
        {
            public CreateEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                Server.Session serverSession = session as Server.Session;
                Server.Session.session_manager.Add(serverSession);
                serverSession.OnCreate();
            }
        }

        public new class DestoryEvent : SessionEvent
        {
            public DestoryEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                Server.Session serverSession = session as Server.Session;
                serverSession.OnDestory();
                Server.Session.session_manager.Remove(serverSession);
            }
        }
    }
}
