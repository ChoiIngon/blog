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
                sphere.transform.localPosition = ntf.localPosition;
            }
            if (true == client.syncRotation)
            {
                sphere.transform.rotation = ntf.rotation;
            }
            if (true == client.syncVelocity)
            {
                sphere.rigidBody.velocity = ntf.velocity;
            }
        }
    }
}
