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
        public void EnableHandOver(bool flag)
        {
            SystemPacket.MsgCliSvr_EnableHandOver_Req req = new SystemPacket.MsgCliSvr_EnableHandOver_Req();
            req.flag = flag;

            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = SystemPacket.MsgCliSvr_EnableHandOver_Req.MSG_ID;
            packet.Serialize(req);
            AsyncSend(packet);
        }

        void OnReceive_EnableHandOver_Ans(MsgSvrCli_EnableHandOver_Ans ans)
        {
            if (0 != ans.error_code)
            {
                Debug.LogError("connect fail(error_code:" + ans.error_code + ")");
                Error(null);
                return;
            }
        }

        void Recv_Close_Ans(MsgSvrCli_Close_Ans ans)
        {
            if (0 != ans.error_code)
            {
                Debug.LogError("connect fail(error_code:" + ans.error_code + ")");
                Error(null);
                return;
            }

            //session_token = ans.session_token;

            CloseEvent evt = new CloseEvent(this);
            Session.EventLoop.EnqueuEvent(evt); // already locked
        }

        void Recv_Reconnect_Ans(MsgSvrCli_Reconnect_Ans ans)
        {
            if (0 != ans.error_code)
            {
                Debug.LogError("connect fail(error_code:" + ans.error_code + ")");
                Error(null);
                return;
            }

            //session_token = ans.session_token;
            OnResume();
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