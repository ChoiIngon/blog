using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Gamnet
{
    public class PacketHandler<T> where T : ServerSession
    {
        public virtual uint Id()
        {
            return 0;
        }
        public virtual IEnumerator OnReceive(T session, Packet packet)
        {
            yield break;
        }
    }


    class Dispatcher<T> where T : ServerSession
    {
        private Dictionary<uint, PacketHandler<T>> handlers = new Dictionary<uint, PacketHandler<T>>();
        public void Init()
        {
            handlers.Clear();

            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] assems = currentDomain.GetAssemblies();

            IEnumerable<Type> childrenTypes = assems.SelectMany(s => s.GetTypes()).Where(p => typeof(PacketHandler<T>).IsAssignableFrom(p) && p.IsClass);
            foreach (var type in childrenTypes)
            {
                PacketHandler<T> packetHandler = Activator.CreateInstance(type) as PacketHandler<T>;
                handlers.Add(packetHandler.Id(), packetHandler);
            }
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
