using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

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
    }
}
