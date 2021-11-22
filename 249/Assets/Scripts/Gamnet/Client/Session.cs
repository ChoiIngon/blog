using Gamnet.SystemPacket;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Gamnet.Client
{
    public class Session : Gamnet.Session
    {
        private static  UInt32 SESSION_KEY = 0;
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
                System.Type type = msg.GetType();
                onReceive(msg);
            }
        }

        public  Action OnCreateEvent;
        public  Action OnConnectEvent;
        public  Action OnCloseEvent;
        public Action OnDestroyEvent;
        public Action OnPauseEvent;
        public Action OnResumeEvent;
        public  Action<System.Exception> OnErrorEvent;

        private UInt32 recv_packet_seq = 0;
        private Connector connector;
        private Dictionary<uint, IPacketHandler> handlers = new Dictionary<uint, IPacketHandler>();

        public Session() : base(++Session.SESSION_KEY)
        {
            this.connector = new Connector(this);

            RegisterHandler<MsgSvrCli_Connect_Ans>(SystemPacket.MsgSvrCli_Connect_Ans.MSG_ID, Recv_Connect_Ans);
            RegisterHandler<MsgSvrCli_Close_Ans>(SystemPacket.MsgSvrCli_Close_Ans.MSG_ID, Recv_Close_Ans);
            RegisterHandler<MsgSvrCli_Reconnect_Ans>(SystemPacket.MsgSvrCli_Reconnect_Ans.MSG_ID, Recv_Reconnect_Ans);
            RegisterHandler<MsgSvrCli_HeartBeat_Ans>(SystemPacket.MsgSvrCli_HeartBeat_Ans.MSG_ID, Recv_HeartBeat_Ans);
            RegisterHandler<MsgSvrCli_ReliableAck_Ntf>(SystemPacket.MsgSvrCli_ReliableAck_Ntf.MSG_ID, Recv_ReliableAck_Ntf);
        }

        public void AsyncConnect(string host, int port, int timeout_sec = 5)
        {
            this.connector.AsyncConnect(host, port, timeout_sec);
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

        public void Pause()
        {
            OnPause();
        }

        public void Resume()
        {
            OnResume();
        }

        protected override void OnCreate()
        {
            OnCreateEvent?.Invoke();
        }

        protected override void OnConnect()
        {
            OnConnectEvent?.Invoke();
        }

        protected override void OnReceive(Packet packet)
        {
            try
            {
                if (false == handlers.ContainsKey(packet.Id))
                {
                    throw new System.Exception("can't find registered msg(id:" + packet.Id + ")");
                }

                if (false == packet.IsReliable || recv_packet_seq < packet.Seq)
                {
                    IPacketHandler handler = handlers[packet.Id];

                    handler.OnReceive(packet);

                    recv_packet_seq = Math.Max(recv_packet_seq, packet.Seq);
                    if (true == packet.IsReliable)
                    {
                        //Send_ReliableAck_Ntf(_recv_seq);
                    }
                }
            }
            catch (System.Exception e)
            {
                Error(e);
            }
        }

        protected override void OnPause()
        {
            socket.Close();
        }

        protected override void OnResume()
        {
            connector.AsyncReconnect();
        }

        protected override void OnClose()
        {
            OnCloseEvent?.Invoke();
        }

        protected override void OnError(Exception e)
        {
            Log.Write(Log.LogLevel.ERR, e.ToString());
            OnErrorEvent?.Invoke(e);
        }

        void Send_Connect_Req()
        {
            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = SystemPacket.MsgCliSvr_Connect_Req.MSG_ID;
            SystemPacket.MsgCliSvr_Connect_Req req = new SystemPacket.MsgCliSvr_Connect_Req();
            packet.Serialize(req);
            AsyncSend(packet);
        }

        void Recv_Connect_Ans(MsgSvrCli_Connect_Ans ans)
        {
            if (0 != ans.error_code)
            {
                Debug.LogError("connect fail(error_code:" + ans.error_code + ")");
                Error(null);
                return;
            }

            //session_token = ans.session_token;

            ConnectEvent evt = new ConnectEvent(this);
            Session.EventLoop.EnqueuEvent(evt); // already locked
        }

        void Recv_Close_Ans(MsgSvrCli_Close_Ans ans)
        {
            if (0 != ans.error_code)
            {
                Debug.LogError("connect fail(error_code:" + ans.error_code + ")");
                Error(null);
                return;
            }

            //session_token = ans.session_token;

            CloseEvent evt = new CloseEvent(this);
            Session.EventLoop.EnqueuEvent(evt); // already locked
        }

        void Recv_Reconnect_Ans(MsgSvrCli_Reconnect_Ans ans)
        {
            if (0 != ans.error_code)
            {
                Debug.LogError("connect fail(error_code:" + ans.error_code + ")");
                Error(null);
                return;
            }

            //session_token = ans.session_token;
            OnResume();
        }

        void Recv_HeartBeat_Ans(MsgSvrCli_HeartBeat_Ans ans)
        {
            if (0 != ans.error_code)
            {
                Debug.LogError("connect fail(error_code:" + ans.error_code + ")");
                Error(null);
                return;
            }

            //session_token = ans.session_token;
        }

        void Recv_ReliableAck_Ntf(MsgSvrCli_ReliableAck_Ntf ntf)
        {
        }
    }
}
