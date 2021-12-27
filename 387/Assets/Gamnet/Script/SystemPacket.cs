using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamnet.SystemPacket
{
    [System.Serializable]
    class MsgCliSvr_EstablishSessionLink_Req
    {
        public const uint MSG_ID = uint.MaxValue - 0; // 4294967295
    }

    [System.Serializable]
    class MsgSvrCli_EstablishSessionLink_Ans
    {
        public const uint MSG_ID = uint.MaxValue - 0; // 4294967295

        public int error_code = 0;
        public uint session_key = 0;
        public string session_token = "";
    }

    [System.Serializable]
    class MsgCliSvr_DestroySessionLink_Ntf
    {
        public const uint MSG_ID = uint.MaxValue - 1; // 4294967294
    }

    [System.Serializable]
    class MsgCliSvr_RecoverSessionLink_Req
    {
        public const uint MSG_ID = uint.MaxValue - 2; // 4294967293

        public uint session_key;
        public string session_token;
    }

    [System.Serializable]
    class MsgSvrCli_RecoverSessionLink_Ans
    {
        public const uint MSG_ID = uint.MaxValue - 2; // 4294967293
        public int error_code = 0;
    }

    [System.Serializable]
    class MsgCliSvr_HeartBeat_Req
    {
        public const uint MSG_ID = uint.MaxValue - 3; // 4294967292
        public uint recv_seq;
        public DateTime date_time;
    }

    [System.Serializable]
    class MsgSvrCli_HeartBeat_Ans
    {
        public const uint MSG_ID = uint.MaxValue - 3; // 4294967292
        public uint recv_seq;
        public DateTime date_time;
    }

    [System.Serializable]
    class Msg_ReliableAck_Ntf
    {
        public const uint MSG_ID = uint.MaxValue - 4; // 4294967291
        public uint recv_seq;
    }
}
