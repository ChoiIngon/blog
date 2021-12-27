using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamnet.Server
{
    public partial class Session : Gamnet.Session
    {
        public class AcceptEvent : SessionEvent
        {
            public AcceptEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                Session serverSession = session as Session;
                SessionManager.Add(serverSession);
                serverSession.BeginReceive();
            }
        }
    }
}
