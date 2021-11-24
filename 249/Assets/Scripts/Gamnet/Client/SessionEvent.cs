using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamnet.Client
{
    public partial class Session : Gamnet.Session
    {
        public class ConnectEvent : SessionEvent
        {
            public ConnectEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                Session clientSession = session as Session;
                clientSession.BeginReceive();
                if (0 == clientSession.session_key)
                {
                    clientSession.Send_EstablishSessionLink_Req();
                }
                else
                {
                    clientSession.Send_RecoverSessionLink_Req();
                }
            }
        }

        public new class CloseEvent : SessionEvent
        {
            public CloseEvent(Session session) : base(session) { }
            public override void OnEvent()
            {
                Session clientSession = session as Session;
                if (true == clientSession.establish_link)
                {
                    clientSession.Send_DestroySessionLink_Req();
                    return;
                }
                clientSession.SocketClose();
            }
        }
    }
}
