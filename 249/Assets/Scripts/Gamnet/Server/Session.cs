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
        private System.Timers.Timer heartbeat_timer;
        public Session()
        {
            int sessionKey = Interlocked.Increment(ref SESSION_KEY);
            session_key = unchecked((uint)sessionKey);
            session_token = "";
            heartbeat_timer = new System.Timers.Timer();
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
                StopHeartBeatTimer();

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

        void StartHeartBeatTimer()
        {
            heartbeat_timer.Enabled = true;
            heartbeat_timer.Interval = 1000;
            heartbeat_timer.AutoReset = true;
            heartbeat_timer.Elapsed += delegate
            {
                Session.EventLoop.EnqueuEvent(new ActionEvent(this, () =>
                {
                    SystemPacket.MsgSvrCli_HeartBeat_Req req = new SystemPacket.MsgSvrCli_HeartBeat_Req();
                    req.recv_seq = recv_seq;
                    req.date_time = System.DateTime.Now;

                    Gamnet.Packet packet = new Gamnet.Packet();
                    packet.Id = SystemPacket.MsgSvrCli_HeartBeat_Req.MSG_ID;
                    packet.Serialize(req);
                    this.Send(packet);
                }));
            };
            heartbeat_timer.Start();
        }

        void StopHeartBeatTimer()
        {
            heartbeat_timer.Stop();
        }
    }
}
