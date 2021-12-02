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
            public Room room;
            protected override void OnConnect()
            {
                Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
            }

            protected override void OnClose()
            {
                if (null != room)
                {
                    room.transform.SetParent(null);
                    GameObject.Destroy(room.gameObject);
                    room = null;
                }

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
        public GameObject RoomPrefab;
        public GameObject SpherePrefab;
        public bool ActivateServer = true;
        void Start()
        {
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
            }
        }

        private void Update()
        {
            Gamnet.Session.EventLoop.Update();
        }

        public Room CreateRoom()
        {
            GameObject go = Instantiate<GameObject>(RoomPrefab);
            Room room = go.AddComponent<Room>();
            return room;
        }

        public Sphere CreateSphere()
        {
            GameObject go = Instantiate<GameObject>(SpherePrefab);
            Sphere sphere = go.AddComponent<Sphere>();
            return sphere;
        }
    }
}
