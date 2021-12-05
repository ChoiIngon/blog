using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityServer.Common.Packet;
using UnityServer.Server;

namespace UnityServer.Packet
{
    class CreateRoom : Gamnet.Server.PacketHandler<Server.Main.Session>
    {
        public override uint Id()
        {
            return MsgCliSvr_CreateRoom_Req.MSG_ID;
        }

        public override IEnumerator OnReceive(Server.Main.Session session, Gamnet.Packet packet)
        {
            MsgCliSvr_CreateRoom_Req req = packet.Deserialize<MsgCliSvr_CreateRoom_Req>();
            GameObject room = Object.Instantiate<GameObject>(Server.Main.Instance.roomPrefab);

            room.name = $"Room_{session.session_key}";
            room.layer = LayerMask.NameToLayer("Server");
            room.transform.SetParent(Server.Main.Instance.transform, false);
            session.room = room;

            List<Vector3> initPositions = new List<Vector3>();
            for (int x = -4; x <= 4; x++)
            {
                for (int y = -4; y <= 4; y++)
                {
                    for (int z = -4; z <= 4; z++)
                    {
                        initPositions.Add(new Vector3(x, y, z));
                    }
                }
            }
            Transform spheresTransform = room.transform.Find("Spheres");

            for (uint i = 0; i < Server.Main.Instance.objectCount; i++)
            {
                GameObject go = Object.Instantiate<GameObject>(Server.Main.Instance.spherePrefab);
                go.name = $"Sphere_{i+1}";
                go.layer = LayerMask.NameToLayer("Server");
                go.transform.SetParent(spheresTransform, false);

                Sphere sphere = go.AddComponent<Sphere>();
                sphere.id = i + 1;
                sphere.rigidBody = sphere.GetComponent<Rigidbody>();

                int index = Random.Range(0, initPositions.Count);
                sphere.transform.localPosition = initPositions[index];
                initPositions.RemoveAt(index);

                session.spheres.Add(sphere.id, sphere);

                MsgSvrCli_CreateSphere_Ntf ntf = new MsgSvrCli_CreateSphere_Ntf();
                ntf.id = sphere.id;
                ntf.localPosition = sphere.transform.localPosition;
                ntf.rotation = sphere.transform.rotation;
                ntf.velocity = sphere.rigidBody.velocity;
                session.Send<MsgSvrCli_CreateSphere_Ntf>(ntf);
            }

            MsgSvrCli_CreateRoom_Ans ans = new MsgSvrCli_CreateRoom_Ans();
            session.Send<MsgSvrCli_CreateRoom_Ans>(ans);
            yield break;
        }
    }
}
