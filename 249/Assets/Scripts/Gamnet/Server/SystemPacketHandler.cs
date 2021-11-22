using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamnet.Server.SystemPacketHandler
{
    class PacketHandler_Connect<SESSION_T> : PacketHandler<SESSION_T> where SESSION_T : Server.Session
    {
        public override uint Id()
        {
            return Gamnet.SystemPacket.MsgCliSvr_Connect_Req.MSG_ID;
        }

        public override IEnumerator OnReceive(SESSION_T session, Gamnet.Packet packet)
        {
            Gamnet.SystemPacket.MsgCliSvr_Connect_Req req = packet.Deserialize<Gamnet.SystemPacket.MsgCliSvr_Connect_Req>();
            Gamnet.SystemPacket.MsgSvrCli_Connect_Ans ans = new Gamnet.SystemPacket.MsgSvrCli_Connect_Ans();
            ans.error_code = 0;
            try
            {
                if (string.Empty != session.session_token)
                {
                    throw new System.Exception();
                }

                session.session_token = System.Guid.NewGuid().ToString();
                Session.SessionEvent evt = new Session.AcceptEvent(session);
                Session.EventLoop.EnqueuEvent(evt);

                ans.session_key = session.session_key;
                ans.session_token = session.session_token;
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            yield break;
        }
    }

    class PacketHandler_Reconnect<SESSION_T> : PacketHandler<SESSION_T> where SESSION_T : Server.Session
    {
        public override uint Id()
        {
            return Gamnet.SystemPacket.MsgCliSvr_Reconnect_Req.MSG_ID;
        }

        public override IEnumerator OnReceive(SESSION_T session, Gamnet.Packet packet)
        {
            Gamnet.SystemPacket.MsgCliSvr_Reconnect_Req req = packet.Deserialize<Gamnet.SystemPacket.MsgCliSvr_Reconnect_Req>();
            Gamnet.SystemPacket.MsgSvrCli_Reconnect_Ans ans = new Gamnet.SystemPacket.MsgSvrCli_Reconnect_Ans();
            ans.error_code = 0;
            try
            {
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            yield break;
        }
    }

    class PacketHandler_Close<SESSION_T> : PacketHandler<SESSION_T> where SESSION_T : Server.Session
    {
        public override uint Id()
        {
            return Gamnet.SystemPacket.MsgCliSvr_Close_Req.MSG_ID;
        }

        public override IEnumerator OnReceive(SESSION_T session, Gamnet.Packet packet)
        {
            Gamnet.SystemPacket.MsgCliSvr_Close_Req req = packet.Deserialize<Gamnet.SystemPacket.MsgCliSvr_Close_Req>();
            Gamnet.SystemPacket.MsgSvrCli_Close_Ans ans = new Gamnet.SystemPacket.MsgSvrCli_Close_Ans();
            ans.error_code = 0;
            try
            {
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            yield break;
        }
    }

    class PacketHandler_HeartBeat<SESSION_T> : PacketHandler<SESSION_T> where SESSION_T : Server.Session
    {
        public override uint Id()
        {
            return Gamnet.SystemPacket.MsgCliSvr_HeartBeat_Req.MSG_ID;
        }

        public override IEnumerator OnReceive(SESSION_T session, Gamnet.Packet packet)
        {
            Gamnet.SystemPacket.MsgCliSvr_HeartBeat_Req req = packet.Deserialize<Gamnet.SystemPacket.MsgCliSvr_HeartBeat_Req>();
            Gamnet.SystemPacket.MsgSvrCli_HeartBeat_Ans ans = new Gamnet.SystemPacket.MsgSvrCli_HeartBeat_Ans();
            ans.error_code = 0;
            try
            {
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            yield break;
        }
    }

    class PacketHandler_ReliableAck<SESSION_T> : PacketHandler<SESSION_T> where SESSION_T : Server.Session
    {
        public override uint Id()
        {
            return Gamnet.SystemPacket.MsgSvrCli_ReliableAck_Ntf.MSG_ID;
        }

        public override IEnumerator OnReceive(SESSION_T session, Gamnet.Packet packet)
        {
            Gamnet.SystemPacket.MsgCliSvr_ReliableAck_Ntf req = packet.Deserialize<Gamnet.SystemPacket.MsgCliSvr_ReliableAck_Ntf>();
            try
            {
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            yield break;
        }
    }
}
