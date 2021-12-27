using Gamnet.SystemPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Gamnet.Client
{
    public partial class Session : Gamnet.Session
    {
        private abstract class IPacketHandler
        {
            public abstract void OnReceive(Packet packet);
        }

        private class PacketHandler<T> : IPacketHandler where T : new()
        {
            private Session session;
            public PacketHandler(Session session)
            {
                this.session = session;
            }
            public Action<T> onReceive;
            public override void OnReceive(Packet packet)
            {
                if (null == onReceive)
                {
                    return;
                }

                BinaryFormatter bf = new BinaryFormatter();
                packet.buffer.ms.Position = Packet.HEADER_SIZE;
                T msg = (T)bf.Deserialize(packet.buffer.ms);
                onReceive(msg);
            }
        }

        public Action OnConnectEvent;
        public Action OnCloseEvent;
        public Action OnPauseEvent;
        public Action OnResumeEvent;
        public Action<System.Exception> OnErrorEvent;

        private Connector connector;
        private Dictionary<uint, IPacketHandler> handlers = new Dictionary<uint, IPacketHandler>();
        private System.Timers.Timer heartbeat_timer;

        private int HEARTBEAT_TIMER_INTERVAL = 1000;
        private Ping ping;
        public int ping_time { get; private set; }

        public class NetworkDelay
        {
            private const int SAMPLE_COUNT = 4;
            private List<int> pings = new List<int>();

            public NetworkDelay()
            {
            }

            public void Update(int ping)
            {
                pings.Add(ping);
                if (SAMPLE_COUNT < pings.Count)
                {
                    pings.RemoveAt(0);
                }
            }

            public int max { get { return pings.Max(); } }
            public int min { get { return pings.Min(); } }
            public int time { get { return 0 == pings.Count ? 0 : (int)pings.Average(); } }
        }

        public NetworkDelay network_delay;


        public Session()
        {
            Clear();
            this.connector = new Connector(this);

            RegisterHandler<MsgSvrCli_EstablishSessionLink_Ans>(SystemPacket.MsgSvrCli_EstablishSessionLink_Ans.MSG_ID, Recv_EstabilshSessionLink_Ans);
            RegisterHandler<MsgSvrCli_RecoverSessionLink_Ans>(SystemPacket.MsgSvrCli_RecoverSessionLink_Ans.MSG_ID, Recv_RecoverSessionLink_Ans);
            RegisterHandler<MsgSvrCli_HeartBeat_Ans>(SystemPacket.MsgSvrCli_HeartBeat_Ans.MSG_ID, Recv_HeartBeat_Ans);
            RegisterHandler<Msg_ReliableAck_Ntf>(SystemPacket.Msg_ReliableAck_Ntf.MSG_ID, Recv_ReliableAck_Ntf);
        }

        public void AsyncConnect(string host, int port, int timeout_sec = 5)
        {
            this.connector.AsyncConnect(host, port, timeout_sec);
        }

        public void Connect(string host, int port, int timeout_sec = 5)
        {
            this.connector.Connect(host, port, timeout_sec);
        }

        public void RegisterHandler<T>(uint msgId, Action<T> handler) where T : new()
        {
            PacketHandler<T> packetHandler = null;

            if (true == handlers.ContainsKey(msgId))
            {
                packetHandler = (PacketHandler<T>)handlers[msgId];
            }
            else
            {
                packetHandler = new PacketHandler<T>(this);
                handlers.Add(msgId, packetHandler);
            }
            packetHandler.onReceive += handler;
        }

        public void UnregisterHandler(uint msgId)
        {
            handlers.Remove(msgId);
        }

        protected override void OnConnect()
        {
            network_delay = new NetworkDelay();
            heartbeat_timer = new System.Timers.Timer();
            heartbeat_timer.Interval = HEARTBEAT_TIMER_INTERVAL;
            heartbeat_timer.AutoReset = false;
            heartbeat_timer.Elapsed += delegate
            {
                Session.EventLoop.EnqueuEvent(new ActionEvent(this, () =>
                {
                    if (null == ping)
                    {
                        ping = new Ping(connector.endpoint.Address.ToString());
                    }
                    else
                    {
                        if (true == ping.isDone)
                        {
                            ping_time = ping.time;
                            ping = null;
                        }
                    }

                    SystemPacket.MsgCliSvr_HeartBeat_Req req = new SystemPacket.MsgCliSvr_HeartBeat_Req();
                    req.recv_seq = recv_seq;
                    req.date_time = System.DateTime.Now;

                    Gamnet.Packet packet = new Gamnet.Packet();
                    packet.Id = SystemPacket.MsgCliSvr_HeartBeat_Req.MSG_ID;
                    packet.Serialize(req);
                    this.Send(packet);
                }));
                this.heartbeat_timer.Start();
            };
            heartbeat_timer.Start();
            OnConnectEvent?.Invoke();
        }

        protected override void OnReceive(Packet packet)
        {
            try
            {
                if (false == handlers.ContainsKey(packet.Id))
                {
                    Debug.Log("can't find registered msg(id:" + packet.Id + ")");
                    return;
                }

                IPacketHandler handler = handlers[packet.Id];
                handler.OnReceive(packet);
            }
            catch (System.Exception e)
            {
                Error(e);
            }
        }

        protected override void OnPause()
        {
            OnPauseEvent?.Invoke();
        }

        protected override void OnResume()
        {
            OnResumeEvent?.Invoke();
        }

        protected override void OnClose()
        {
            OnCloseEvent?.Invoke();
        }

        protected override void OnError(Exception e)
        {
            //Log.Write(Log.LogLevel.ERR, e.ToString());
            OnErrorEvent?.Invoke(e);
        }

        public void Pause()
        {
            socket.Close();
            OnPause();
        }

        public void Resume()
        {
            if (null == socket)
            {
                return;
            }

            if (true == socket.Connected)
            {
                return;
            }
            connector.AsyncReconnect();
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

            if (true == link_establish)
            {
                Send_DestroySessionLink_Ntf();
            }
            socket.Close();
            OnClose();
            Clear();
        }

        public void Error(System.Exception e)
        {
            ErrorEvent evt = new ErrorEvent(this, e);
            evt.exception = e;
            EventLoop.EnqueuEvent(evt);
        }

		public override void Send(Packet packet)
		{
            base.Send(packet);
            if (true == link_establish)
            {
                if (null == socket || false == socket.Connected)
                {
                    connector.AsyncReconnect();
                }
            }
        }
    }
}
