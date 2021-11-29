﻿using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

namespace UnityServer.Packet
{
    class CreateCube : Gamnet.Server.PacketHandler<Server.Session>
    {

        public override uint Id()
        {
            return Packet.MsgCliSvr_CreateSphereReq.MSG_ID;
        }

        public override IEnumerator OnReceive(Server.Session session, Gamnet.Packet packet)
        {
            Packet.MsgCliSvr_CreateSphereReq req = packet.Deserialize<Packet.MsgCliSvr_CreateSphereReq>();
            GameObject sphere = UnityServer.Server.Instance.CreateSphere();
            sphere.transform.SetParent(session.Agent.transform, false);
            session.Agent.sphere = sphere;
            yield break;
        }
    }
}