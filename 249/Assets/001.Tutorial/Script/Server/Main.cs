using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityServer.Common.Packet;

namespace UnityServer.Server
{
    public class Main : Gamnet.Util.MonoSingleton<Main>
    {
        private Dictionary<uint, Session> sessions = new Dictionary<uint, Session>();
        public class Session : Gamnet.Server.Session
        {
            public GameObject room;
            public Dictionary<uint, Sphere> spheres = new Dictionary<uint, Sphere>();
            private float deltaTime;
            protected override void OnConnect()
            {
                Main.Instance.sessions.Add(session_key, this);
                deltaTime = 0;
            }

            protected override void OnClose()
            {
                foreach (var itr in spheres)
                {
                    var sphere = itr.Value;
                    sphere.transform.SetParent(null);
                    GameObject.Destroy(sphere.gameObject);
                }
                spheres.Clear();

                if (null != room)
                {
                    room.transform.SetParent(null);
                    GameObject.Destroy(room.gameObject);
                    room = null;
                }

                Main.Instance.sessions.Remove(session_key);
            }

            protected override void OnResume()
            {
            }

            protected override void OnPause()
            {
            }

            public void Update()
            {
                if (null == this.room)
                {
                    return;
                }

                deltaTime += Time.deltaTime;
                if (Server.Main.Instance.syncInterval <= deltaTime && true == Server.Main.Instance.sync)
                {
                    MsgSvrCli_SyncPosition_Ntf ntf = new MsgSvrCli_SyncPosition_Ntf();
                    ntf.transforms = new List<ObjectTransform>();
                    foreach (var itr in spheres)
                    {
                        var sphere = itr.Value;
                        if (false == sphere.gameObject.activeSelf)
                        {
                            continue;
                        }

                        ObjectTransform objTrans = new ObjectTransform();
                        objTrans.id = sphere.id;
                        objTrans.localPosition = sphere.transform.localPosition;
                        objTrans.rotation = sphere.transform.rotation;
                        objTrans.velocity = sphere.rigidBody.velocity;
                        ntf.transforms.Add(objTrans);
                    }
                    Send<MsgSvrCli_SyncPosition_Ntf>(ntf);

                    deltaTime -= Server.Main.Instance.syncInterval;
                }
            }

            public void Send<MSG_T>(MSG_T msg)
            {
                FieldInfo fieldInfo = msg.GetType().GetField("MSG_ID");
                uint packetId = (uint)fieldInfo.GetValue(msg);
                Gamnet.Packet packet = new Gamnet.Packet();
                packet.Id = packetId;
                packet.Serialize(msg);
                base.Send(packet);
            }
        }

        private Gamnet.Server.Acceptor<Session> acceptor = new Gamnet.Server.Acceptor<Session>();
        public GameObject roomPrefab;
        public GameObject spherePrefab;

        public int Port;
        public int MaxSessionCount;

        public int objectCount = 10;
        public bool sync = true;

        public bool clientOnly = false;

        [Range(0.1f, 1.0f)]
        public float syncInterval = 0.1f;

        void Start()
        {
            Gamnet.Util.Debug.Init();
            Gamnet.Log.Init("log", "UnityServer", 1);
            if (false == clientOnly)
            {
                acceptor.Init(Port, MaxSessionCount);
                Gamnet.Simulation.Simulator simulator = GetComponent<Gamnet.Simulation.Simulator>();
                if (null != simulator && true == simulator.enabled)
                {
                    simulator.Init<SimulationClient>();
                }
            }
        }

        private void Update()
        {
            Gamnet.Session.EventLoop.Update();
            foreach (var itr in sessions)
            {
                itr.Value.Update();
            }
        }
    }
}
