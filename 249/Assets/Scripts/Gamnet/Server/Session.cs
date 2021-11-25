using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Gamnet.Server
{
    public partial class Session : Gamnet.Session
    {
        public static int SESSION_KEY = 0;
        
        public IDispatcher dispatcher;
        public string session_token;

        public Session()
        {
            int sessionKey = Interlocked.Increment(ref SESSION_KEY);
            this.session_key = unchecked((uint)sessionKey);

            session_token = "";
        }

        protected override void OnReceive(Packet packet)
        {
            dispatcher.OnReceive(this, packet);
        }

        public override void Close()
        {
            Debug.Assert(Gamnet.Util.Debug.IsMainThread());
            try
            {
                socket.Close();
                if (true == link_establish)
                {
                    OnPause();
                }
                else
                {
                    foreach (var pair in async_receives)
                    {
                        Async.AsyncReceive asyncReceive = pair.Value;
                        asyncReceive.Cancel();
                    }

                    async_receives.Clear();
                    current_coroutine = null;
                    OnClose();
                    SessionManager.Remove(this);
                }
            }
            catch (SocketException e)
            {
                Debug.Log($"[{Gamnet.Util.Debug.__FUNC__()}] exception:" + e.ToString());
            }
        }

        
    }
}
