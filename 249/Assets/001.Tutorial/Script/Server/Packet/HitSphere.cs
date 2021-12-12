using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityServer.Common.Packet;
using UnityServer.Server;

namespace UnityServer.Packet
{
    class HitSphere : Gamnet.Server.PacketHandler<Server.Main.Session>
    {
        public override uint Id()
        {
            return MsgCliSvr_HitSphere_Ntf.MSG_ID;
        }

        public override IEnumerator OnReceive(Server.Main.Session session, Gamnet.Packet packet)
        {
            MsgCliSvr_HitSphere_Ntf ntf = packet.Deserialize<MsgCliSvr_HitSphere_Ntf>();
            Sphere sphere = null;
            if (false == session.spheres.TryGetValue(ntf.id, out sphere))
            {
                yield break;
            }

            Vector3 hitDirection = ntf.hitDirection;
            sphere.rigidBody.velocity += hitDirection * 30.0f;
            yield break;
        }
    }
}
