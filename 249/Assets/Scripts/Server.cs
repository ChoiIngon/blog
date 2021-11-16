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

        Gamnet.ServerTest<Test.Client> test = new Gamnet.ServerTest<Test.Client>();
        void Start()
        {
            Gamnet.Log.Init("log", "UnityServer", 1);
            acceptor.Init(4000, 8000);

            test.Host = "127.0.0.1";
            test.Port = 4000;
            test.SessionCount = 1;
            test.LoopCount = 1;
            test.Init();
            test.Run();
        }

        private void Update()
        {
            Gamnet.EventLoop.Update();
        }
    }
}
