using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Server : MonoBehaviour
    {
        Gamnet.Server.Acceptor<Session> acceptor = new Gamnet.Server.Acceptor<Session>();

        void Start()
        {
            Gamnet.Log.Init("log", "UnityServer", 1);
            acceptor.Init(4000, 8000);

            Assets.Scripts.Simulator test = GetComponent<Assets.Scripts.Simulator>();
            if (null != test)
            {
                Gamnet.Simulation.Simulator.Init<Assets.Scripts.Simulator.Client>("127.0.0.1", 4000, 1, 1);
            }
        }

        private void Update()
        {
            Gamnet.EventLoop.Update();
        }
    }
}
