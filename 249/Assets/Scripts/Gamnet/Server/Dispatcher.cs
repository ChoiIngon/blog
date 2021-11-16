using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Gamnet.Server
{
    public class TestMethod : System.Attribute
    {
    }

    public abstract class IPacketHandler
    {
        public abstract uint Id();

    }
    public abstract class PacketHandler<T> : IPacketHandler where T : Session
    {
        public abstract IEnumerator OnReceive(T session, Packet packet);
    }


    public interface IDispatcher
    {
        void OnReceive(Session session, Packet packet);
    }
    class Dispatcher<T> : IDispatcher where T : Session
    {
        private Dictionary<uint, PacketHandler<T>> handlers = new Dictionary<uint, PacketHandler<T>>();
        public void Init()
        {
            handlers.Clear();

            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] assems = currentDomain.GetAssemblies();

            IEnumerable<Type> childrenTypes = assems.SelectMany(s => s.GetTypes()).Where(p => typeof(PacketHandler<T>).IsAssignableFrom(p));
            foreach (var type in childrenTypes)
            {
                PacketHandler<T> packetHandler = Activator.CreateInstance(type) as PacketHandler<T>;
                handlers.Add(packetHandler.Id(), packetHandler);
            }
        }

        public void OnReceive(Session session, Packet packet)
        {
            T session_t = session as T;
            OnReceive(session_t, packet);
        }
        public void OnReceive(T session, Packet packet)
        {
            Async.AsyncReceive asyncReceive;
            if (true == session.async_receives.TryGetValue(packet.Id, out asyncReceive))
            {
                asyncReceive.OnReceive(packet);
                return;
            }

            PacketHandler<T> packetHandler;
            if (true == handlers.TryGetValue(packet.Id, out packetHandler))
            {
                session.enumerator = packetHandler.OnReceive(session, packet);
                session.enumerator.MoveNext();
            }
        }
    }
}
