using System.Collections;
using System.Collections.Generic;

namespace Gamnet
{
    public class Timeout
    {
        private Dictionary<uint, System.Timers.Timer> timers = new Dictionary<uint, System.Timers.Timer>();
        public delegate void OnTimeout();

        public void SetTimeout(uint seq, int interval, OnTimeout timeoutCallback)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = interval;
            timer.AutoReset = false;
            timer.Elapsed += delegate
            {
                timeoutCallback();
            };
            timer.Start();

            if (true == timers.ContainsKey(seq))
            {
                throw new System.Exception($"duplicated timeout register(msg_seq:{seq})");
            }
            timers.Add(seq, timer);
        }

        public void UnsetTimeout(uint seq)
        {
            if (false == timers.ContainsKey(seq))
            {
                return;
            }

            System.Timers.Timer timer = timers[seq];
            timer.Enabled = false;
            timer.Stop();
            timer.Dispose();
            timers.Remove(seq);
        }
    }
}