using System;

namespace UnityServer.Common.Packet
{
    [Serializable]
    public class MsgCliSvr_Greeting_Req
    {
        public const uint MSG_ID = 00000001;
        public string text;
    }

    [Serializable]
    public class MsgSvrCli_Greeting_Ans
    {
        public const uint MSG_ID = 00000001;
        public string text;
    }

    [Serializable]
    public class MsgCliSvr_Greeting_Ntf
    {
        public const uint MSG_ID = 00000002;
        public string text;
    }

    [Serializable]
    public class MsgCliSvr_CreateRoom_Req
    {
        public const uint MSG_ID = 00000003;
    }

    [Serializable]
    public class MsgSvrCli_CreateRoom_Ans
    {
        public const uint MSG_ID = 00000003;
    }

    [Serializable]
    public class MsgSvrCli_CreateSphere_Ntf
    {
        public const uint MSG_ID = 00000004;
        public uint id;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float velocityX;
        public float velocityY;
        public float velocityZ;
    }

    [Serializable]
    public class MsgSvrCli_SyncPosition_Ntf
    {
        public const uint MSG_ID = 00000005;
        public uint id;
        public float positionX;
        public float positionY;
        public float positionZ;
        public float rotationX;
        public float rotationY;
        public float rotationW;
        public float rotationZ;
        public float velocityX;
        public float velocityY;
        public float velocityZ;
    }
}