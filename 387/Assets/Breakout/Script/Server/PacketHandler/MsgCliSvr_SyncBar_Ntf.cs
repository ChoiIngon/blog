using System.Collections;

namespace Breakout.Server
{
    class MsgCliSvr_SyncBar_Ntf : Gamnet.Server.PacketHandler<Session>
    {
        public override uint Id()
        {
            return Packet.MsgCliSvr_SyncBar_Ntf.PACKET_ID;
        }

        public override IEnumerator OnReceive(Session session, Gamnet.Packet packet)
        {
            Bar bar = session.bar;
            Room room = session.room;
            {
                Packet.MsgCliSvr_SyncBar_Ntf ntf = packet.Deserialize<Packet.MsgCliSvr_SyncBar_Ntf>();
                bar.destination = ntf.destination;
            }

            {
                Packet.MsgSvrCli_SyncBar_Ntf ntf = new Packet.MsgSvrCli_SyncBar_Ntf();
                ntf.objectId = bar.id;
                ntf.destination = bar.destination;

                foreach (Session s in room.sessions)
                {
                    if (session == s)
                    {
                        continue;
                    }

                    s.Send(ntf);
                }
            }
            yield break;
        }
    }
}
