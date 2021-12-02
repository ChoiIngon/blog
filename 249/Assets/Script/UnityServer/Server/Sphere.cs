using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityServer.Common.Packet;
using static UnityServer.Server;

namespace UnityServer
{
    public class Sphere : MonoBehaviour
    {
        public Session session;
        public uint id;
        private Rigidbody rigidBody;
        // Start is called before the first frame update
        void Start()
        {
            Debug.Assert(null != session);
            rigidBody = GetComponent<Rigidbody>();

            /*
            MsgSvrCli_CreateSphere_Ntf ntf = new MsgSvrCli_CreateSphere_Ntf();
            ntf.id = id;
            ntf.positionX = transform.localPosition.x;
            ntf.positionY = transform.localPosition.y;
            ntf.positionZ = transform.localPosition.z;
            ntf.velocityX = rigidBody.velocity.x;
            ntf.velocityY = rigidBody.velocity.x;
            ntf.velocityZ = rigidBody.velocity.x;
            session.Send<MsgSvrCli_CreateSphere_Ntf>(ntf);
            */
        }

        // Update is called once per frame
        void Update()
        {
            /*
            MsgSvrCli_SyncPosition_Ntf ntf = new MsgSvrCli_SyncPosition_Ntf();
            ntf.id = id;
            ntf.positionX = transform.localPosition.x;
            ntf.positionY = transform.localPosition.y;
            ntf.positionZ = transform.localPosition.z;
            ntf.velocityX = rigidBody.velocity.x;
            ntf.velocityY = rigidBody.velocity.x;
            ntf.velocityZ = rigidBody.velocity.x;
            session.Send<MsgSvrCli_SyncPosition_Ntf>(ntf);
            */
        }
    }
}