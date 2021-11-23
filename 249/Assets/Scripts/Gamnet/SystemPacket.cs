using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamnet.SystemPacket
{
    enum ErrorCode
    {
        Success
    }
    [System.Serializable]
    class MsgCliSvr_EstablishSessionLink_Req
    {
        public const uint MSG_ID = uint.MaxValue - 1;
    }

    [System.Serializable]
    class MsgSvrCli_EstablishSessionLink_Ans
    {
        public const uint MSG_ID = uint.MaxValue - 1;

        public int error_code = 0;
        public uint session_key = 0;
        public string session_token = "";
    }

    [System.Serializable]
    class MsgCliSvr_DestroySessionLink_Req
    {
        public const uint MSG_ID = uint.MaxValue - 2;
    }

    [System.Serializable]
    class MsgSvrCli_DestroySessionLink_Ans
    {
        public const uint MSG_ID = uint.MaxValue - 2;
        public int error_code = 0;
    }

    [System.Serializable]
    class MsgCliSvr_RecoverSessionLink_Req
    {
        public const uint MSG_ID = uint.MaxValue - 3;

        public uint session_key;
        public string session_token;
    }

    [System.Serializable]
    class MsgSvrCli_RecoverSessionLink_Ans
    {
        public const uint MSG_ID = uint.MaxValue - 3;
        public int error_code = 0;
    }

    [System.Serializable]
    class MsgCliSvr_HeartBeat_Req
    {
        public const uint MSG_ID = uint.MaxValue - 4;
    }

    [System.Serializable]
    class MsgSvrCli_HeartBeat_Ans
    {
        public const uint MSG_ID = uint.MaxValue - 4;
        public int error_code = 0;
    }

    [System.Serializable]
    class MsgCliSvr_ReliableAck_Ntf
    {
        public const uint MSG_ID = uint.MaxValue - 5;
        public uint ack_seq;
    }

    [System.Serializable]
    class MsgSvrCli_ReliableAck_Ntf
    {
        public const uint MSG_ID = uint.MaxValue - 5;
        public uint ack_seq;
    }
}
