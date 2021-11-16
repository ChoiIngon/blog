using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamnet.Async
{
    public class AsyncTask : AsyncOperation
    {
        public AsyncTask(Session session, Action action) : base(session)
        {
            Task.Run(() =>
            {
                action();
                AsyncTaskCompleteEvent evt = new AsyncTaskCompleteEvent(session, enumerator);
                EventLoop.EnqueuEvent(evt);
            });
        }

        public class AsyncTaskCompleteEvent : SessionEvent
        {
            IEnumerator enumerator;
            public AsyncTaskCompleteEvent(Session session, IEnumerator enumerator) : base(session)
            {
                this.enumerator = enumerator;
            }

            public override void OnEvent()
            {
                session.enumerator = enumerator;
                enumerator.MoveNext();
            }
        }
    }
}
