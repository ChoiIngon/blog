using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Breakout.Client
{
    public static class Network
    {
        private static Gamnet.Client.Session session = new Gamnet.Client.Session();
        public static Action OnConnectEvent
        {
            get { return session.OnConnectEvent; }
            set { session.OnConnectEvent += value; }
        }
        public static Action OnCloseEvent
        {
            get { return session.OnCloseEvent; }
            set { session.OnCloseEvent += value; }
        }
        public static Action<System.Exception> OnErrorEvent
        {
            get { return session.OnErrorEvent; }
            set { session.OnErrorEvent += value; }
        }
        public static Action OnPauseEvent
        {
            get { return session.OnPauseEvent; }
            set { session.OnPauseEvent += value; }
        }
        public static Action OnResumeEvent
        {
            get { return session.OnResumeEvent; }
            set { session.OnResumeEvent += value; }
        }

        public static void Connect()
        {
            session.AsyncConnect(GameManager.Instance.host, GameManager.Instance.port);
        }

        public static void Close()
        {
            session.Close();
        }

        public static void Send(object msg)
        {
            FieldInfo fieldInfo = msg.GetType().GetField("PACKET_ID");
            uint packetId = (uint)fieldInfo.GetValue(msg);
            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = packetId;
            packet.Serialize(msg);
            session.Send(packet);
        }

        public static void RegisterHandler<T>(Action<T> handler) where T : new()
        {
            FieldInfo fieldInfo = typeof(T).GetField("PACKET_ID");
            uint packetId = (uint)fieldInfo.GetValue(null);
            session.RegisterHandler<T>(packetId, handler);
        }

        public static void UnregisterHandler<T>(uint msgId) where T : new()
        {
            FieldInfo fieldInfo = typeof(T).GetField("PACKET_ID");
            uint packetId = (uint)fieldInfo.GetValue(null);
            session.UnregisterHandler(packetId);
        }

        public static int NetworkDelay
        {
            get
            {
#if UNITY_EDITOR
                return GameManager.Instance.packetDelay;
#else
                return session.network_delay.time;
#endif
            }
        }
    }
}
