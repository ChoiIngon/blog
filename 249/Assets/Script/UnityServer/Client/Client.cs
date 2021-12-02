using Gamnet;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityServer.Common.Packet;

namespace UnityServer
{
    public class Client : MonoBehaviour
    {
        public Gamnet.Client.Session session;
        public Button btnConnect;
        public Button btnClose;
        public GameObject spherePrefab;
        private int syncPacketCount;
        public void Send<MSG_T>(MSG_T msg)
        {
            FieldInfo fieldInfo = msg.GetType().GetField("MSG_ID");
            uint packetId = (uint)fieldInfo.GetValue(msg);

            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = packetId;
            packet.Serialize(msg);
            session.Send(packet);
        }

        private void Start()
        {
            btnConnect.onClick.AddListener(() =>
            {
                if (null != session)
                {
                    session.Close();
                }
                session = new Gamnet.Client.Session();
                syncPacketCount = 0;
                InvokeRepeating("OnTimerExpire", 0, 5);

                session.OnConnectEvent += () =>
                {
                    Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
                    CreateRoomReq();
                };

                session.OnErrorEvent += (System.Exception e) =>
                {
                    Debug.Log(e.Message + "\n" + e.StackTrace.ToString());
                };

                session.OnCloseEvent += () =>
                {
                    session.UnregisterHandler(MsgSvrCli_CreateRoom_Ans.MSG_ID);
                    session.UnregisterHandler(MsgSvrCli_SyncPosition_Ntf.MSG_ID);
                    session.OnConnectEvent = null;
                    session.OnErrorEvent = null;
                    session.OnCloseEvent = null;
                };
                session.AsyncConnect("127.0.0.1", 4000);
            });

            btnClose.onClick.AddListener(() =>
            {
                session.Close();
            });
        }

        public void OnTimerExpire()
        {
            Debug.Log($"recv:{syncPacketCount/5}");
            syncPacketCount = 0;
        }
        private void OnDestroy()
        {
            btnConnect.onClick.RemoveAllListeners();
            btnClose.onClick.RemoveAllListeners();
        }

        public void CreateRoomReq()
        {
            MsgCliSvr_CreateRoom_Req req = new MsgCliSvr_CreateRoom_Req();
            Send<MsgCliSvr_CreateRoom_Req>(req);

            session.RegisterHandler<MsgSvrCli_CreateRoom_Ans>(MsgSvrCli_CreateRoom_Ans.MSG_ID, (MsgSvrCli_CreateRoom_Ans ans) =>
            {
                session.UnregisterHandler(MsgSvrCli_CreateRoom_Ans.MSG_ID);
            });

            session.RegisterHandler<MsgSvrCli_CreateSphere_Ntf>(MsgSvrCli_CreateSphere_Ntf.MSG_ID, (MsgSvrCli_CreateSphere_Ntf ntf) =>
            {

            });

            session.RegisterHandler<MsgSvrCli_SyncPosition_Ntf>(MsgSvrCli_SyncPosition_Ntf.MSG_ID, (MsgSvrCli_SyncPosition_Ntf ntf) =>
            {
                syncPacketCount++;
            });
        }

        private void OnApplicationPause(bool pause)
        {
            if (null == session)
            {
                return;
            }
            if (true == pause)
            {
                session.Pause();
            }
            else
            {
                session.Resume();
            }
        }
    }
}
