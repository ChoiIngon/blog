using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamnet
{
    public class EventLoop
    {
        private ConcurrentQueue<Session.SessionEvent> eventQueue = new ConcurrentQueue<Session.SessionEvent>();
        static private EventLoop instance = new EventLoop();

        static public void Update()
        {
            Session.SessionEvent evt;
            while (true == instance.eventQueue.TryDequeue(out evt))
            {
                evt.OnEvent();
            }
        }

        public static void EnqueuEvent(Session.SessionEvent evt)
        {
            instance.eventQueue.Enqueue(evt);
        }
    }
}
