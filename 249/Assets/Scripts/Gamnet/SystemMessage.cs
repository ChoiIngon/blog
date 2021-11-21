using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamnet.SystemMessage
{
    /*

        const uint MsgID_CliSvr_Reconnect_Req = MsgID_Max - 2;
        const uint MsgID_SvrCli_Reconnect_Ans = MsgID_Max - 2;
        const uint MsgID_CliSvr_HeartBeat_Req = MsgID_Max - 3;
        const uint MsgID_SvrCli_HeartBeat_Ans = MsgID_Max - 3;
        const uint MsgID_CliSvr_ReliableAck_Ntf = MsgID_Max - 4;
        const uint MsgID_SvrCli_ReliableAck_Ntf = MsgID_Max - 4;
        const uint MsgID_SvrCli_Kickout_Ntf = MsgID_Max - 5;
    */
    [System.Serializable]
    class MsgCliSvr_Connect_Req
    {
        public const uint MSG_ID = uint.MaxValue - 1;
    }

    [System.Serializable]
    class MsgSvrCli_Connect_Ans
    {
        public const uint MSG_ID = uint.MaxValue - 1;

        public int error_code = 0;
        public uint session_key = 0;
        public string session_token = "";
    }

    [System.Serializable]
    class MsgCliSvr_Close_Req
    {
        public const uint MSG_ID = uint.MaxValue - 2;
    }

    [System.Serializable]
    class MsgSvrCli_Close_Ans
    {
        public const uint MSG_ID = uint.MaxValue - 2;
    }

    [System.Serializable]
    class MsgCliSvr_Reconnect_Req
    {
        public const uint MSG_ID = uint.MaxValue - 3;
    }

    [System.Serializable]
    class MsgSvrCli_Reconnect_Ans
    {
        public const uint MSG_ID = uint.MaxValue - 3;
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
