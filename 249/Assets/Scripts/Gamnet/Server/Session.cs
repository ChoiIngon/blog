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

        public override void Close()
        {
            if (null == socket)
            {
                return;
            }

            if (false == socket.Connected)
            {
                return;
            }

            try
            {
                socket.BeginDisconnect(false, new AsyncCallback(CloseCallback), socket);
            }
            catch (SocketException e)
            {
                Debug.LogError("[Session.Disconnect] exception:" + e.ToString());
            }
            catch (ObjectDisposedException e)
            {
                Debug.LogError("[Session.Disconnect] exception:" + e.ToString());
            }
        }

        private void CloseCallback(IAsyncResult result)
        {
            try
            {
                socket.EndDisconnect(result);
                if (true == enable_handover)
                {
                    EventLoop.EnqueuEvent(new PauseEvent(this));
                }
                else
                {
                    EventLoop.EnqueuEvent(new CloseEvent(this));
                }
            }
            catch (SocketException e)
            {
                Debug.Log($"[{Gamnet.Util.Debug.__FUNC__()}] session_state:" + state.ToString() + ", exception:" + e.ToString());
            }
        }
    }
}
