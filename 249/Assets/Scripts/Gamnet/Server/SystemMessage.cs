using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamnet.Server.SystemMessage
{
    class SystemMsgCliSvr_Connect<SESSION_T> : PacketHandler<SESSION_T> where SESSION_T : Server.Session
    {
        public override uint Id()
        {
            return Gamnet.SystemMessage.MsgSvrCli_Connect_Ans.MSG_ID;
        }

        public override IEnumerator OnReceive(SESSION_T session, Gamnet.Packet packet)
        {
            yield break;
        }
    }
}
