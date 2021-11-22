using System;
using System.Threading;

namespace Gamnet.Server
{
    public partial class Session : Gamnet.Session
    {
        public static int SESSION_KEY = 0;
        public readonly uint session_key;

        public IDispatcher dispatcher;
        public string session_token;
        public bool enable_handover { get; private set; }
        public Session()
        {
            int sessionKey = Interlocked.Increment(ref SESSION_KEY);
            this.session_key = unchecked((uint)sessionKey);

            session_token = "";
            enable_handover = false;
        }

        protected override void OnReceive(Packet packet)
        {
            dispatcher.OnReceive(this, packet);
        }

        public override void Destroy()
        {
            EventLoop.EnqueuEvent(new DestoryEvent(this));
        }
    }
}
