using Gamnet;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityServer.Common.Packet;

namespace UnityServer.Client
{
    public class Main : MonoBehaviour
    {
        public Gamnet.Client.Session session;
        public Button btnConnect;
        public Button btnClose;
        public GameObject spherePrefab;
        private int syncPacketCount;
        public Dictionary<uint, Sphere> spheres = new Dictionary<uint, Sphere>();
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
                GameObject go = Server.Main.Instance.CreateSphere();
                Sphere sphere = go.AddComponent<Sphere>();
                sphere.id = ntf.id;
                sphere.transform.localPosition = new Vector3(ntf.positionX, ntf.positionY, ntf.positionZ);
                //sphere.rigidBody.velocity = new Vector3(ntf.velocityX, ntf.velocityY, ntf.velocityZ);
                sphere.transform.SetParent(transform, false);

                spheres.Add(sphere.id, sphere);
            });

            session.RegisterHandler<MsgSvrCli_SyncPosition_Ntf>(MsgSvrCli_SyncPosition_Ntf.MSG_ID, (MsgSvrCli_SyncPosition_Ntf ntf) =>
            {
                Sphere sphere = null;
                if (false == spheres.TryGetValue(ntf.id, out sphere))
                {
                    return;
                }

                sphere.transform.localPosition = new Vector3(ntf.positionX, ntf.positionY, ntf.positionZ);
                sphere.rigidBody.velocity = new Vector3(ntf.velocityX, ntf.velocityY, ntf.velocityZ);
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
