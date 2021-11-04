using System;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Gamnet
{
    public class ServerSession : Gamnet.Session
    {
        static UInt32 SESSION_KEY = 0;
        public SessionManager session_manager;

        public ServerSession() : base(++SESSION_KEY)
        {
        }

        public override void OnReceive(Packet packet)
        {
            session_manager.Dispatch(this, packet);
        }

        public override void OnAccept()
        {
            Debug.Log("OnAccept");
        }

        public override void OnClose()
        {

        }
    }
}
