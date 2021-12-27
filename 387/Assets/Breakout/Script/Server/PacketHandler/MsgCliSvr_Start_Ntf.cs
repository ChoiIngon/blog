using System.Collections;
using UnityEngine;

namespace Breakout.Server
{
    class MsgCliSvr_Start_Ntf : Gamnet.Server.PacketHandler<Session>
    {
        public override uint Id()
        {
            return Packet.MsgCliSvr_Start_Ntf.PACKET_ID;
        }

        public override IEnumerator OnReceive(Session session, Gamnet.Packet packet)
        {
            {
                Packet.MsgCliSvr_Start_Ntf ntf = packet.Deserialize<Packet.MsgCliSvr_Start_Ntf>();
                session.ball.transform.SetParent(session.room.transform);
                session.ball.rigidBody.useGravity = false;
                session.ball.SetDirection(Vector3.up + new Vector3(Random.Range(-0.5f, 0.5f), 0, 0));
                session.room.state = Room.State.Play;
            }

            {
                Packet.MsgSvrCli_Start_Ntf ntf = new Packet.MsgSvrCli_Start_Ntf();
                ntf.objectId = session.ball.id;

                Room room = session.room;
                foreach (Session s in room.sessions)
                {
                    if (s == session)
                    {
                        continue;
                    }
                    s.Send(ntf);
                }
            }

            session.room.SyncBall(session.ball);
            yield break;
        }
    }
}
