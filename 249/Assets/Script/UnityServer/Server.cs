using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityServer
{
    public class Server : Gamnet.Util.MonoSingleton<Server>
    {
        public class Session : Gamnet.Server.Session
        {
            public UnityServer.Agent Agent;
            protected override void OnConnect()
            {
                GameObject go = new GameObject();
                Agent = go.AddComponent<UnityServer.Agent>();
                Agent.name = $"Agent_{session_key}";
                Agent.session = this;
                Agent.transform.SetParent(Server.Instance.transform, false);
                Agent.transform.localPosition = new Vector3(1, 0, 0);

                Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
            }

            protected override void OnClose()
            {
                Agent.transform.SetParent(null);
                GameObject.Destroy(Agent.gameObject);
                Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
            }

            protected override void OnResume()
            {
                Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
            }

            protected override void OnPause()
            {
                Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
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

        public int Port;
        public int MaxSessionCount;
        public GameObject SpherePrefab;
        public bool ActivateServer = true;
        void Start()
        {
            List<Gamnet.Util.SharedDisposable<Gamnet.Packet>> sharedPacketList = new List<Gamnet.Util.SharedDisposable<Gamnet.Packet>>();
            Gamnet.Util.Debug.Init();
            Gamnet.Log.Init("log", "UnityServer", 1);
            if (true == ActivateServer)
            {
                acceptor.Init(Port, MaxSessionCount);
                Gamnet.Simulation.Simulator simulator = GetComponent<Gamnet.Simulation.Simulator>();
                if (null != simulator && true == simulator.enabled)
                {
                    simulator.Init<SimulationClient>();
                }

                using (Gamnet.Util.SharedDisposable<Gamnet.Packet> sharedPacket = new Gamnet.Util.SharedDisposable<Gamnet.Packet>(new Gamnet.Packet()))
                {
                    sharedPacketList.Add(sharedPacket.Share());
                };

                Gamnet.Packet packet = sharedPacketList[0];
                Debug.Log(packet.Id);
                sharedPacketList.Clear();
            }
        }

        private void Update()
        {
            Gamnet.Session.EventLoop.Update();
        }

        public GameObject CreateSphere()
        {
            return Instantiate<GameObject>(SpherePrefab);
        }
    }
}
