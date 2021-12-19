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
            Packet.MsgCliSvr_SyncBar_Ntf ntf = packet.Deserialize<Packet.MsgCliSvr_SyncBar_Ntf>();

            Bar bar = session.bar;
            bar.position = ntf.localPosition;
            yield break;
        }
    }
}
