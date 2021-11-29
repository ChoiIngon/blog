using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace UnityServer
{
    public class Client : MonoBehaviour
    {
        public Gamnet.Client.Session session;
        public Button btnConnect;
        public Button btnClose;
        public GameObject sphere;
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
                session.RegisterHandler<Packet.Packet.MsgSvrCli_CreateCube_Ans>(Packet.Packet.MsgSvrCli_CreateCube_Ans.MSG_ID, (Packet.Packet.MsgSvrCli_CreateCube_Ans ans) =>
                {
                    session.UnregisterHandler(Packet.Packet.MsgSvrCli_CreateCube_Ans.MSG_ID);
                });

                session.RegisterHandler<Packet.Packet.MsgSvrCli_SyncPosition_Ntf>(Packet.Packet.MsgSvrCli_SyncPosition_Ntf.MSG_ID, (Packet.Packet.MsgSvrCli_SyncPosition_Ntf ntf) =>
                {
                    sphere.transform.position = new Vector3(sphere.transform.position.x, ntf.y, sphere.transform.position.z);
                    syncPacketCount++;
                });

                session.OnConnectEvent += () =>
                {
                    Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
                    CreateSphereReq();
                };

                session.OnErrorEvent += (System.Exception e) =>
                {
                    Debug.Log(e.Message + "\n" + e.StackTrace.ToString());
                };

                session.OnCloseEvent += () =>
                {
                    session.UnregisterHandler(Packet.Packet.MsgSvrCli_CreateCube_Ans.MSG_ID);
                    session.UnregisterHandler(Packet.Packet.MsgSvrCli_SyncPosition_Ntf.MSG_ID);
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

        public void CreateSphereReq()
        {
            Packet.Packet.MsgCliSvr_CreateSphereReq req = new Packet.Packet.MsgCliSvr_CreateSphereReq();
            Send<Packet.Packet.MsgCliSvr_CreateSphereReq>(req);
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
