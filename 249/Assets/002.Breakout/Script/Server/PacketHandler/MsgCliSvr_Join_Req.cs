using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Breakout.Server
{
    class MsgCliSvr_Join_Req : Gamnet.Server.PacketHandler<Session>
    {
        public override uint Id()
        {
            return Packet.MsgCliSvr_Join_Req.PACKET_ID;
        }

        public override IEnumerator OnReceive(Session session, Gamnet.Packet packet)
        {
            Packet.MsgCliSvr_Join_Req req = packet.Deserialize<Packet.MsgCliSvr_Join_Req>();
            Packet.MsgSvrCli_Join_Ans ans = new Packet.MsgSvrCli_Join_Ans();
            ans.errorCode = Packet.ErrorCode.Success;

            Room room = Main.Room.Find(req.roomId);
            room.AddUser(session);
            session.Send(ans);

            if (2 == room.sessions.Count)
            {
                Main.Room.Remove(room.Id);
                room.Ready();
            }

            yield break;
        }
    }
}
