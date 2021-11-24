using Gamnet.SystemPacket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamnet.Client
{
    public partial class Session : Gamnet.Session
    {
        private void Send_EstablishSessionLink_Req()
        {
            SystemPacket.MsgCliSvr_EstablishSessionLink_Req req = new SystemPacket.MsgCliSvr_EstablishSessionLink_Req();

            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = SystemPacket.MsgCliSvr_EstablishSessionLink_Req.MSG_ID;
            packet.Serialize(req);
            Send(packet);
        }

        private void Recv_EstabilshSessionLink_Ans(MsgSvrCli_EstablishSessionLink_Ans ans)
        {
            if (0 != ans.error_code)
            {
                Debug.LogError("connect fail(error_code:" + ans.error_code + ")");
                Error(null);
                return;
            }

            session_key = ans.session_key;
            session_token = ans.session_token;
            establish_link = true;
            OnConnect();
        }
        private void Send_RecoverSessionLink_Req()
        {
            SystemPacket.MsgCliSvr_RecoverSessionLink_Req req = new SystemPacket.MsgCliSvr_RecoverSessionLink_Req();
            req.session_key = session_key;
            req.session_token = session_token;

            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = SystemPacket.MsgCliSvr_RecoverSessionLink_Req.MSG_ID;
            packet.Serialize(req);
            Send(packet);
        }

        private void Recv_RecoverSessionLink_Ans(MsgSvrCli_RecoverSessionLink_Ans ans)
        {
            if (0 != ans.error_code)
            {
                Debug.LogError("connect fail(error_code:" + ans.error_code + ")");
                Error(null);
                return;
            }

            establish_link = true;
            OnResume();
        }

        private void Send_DestroySessionLink_Req()
        {
            if (false == socket.Connected)
            {
                return;
            }

            establish_link = false;
            SystemPacket.MsgCliSvr_DestroySessionLink_Req req = new SystemPacket.MsgCliSvr_DestroySessionLink_Req();

            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = SystemPacket.MsgCliSvr_DestroySessionLink_Req.MSG_ID;
            packet.Serialize(req);
            Send(packet);
        }

        private void Recv_DestroySessionLink_Ans(MsgSvrCli_DestroySessionLink_Ans ans)
        {
            if (0 != ans.error_code)
            {
                Debug.LogError("connect fail(error_code:" + ans.error_code + ")");
                Error(null);
                return;
            }

            SocketClose();
        }
        void Recv_HeartBeat_Ans(MsgSvrCli_HeartBeat_Ans ans)
        {
            if (0 != ans.error_code)
            {
                Debug.LogError("connect fail(error_code:" + ans.error_code + ")");
                Error(null);
                return;
            }

            //session_token = ans.session_token;
        }

        void Recv_ReliableAck_Ntf(MsgSvrCli_ReliableAck_Ntf ntf)
        {
        }
    }
}