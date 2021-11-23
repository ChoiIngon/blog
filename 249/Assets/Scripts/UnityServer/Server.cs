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
            protected override void OnConnect()
            {
                Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, "server session connected");
            }

            protected override void OnClose()
            {
                Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, "server session closed");
            }

            protected override void OnResume()
            {
                Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, "server session resume");
            }

            protected override void OnPause()
            {
                Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, "server session pause");
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
