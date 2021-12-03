using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityServer.Server
{
    public class Main : Gamnet.Util.MonoSingleton<Main>
    {
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

        public GameObject CreateSphere()
        {
            GameObject sphere = Instantiate<GameObject>(SpherePrefab);
            return sphere;
        }
    }
}
