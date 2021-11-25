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

    public interface IPacketHandler
    {
    }

    public abstract class PacketHandler<T> : IPacketHandler where T : Server.Session
    {
        public abstract uint Id();
        public abstract IEnumerator OnReceive(T session, Packet packet);
        public virtual bool IsSystemPacket
        {
            get { return false; }
        }
    }

    public interface IDispatcher
    {
        void OnReceive(Session session, Packet packet);
    }

    class Dispatcher<SESSION_T> : IDispatcher where SESSION_T : Session
    {
        private Dictionary<uint, PacketHandler<SESSION_T>> handlers = new Dictionary<uint, PacketHandler<SESSION_T>>();
        public void Init()
        {
            handlers.Clear();
            handlers.Add(SystemPacket.MsgCliSvr_EstablishSessionLink_Req.MSG_ID, new Session.PacketHandler_EstablishSessionLink<SESSION_T>());
            handlers.Add(SystemPacket.MsgCliSvr_RecoverSessionLink_Req.MSG_ID, new Session.PacketHandler_RecoverSessionLink<SESSION_T>());
            handlers.Add(SystemPacket.MsgCliSvr_DestroySessionLink_Req.MSG_ID, new Session.PacketHandler_DestroySessionLink<SESSION_T>());
            handlers.Add(SystemPacket.MsgCliSvr_HeartBeat_Req.MSG_ID, new Session.PacketHandler_HeartBeat<SESSION_T>());
            handlers.Add(SystemPacket.MsgCliSvr_ReliableAck_Ntf.MSG_ID, new Session.PacketHandler_ReliableAck<SESSION_T>());

            string exeAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] assems = currentDomain.GetAssemblies();
            IEnumerable<Assembly> executingAssembly = assems.Where(a => a.GetName().Name.Equals(exeAssemblyName));

            IEnumerable<Type> childrenTypes = executingAssembly.SelectMany(s => s.GetTypes()).Where(p => typeof(PacketHandler<SESSION_T>).IsAssignableFrom(p) && p.IsClass);
            foreach (var type in childrenTypes)
            {
                PacketHandler<SESSION_T> packetHandler = Activator.CreateInstance(type) as PacketHandler<SESSION_T>;
                handlers.Add(packetHandler.Id(), packetHandler);
            }
        }

        public void OnReceive(Session session, Packet packet)
        {
            SESSION_T session_t = session as SESSION_T;
            OnReceive(session_t, packet);
        }

        private void OnReceive(SESSION_T session, Packet packet)
        {
            Async.AsyncReceive asyncReceive;
            if (true == session.async_receives.TryGetValue(packet.Id, out asyncReceive))
            {
                asyncReceive.OnReceive(packet);
                session.async_receives.Remove(packet.Id);
                session.current_coroutine = asyncReceive.coroutine;
                session.current_coroutine.MoveNext();
                return;
            }

            PacketHandler<SESSION_T> packetHandler;
            if (true == handlers.TryGetValue(packet.Id, out packetHandler))
            {
                if (false == session.link_establish && false == packetHandler.IsSystemPacket)
                {
                    Debug.Assert(false, "invalid link");
                    session.Close();
                    return;
                }
                session.current_coroutine = packetHandler.OnReceive(session, packet);
                session.current_coroutine.MoveNext();
            }
        }
    }
}
