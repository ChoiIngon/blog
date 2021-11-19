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
            protected override void OnClose()
            {
                Gamnet.Log.Write(Gamnet.Log.LogLevel.DEV, "server session closed");
            }

            protected override void OnError(Exception e)
            {

            }
        }

        Gamnet.Server.Acceptor<Session> acceptor = new Gamnet.Server.Acceptor<Session>();

        void Start()
        {
            Gamnet.Log.Init("log", "UnityServer", 1);
            acceptor.Init(4000, 8000);

            Simulator simulator = GetComponent<Simulator>();
            simulator?.Init();
        }

        private void Update()
        {
            Gamnet.EventLoop.Update();
        }
    }
}
