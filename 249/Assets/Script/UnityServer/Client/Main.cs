using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityServer.Common.Packet;

namespace UnityServer.Client
{
    public class Main : Gamnet.Util.MonoSingleton<Main>
    {
        public Gamnet.Client.Session session;

        public Button btnConnect;
        public Button btnClose;

        public GameObject spherePrefab;

        private Transform room;
        private Dictionary<uint, Common.Sphere> spheres = new Dictionary<uint, Common.Sphere>();

        private void Start()
        {
            btnConnect.onClick.AddListener(() =>
            {
                if (null != session)
                {
                    session.Close();
                }
                session = new Gamnet.Client.Session();

                session.OnConnectEvent += () =>
                {
                    session.RegisterHandler<MsgSvrCli_CreateRoom_Ans>(MsgSvrCli_CreateRoom_Ans.MSG_ID, (MsgSvrCli_CreateRoom_Ans ans) => {});
                    session.RegisterHandler<MsgSvrCli_CreateSphere_Ntf>(MsgSvrCli_CreateSphere_Ntf.MSG_ID, OnCreateSphereNtf);
                    session.RegisterHandler<MsgSvrCli_SyncPosition_Ntf>(MsgSvrCli_SyncPosition_Ntf.MSG_ID, OnSyncPositionNtf);

                    MsgCliSvr_CreateRoom_Req req = new MsgCliSvr_CreateRoom_Req();
                    Send<MsgCliSvr_CreateRoom_Req>(req);
                };

                session.OnErrorEvent += (System.Exception e) =>
                {
                    Debug.Log(e.Message + "\n" + e.StackTrace.ToString());
                };

                session.OnCloseEvent += () =>
                {
                    foreach (var itr in spheres)
                    {
                        Common.Sphere sphere = itr.Value;
                        sphere.transform.SetParent(null);
                        GameObject.Destroy(sphere.gameObject);
                    }
                    spheres.Clear();

                    session.UnregisterHandler(MsgSvrCli_CreateRoom_Ans.MSG_ID);
                    session.UnregisterHandler(MsgSvrCli_CreateSphere_Ntf.MSG_ID);
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

        private void OnDestroy()
        {
            btnConnect.onClick.RemoveAllListeners();
            btnClose.onClick.RemoveAllListeners();
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

        private void OnCreateSphereNtf(MsgSvrCli_CreateSphere_Ntf ntf)
        {
            GameObject go = Instantiate<GameObject>(spherePrefab);
            Common.Sphere sphere = go.AddComponent<Common.Sphere>();
            sphere.gameObject.tag = "Client";
            sphere.gameObject.layer = LayerMask.NameToLayer("Client");
            sphere.rigidBody = sphere.GetComponent<Rigidbody>();
            sphere.id = ntf.id;
            sphere.transform.localPosition = new Vector3(ntf.positionX, ntf.positionY, ntf.positionZ);
            sphere.rigidBody.velocity = new Vector3(ntf.velocityX, ntf.velocityY, ntf.velocityZ);

            Transform sphereTransform = transform.Find("Room/Spheres");
            sphere.transform.SetParent(sphereTransform, false);

            spheres.Add(sphere.id, sphere);
        }

        private void OnSyncPositionNtf(MsgSvrCli_SyncPosition_Ntf ntf)
        {
            Common.Sphere sphere = null;
            if (false == spheres.TryGetValue(ntf.id, out sphere))
            {
                return;
            }

            sphere.transform.localPosition = new Vector3(ntf.positionX, ntf.positionY, ntf.positionZ);
            sphere.transform.rotation = new Quaternion(ntf.rotationX, ntf.rotationY, ntf.rotationZ, ntf.rotationW);
            sphere.rigidBody.velocity = new Vector3(ntf.velocityX, ntf.velocityY, ntf.velocityZ);
        }

        public void Send<MSG_T>(MSG_T msg)
        {
            FieldInfo fieldInfo = msg.GetType().GetField("MSG_ID");
            uint packetId = (uint)fieldInfo.GetValue(msg);
            Gamnet.Packet packet = new Gamnet.Packet();
            packet.Id = packetId;
            packet.Serialize(msg);
            session.Send(packet);
        }
    }
}
