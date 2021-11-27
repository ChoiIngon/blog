using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace UnityServer
{
    public class Client : MonoBehaviour
    {
        public Gamnet.Client.Session session;
        public Button connect;
        public Button create;
        public GameObject sphere;
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
            connect.onClick.AddListener(() =>
            {
                if (null != session)
                {
                    session.Close();
                }

                session = new Gamnet.Client.Session();
                session.OnConnectEvent += OnConnect;
                session.OnCloseEvent += OnClose;
                session.OnPauseEvent += OnPause;
                session.OnResumeEvent += OnResume;

                session.RegisterHandler<Packet.Packet.MsgSvrCli_SyncPosition_Ntf>(Packet.Packet.MsgSvrCli_SyncPosition_Ntf.MSG_ID, (Packet.Packet.MsgSvrCli_SyncPosition_Ntf ntf) =>
                {
                    sphere.transform.position = new Vector3(sphere.transform.position.x, ntf.y, sphere.transform.position.z);
                });

                session.AsyncConnect("127.0.0.1", 4000);
            });
        }

        public void SendCreateReq()
        {
            Packet.Packet.MsgCliSvr_CreateCube_Req req = new Packet.Packet.MsgCliSvr_CreateCube_Req();
            Send<Packet.Packet.MsgCliSvr_CreateCube_Req>(req);

            session.RegisterHandler<Packet.Packet.MsgSvrCli_CreateCube_Ans>(Packet.Packet.MsgSvrCli_CreateCube_Ans.MSG_ID, (Packet.Packet.MsgSvrCli_CreateCube_Ans ans) =>
            {
                session.UnregisterHandler(Packet.Packet.MsgSvrCli_CreateCube_Ans.MSG_ID);
            });
        }

        private void OnConnect()
        {
            Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
            SendCreateReq();
        }
        private void OnClose()
        {
            Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
        }
        private void OnPause()
        {
            Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
        }
        private void OnResume()
        {
            Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
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
