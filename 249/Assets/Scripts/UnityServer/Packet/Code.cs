using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityServer.Packet
{
    public class Code
    {
        public const uint MsgCliSvr_Greeting_Req = 1;
        public const uint MsgSvrCli_Greeting_Ans = 2;
        public const uint MsgCliSvr_Greeting_Ntf = 3;
    }
}