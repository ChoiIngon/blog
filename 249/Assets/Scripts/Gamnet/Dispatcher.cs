using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gamnet
{
    public interface PacketHandler<T> where T : ServerSession
    {
        uint Id();
        void OnReceive(T session, Packet packet);
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
            PacketHandler<T> packetHandler;
            if (false == handlers.TryGetValue(packet.Id, out packetHandler))
            {
                return;
            }
            packetHandler.OnReceive(session, packet);
        }
    }
}
