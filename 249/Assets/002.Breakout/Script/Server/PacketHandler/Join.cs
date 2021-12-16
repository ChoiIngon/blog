using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Breakout.Server
{
    class Join : Gamnet.Server.PacketHandler<Session>
    {
        public override uint Id()
        {
            return Packet.MsgCliSvr_Join_Req.PACKET_ID;
        }

        public override IEnumerator OnReceive(Session session, Gamnet.Packet packet)
        {
            Packet.MsgCliSvr_Join_Req req = packet.Deserialize<Packet.MsgCliSvr_Join_Req>();

            Room room = Manager.Room.Find(req.roomId);
            room.AddUser(session);
            if (2 == room.sessions.Count)
            {
                Manager.Room.Remove(room.Id);
            }

            yield break;
        }
    }
}
