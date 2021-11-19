using System;
using System.Collections;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Gamnet.Async
{
    public class AsyncTask : AsyncOperation
    {
        public AsyncTask(Session session, Action action) : base(session)
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
            Task.Run(() =>
            {
                action();
                AsyncTaskCompleteEvent evt = new AsyncTaskCompleteEvent(session, coroutine);
                EventLoop.EnqueuEvent(evt);
            });
        }

        public class AsyncTaskCompleteEvent : Session.SessionEvent
        {
            IEnumerator coroutine;
            public AsyncTaskCompleteEvent(Session session, IEnumerator coroutine) : base(session)
            {
                this.coroutine = coroutine;
            }

            public override void OnEvent()
            {
                session.current_coroutine = coroutine;
                session.current_coroutine.MoveNext();
            }
        }
    }
}
