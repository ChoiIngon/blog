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

            Room room = null;
            if (false == Main.Instance.rooms.TryGetValue(req.roomId, out room))
            {
                GameObject obj = new GameObject();
                room = obj.AddComponent<Room>();
                room.Id = req.roomId;
                Main.Instance.rooms.Add(room.Id, room);
            }

            session.room = room;
            yield break;
        }
    }
}
