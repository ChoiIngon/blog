using System;
using System.Collections;
using System.Net.Sockets;
using System.Timers;

namespace Gamnet.Async
{
    public class AsyncReceive : AsyncOperation
    {
        public uint msgId { get; private set; }
        public Timer timer { get; private set; }
        public Packet Packet { get; private set; }

        public AsyncReceive(Session session, uint msgId, int expireTime) : base(session)
        {
            /*
            if (Session.State.Connected != session.state)
            {
                this.Exception = new SocketException();
                session.current_coroutine = coroutine;
                session.current_coroutine.MoveNext();
                return;
            }
            */
            this.msgId = msgId;

            timer = new Timer();
            timer.Interval = expireTime * 1000;
            timer.AutoReset = false;
            timer.Elapsed += delegate { OnTimeout(); };
            timer.Start();

            session.async_receives.Add(msgId, this);
        }

        public void OnReceive(Packet packet)
        {
            timer.Stop();
            timer.Dispose();
            this.Packet = packet;
        }

        public void Cancel()
        {
            timer.Stop();
            timer.Dispose();
            this.Packet = null;
            this.Exception = new OperationCanceledException();
            session.current_coroutine = coroutine;
            session.current_coroutine.MoveNext();
        }

        private void OnTimeout()
        {
            this.Exception = new TimeoutException();
            TimeoutEvent evt = new TimeoutEvent(session, msgId, coroutine);
            Session.EventLoop.EnqueuEvent(evt);
        }

        private class TimeoutEvent : Session.SessionEvent
        {
            uint msgId;
            IEnumerator enumerator;
            public TimeoutEvent(Session session, uint msgId, IEnumerator enumerator) : base(session)
            {
                this.msgId = msgId;
                this.enumerator = enumerator;
            }

            public override void OnEvent()
            {
                session.async_receives.Remove(msgId);
                enumerator.MoveNext();
            }
        }
    }
}
