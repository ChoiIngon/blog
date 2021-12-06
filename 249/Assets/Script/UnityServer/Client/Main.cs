using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityServer.Client.Packet;
using UnityServer.Common.Packet;

namespace UnityServer.Client
{
    public class Main : Gamnet.Util.MonoSingleton<Main>
    {
        private Gamnet.Client.Session session;
        public Dictionary<uint, Common.Sphere> spheres = new Dictionary<uint, Common.Sphere>();
        public bool syncPosition;
        public bool syncRotation;
        public bool syncVelocity;

        public Button btnConnect;
        public Button btnClose;
        public Slider sliderObjectCount;
        public InputField inputObjectCount;
        public Slider sliderSyncInterval;
        public InputField inputSyncInterval;
        public Toggle toggleSync;
        public Toggle toggleSyncPosition;
        public Toggle toggleSyncRotation;
        public Toggle toggleSyncVelocity;
        public Toggle toggleClientOnly;

        public GameObject spherePrefab;

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
                    session.RegisterHandler<MsgSvrCli_CreateRoom_Ans>(MsgSvrCli_CreateRoom_Ans.MSG_ID, CreateRoom.OnReceive);
                    session.RegisterHandler<MsgSvrCli_CreateSphere_Ntf>(MsgSvrCli_CreateSphere_Ntf.MSG_ID, CreateSphere.OnReceive);
                    session.RegisterHandler<MsgSvrCli_SyncPosition_Ntf>(MsgSvrCli_SyncPosition_Ntf.MSG_ID, SuncPosition.OnReceive);

                    MsgCliSvr_CreateRoom_Req req = new MsgCliSvr_CreateRoom_Req();
                    Send(req);
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
        private void Update()
        {
            inputObjectCount.text = Server.Main.Instance.objectCount.ToString();
            Server.Main.Instance.objectCount = (int)sliderObjectCount.value;

            inputSyncInterval.text = Server.Main.Instance.syncInterval.ToString();
            Server.Main.Instance.syncInterval = sliderSyncInterval.value;

            syncPosition = toggleSyncPosition.isOn;
            syncRotation = toggleSyncRotation.isOn;
            syncVelocity = toggleSyncVelocity.isOn;

            Server.Main.Instance.sync = toggleSync.isOn;
            Server.Main.Instance.clientOnly = toggleClientOnly.isOn;

            if (true == Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (true == Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.NameToLayer("Client")))
                {
                    if ("ClientSphere" == hit.transform.gameObject.tag)
                    {
                        Rigidbody rb = hit.transform.GetComponent<Rigidbody>();
                        rb.velocity += ray.direction.normalized * 30.0f;

                        Sphere sphere = hit.transform.GetComponent<Sphere>();
                        MsgCliSvr_HitSphere_Ntf ntf = new MsgCliSvr_HitSphere_Ntf();
                        ntf.id = sphere.id;
                        ntf.hitDirection = ray.direction.normalized;
                        Send(ntf);
                    }
                }
            }
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
