using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamnet
{
    public class ServerTest<T> where T : Gamnet.Client
    {
        public string Host;
        public int Port;
        public int SessionCount;
        public int LoopCount;
        public Dictionary<string, Action<T>> testcases = new Dictionary<string, Action<T>>();
        private Dictionary<uint, T> clients = new Dictionary<uint, T>();


        public void Run()
        {
            for (int i = 0; i < SessionCount; i++)
            {
                GameObject go = new GameObject();
                T client = go.AddComponent<T>();
                client.session = new Gamnet.ClientSession();
                clients.Add(client.session.session_key, client);
                /*
                client.session.RegisterHandler<.Message>(1, (Assets.Scripts.Message msg) =>
                {
                    Packet p = new Packet();
                    p.Id = 2;
                    p.Serialize(msg);
                    client.session.AsyncSend(p);
                });

                client.session.OnConnectEvent += () =>
                {
                    Assets.Scripts.Message message = new Assets.Scripts.Message();

                    message.greeting = "Hello World";
                    Packet req = new Packet();
                    req.Id = 1;
                    req.Serialize(message);
                    client.session.AsyncSend(req);
                };
                */
                client.session.AsyncConnect(Host, Port);
            }
        }
    }
}
