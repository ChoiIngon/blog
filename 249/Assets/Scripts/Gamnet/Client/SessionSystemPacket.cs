using Gamnet.SystemPacket;
using System.Collections.Generic;
using UnityEngine;

namespace Gamnet.Client
{
    public partial class Session : Gamnet.Session
    {
        private void Send_EstablishSessionLink_Req()
        {
            // Debug.Log($"{Util.Debug.__FUNC__()}");
            SystemPacket.MsgCliSvr_EstablishSessionLink_Req req = new SystemPacket.MsgCliSvr_EstablishSessionLink_Req();

            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = SystemPacket.MsgCliSvr_EstablishSessionLink_Req.MSG_ID;
            packet.Serialize(req);
            Send(packet);
        }

        private void Recv_EstabilshSessionLink_Ans(MsgSvrCli_EstablishSessionLink_Ans ans)
        {
            // Debug.Log($"{Util.Debug.__FUNC__()}");
            if (0 != ans.error_code)
            {
                Debug.LogError("connect fail(error_code:" + ans.error_code + ")");
                Error(null);
                return;
            }

            session_key = ans.session_key;
            session_token = ans.session_token;
            link_establish = true;
            OnConnect();
        }

        private void Send_RecoverSessionLink_Req()
        {
            // Debug.Log($"{Util.Debug.__FUNC__()}");
            SystemPacket.MsgCliSvr_RecoverSessionLink_Req req = new SystemPacket.MsgCliSvr_RecoverSessionLink_Req();
            req.session_key = session_key;
            req.session_token = session_token;

            List<Packet> unsendPacketQueue = new List<Packet>();
            foreach (Packet unsentPacket in send_queue)
            {
                unsendPacketQueue.Add(unsentPacket);
            }

            send_queue_index = 0;
            send_queue.Clear();

            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = SystemPacket.MsgCliSvr_RecoverSessionLink_Req.MSG_ID;
            packet.Serialize(req);
            Send(packet);

            foreach (Packet unsentPacket in unsendPacketQueue)
            {
                Send(unsentPacket);
            }
        }

        private void Recv_RecoverSessionLink_Ans(MsgSvrCli_RecoverSessionLink_Ans ans)
        {
            // Debug.Log($"{Util.Debug.__FUNC__()}");
            if (0 != ans.error_code)
            {
                Debug.LogError("connect fail(error_code:" + ans.error_code + ")");
                Error(null);
                return;
            }

            OnResume();
        }

        private void Send_DestroySessionLink_Req()
        {
            // Debug.Log($"{Util.Debug.__FUNC__()}");
            if (false == socket.Connected)
            {
                return;
            }

            link_establish = false;
            SystemPacket.MsgCliSvr_DestroySessionLink_Req req = new SystemPacket.MsgCliSvr_DestroySessionLink_Req();

            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = SystemPacket.MsgCliSvr_DestroySessionLink_Req.MSG_ID;
            packet.Serialize(req);
            Send(packet);
        }

        private void Recv_DestroySessionLink_Ans(MsgSvrCli_DestroySessionLink_Ans ans)
        {
            // Debug.Log($"{Util.Debug.__FUNC__()}");
            if (0 != ans.error_code)
            {
                Debug.LogError("MsgSvrCli_DestroySessionLink_Ans(error_code:" + ans.error_code + ")");
                return;
            }

            socket.Close();
            OnClose();
        }

        void Recv_HeartBeat_Req(MsgSvrCli_HeartBeat_Req req)
        {
            RemoveSentPacket(req.recv_seq);

            SystemPacket.MsgCliSvr_HeartBeat_Ans ans = new SystemPacket.MsgCliSvr_HeartBeat_Ans();
            ans.recv_seq = recv_seq;
            ans.date_time = req.date_time;
            ans.error_code = 0;

            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = SystemPacket.MsgCliSvr_HeartBeat_Ans.MSG_ID;
            packet.Serialize(ans);
            Send(packet);
        }

        void Recv_ReliableAck_Ntf(Msg_ReliableAck_Ntf ntf)
        {
            try
            {
                RemoveSentPacket(ntf.recv_seq);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }
}