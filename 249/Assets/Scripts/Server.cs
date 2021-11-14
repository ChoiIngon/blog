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
        Gamnet.Server<Gamnet.ServerSession> server = new Gamnet.Server<Gamnet.ServerSession>();

        void Start()
        {
            Gamnet.Log.Init("log", "UnityServer", 1);
            server.Listen(4000, 8000);
        }

        private void Update()
        {
            server.Update();
        }
    }
}
