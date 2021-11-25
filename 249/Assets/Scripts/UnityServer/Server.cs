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
                Debug.Log($"{Gamnet.Util.Debug.__FUNC__()}");
            }

            protected override void OnClose()
            {
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
        }

        private Gamnet.Server.Acceptor<Session> acceptor = new Gamnet.Server.Acceptor<Session>();

        public int Port;
        public int MaxSessionCount;

        void Start()
        {
            Gamnet.Util.Debug.Init();
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
