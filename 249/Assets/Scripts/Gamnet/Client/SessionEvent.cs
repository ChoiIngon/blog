﻿using System.Collections;
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
    }
}