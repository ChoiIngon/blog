using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamnet.Server
{
    public partial class Session : Gamnet.Session
    {
        public new class CreateEvent : SessionEvent
        {
            public CreateEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                Session serverSession = session as Session;
                SessionManager.Add(serverSession);
                serverSession.OnCreate();
            }
        }

        public new class DestoryEvent : SessionEvent
        {
            public DestoryEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                Session serverSession = session as Session;
                if (true == serverSession.enable_handover)
                {
                    return;
                }

                serverSession.OnDestory();
                SessionManager.Remove(serverSession);
            }
        }
    }
}
