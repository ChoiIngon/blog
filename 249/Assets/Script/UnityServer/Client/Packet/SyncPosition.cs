using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityServer.Common.Packet;

namespace UnityServer.Client.Packet
{
    static class SuncPosition
    {
        public static void OnReceive(MsgSvrCli_SyncPosition_Ntf ntf)
        {
            Client.Main client = Client.Main.Instance;

            Common.Sphere sphere = null;
            if (false == client.spheres.TryGetValue(ntf.id, out sphere))
            {
                return;
            }

            if (true == client.syncPosition)
            {
                sphere.transform.localPosition = new Vector3(ntf.positionX, ntf.positionY, ntf.positionZ);
            }
            if (true == client.syncRotation)
            {
                sphere.transform.rotation = new Quaternion(ntf.rotationX, ntf.rotationY, ntf.rotationZ, ntf.rotationW);
            }
            if (true == client.syncVelocity)
            {
                sphere.rigidBody.velocity = new Vector3(ntf.velocityX, ntf.velocityY, ntf.velocityZ);
            }
        }
    }
}
