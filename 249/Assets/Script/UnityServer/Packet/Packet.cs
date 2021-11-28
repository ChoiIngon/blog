using System;

namespace UnityServer.Packet
{
    public class Packet
    {
        [Serializable]
        public class MsgCliSvr_CreateSphereReq
        {
            public const uint MSG_ID = Code.MsgCliSvr_CreateSphere_Req;
        }

        [Serializable]
        public class MsgSvrCli_CreateCube_Ans
        {
            public const uint MSG_ID = Code.MsgSvrCli_CreateCube_Ans;
        }

        [Serializable]
        public class MsgSvrCli_SyncPosition_Ntf
        {
            public const uint MSG_ID = Code.MsgSvrCli_SyncPosition_Ntf;
            public float y;
        }
    }
}