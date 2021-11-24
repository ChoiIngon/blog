using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Gamnet.Server
{
    public partial class Session : Gamnet.Session
    {
        public static int SESSION_KEY = 0;
        public readonly uint session_key;

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
            Debug.Log($"[{Util.Debug.__FUNC__()}] server");
            BeginDisconnect();
        }

        private void BeginDisconnect()
        {
            if (null == socket)
            {
                return;
            }

            if (false == socket.Connected)
            {
                return;
            }
            Debug.Log($"[{Util.Debug.__FUNC__()}] server");
            try
            {
                socket.BeginDisconnect(false, new AsyncCallback((IAsyncResult result) => {
                    Session.EventLoop.EnqueuEvent(new EndDisconnectEvent(this, result));
                }), socket);
            }
            catch (SocketException e)
            {
                Debug.LogError("[Session.BeginDisconnect] exception:" + e.ToString());
            }
            catch (ObjectDisposedException e)
            {
                Debug.LogError("[Session.BeginDisconnect] exception:" + e.ToString());
            }
        }

        public class EndDisconnectEvent : SessionEvent
        {
            private IAsyncResult result;
            public EndDisconnectEvent(Session session, IAsyncResult result) : base(session)
            {
                this.result = result;
            }
            public override void OnEvent()
            {
                Session serverSession = session as Session;
                serverSession.OnEndDisconnect(result);
            }
        }

        private void OnEndDisconnect(IAsyncResult result)
        {
            try
            {
                socket.EndDisconnect(result);
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
                Debug.Log($"[{Gamnet.Util.Debug.__FUNC__()}] session_state:" + state.ToString() + ", exception:" + e.ToString());
            }
        }
    }
}
