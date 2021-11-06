using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamnet
{
    public abstract class SessionEvent
    {
        protected Session session;
        public SessionEvent(Session session)
        {
            this.session = session;
        }
        public abstract void OnEvent();
    };

    public class AcceptEvent : SessionEvent
    {
        public AcceptEvent(Session session) : base(session) { }
        public override void OnEvent()
        {
            session.AsyncReceive();
            session.OnAccept();
        }
    }
    public class ConnectEvent : SessionEvent
    {
        public ConnectEvent(Session session) : base(session) { }
        public override void OnEvent()
        {
            session.AsyncReceive();
            session.OnConnect();
        }
    }
    public class ReconnectEvent : SessionEvent
    {
        public ReconnectEvent(Session session) : base(session) { }
        public override void OnEvent()
        {
            session.AsyncReceive();
            session.OnReconnect();
        }
    }
    public class PauseEvent : SessionEvent
    {
        public PauseEvent(Session session) : base(session) { }
        public override void OnEvent()
        {
            session.OnPause();
        }
    }
    public class ResumeEvent : SessionEvent
    {
        public ResumeEvent(Session session) : base(session) { }
        public override void OnEvent()
        {
            session.AsyncReceive();
            session.OnResume();
        }
    }
    public class ErrorEvent : SessionEvent
    {
        public ErrorEvent(Session session) : base(session) { }
        public System.Exception exception;
        public override void OnEvent()
        {
            session.OnError(exception);
        }
    }
    public class CloseEvent : SessionEvent
    {
        public CloseEvent(Session session) : base(session) { }
        public override void OnEvent()
        {
            session.OnClose();
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
            session.OnReceive(this.packet);
        }
    }

    public class SessionEventQueue
    {
        private ConcurrentQueue<SessionEvent> eventQueue = new ConcurrentQueue<SessionEvent>();
        static private SessionEventQueue instance = new SessionEventQueue();
        static public SessionEventQueue Instance
        {
            get
            {
                return instance;
            }
        }
        public void Update()
        {
            SessionEvent evt;
            while (true == eventQueue.TryDequeue(out evt))
            {
                evt.OnEvent();
            }
        }
        public void EnqueuEvent(SessionEvent evt)
        {
            eventQueue.Enqueue(evt);
        }
    }
}
