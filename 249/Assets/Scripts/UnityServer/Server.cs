using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityServer
{
    public class Server : MonoBehaviour
    {
        public class Session : Gamnet.Server.Session
        {
            protected override void OnCreate()
            {
            }

            protected override void OnAccept()
            {
            }

            protected override void OnClose()
            {
                Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, "server session closed");
            }

            protected override void OnDestory()
            {
                Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, "server session destroyed");
            }

            protected override void OnError(Exception e)
            {
            }
        }

        private Gamnet.Server.Acceptor<Session> acceptor = new Gamnet.Server.Acceptor<Session>();

        public int Port;
        public int MaxSessionCount;

        void Start()
        {
            Gamnet.Log.Init("log", "UnityServer", 1);
            acceptor.Init(Port, MaxSessionCount);
            Gamnet.Simulation.Simulator simulator = GetComponent<Gamnet.Simulation.Simulator>();
            if (null != simulator && true == simulator.enabled)
            {
                simulator.Init<SimulationClient>();
            }
        }

        private void Update()
        {
            Gamnet.Session.EventLoop.Update();
        }
    }
}
