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
        private System.Timers.Timer heartbeat_timer;
        private Ping ping;

        public Session()
        {
            Clear();
            int sessionKey = Interlocked.Increment(ref SESSION_KEY);
            session_key = unchecked((uint)sessionKey);
            heartbeat_timer = new System.Timers.Timer();
            heartbeat_timer.Interval = 1000;
            heartbeat_timer.AutoReset = false;
            heartbeat_timer.Elapsed += delegate
            {
                if (null == socket)
                {
                    return;
                }

                if (false == socket.Connected)
                {
                    return;
                }

                TimeSpan span = DateTime.Now - receiver.last_recv_time;
                if (span.TotalSeconds > SessionManager.keepalive_time)
                {
                    Session.EventLoop.EnqueuEvent(new Receiver.CloseEvent(this));
                    return;
                }

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
                this.heartbeat_timer.Start();
            };
            ping = new Ping();
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

                    SessionManager.Remove(this);

                    async_receives.Clear();
                    current_coroutine = null;
                    OnClose();
                    Clear();
                }
            }
            catch (SocketException e)
            {
                Debug.Log($"[{Gamnet.Util.Debug.__FUNC__()}] exception:" + e.ToString());
            }
        }

        public void Kickout()
        {
            link_establish = false;
            Close();
        }

        void StartHeartBeatTimer()
        {

            heartbeat_timer.Start();
        }

        private class Ping
        {
            public double max { get; private set; }
            public double min { get; private set; }
            public double total { get; private set; }
            public int count { get; private set; }
            public Ping()
            {
                max = 0;
                min = double.MaxValue;
                total = 0;
                count = 0;
            }
            public void Update(double t)
            {
                count++;
                max = Math.Max(max, t);
                min = Math.Min(min, t);
                total += t;
            }
        }
    }
}
