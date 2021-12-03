using System.Collections;
using UnityEngine;
using UnityServer.Common.Packet;

namespace UnityServer.Handler
{
    class CreateRoom : Gamnet.Server.PacketHandler<Server.Session>
    {
        public override uint Id()
        {
            return MsgCliSvr_CreateRoom_Req.MSG_ID;
        }

        public override IEnumerator OnReceive(Server.Session session, Gamnet.Packet packet)
        {
            MsgCliSvr_CreateRoom_Req req = packet.Deserialize<MsgCliSvr_CreateRoom_Req>();
            session.room = Server.Main.Instance.CreateRoom();
            session.room.session = session;
            session.room.transform.SetParent(Server.Main.Instance.transform, false);
            session.room.gameObject.name = $"Room_{session.session_key}";

            MsgSvrCli_CreateRoom_Ans ans = new MsgSvrCli_CreateRoom_Ans();
            session.Send<MsgSvrCli_CreateRoom_Ans>(ans);
            yield break;
        }
    }
}
