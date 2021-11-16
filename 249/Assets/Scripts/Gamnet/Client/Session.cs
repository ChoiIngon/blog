using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace Gamnet.Client
{
    public class Session : Gamnet.Session
    {
        private static UInt32 SESSION_KEY = 0;
        private UInt32 recv_packet_seq = 0;

        public Action OnConnectEvent;
        private IPEndPoint endPoint; // 자동 재접속을 위해
        public abstract class IPacketHandler
        {
            public abstract void OnReceive(Packet packet);
        }

        public class PacketHandler<T> : IPacketHandler where T : new()
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

        private Dictionary<uint, IPacketHandler> handlers = new Dictionary<uint, IPacketHandler>();

        public ConcurrentQueue<SessionEvent> sessionEventQueue = new ConcurrentQueue<SessionEvent>();
        public Session() : base(++Session.SESSION_KEY)
        {
        }

        public override void OnConnect()
        {
            this?.OnConnectEvent();
        }

        public override void OnReceive(Packet packet)
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
        public void Update()
        {
            SessionEvent evt;
            while (true == sessionEventQueue.TryDequeue(out evt))
            {
                evt.OnEvent();
            }
        }

        protected override void OnPacket(Packet packet)
        {
            ReceiveEvent evt = new ReceiveEvent(this, packet);
            sessionEventQueue.Enqueue(evt);
        }
    }
}
