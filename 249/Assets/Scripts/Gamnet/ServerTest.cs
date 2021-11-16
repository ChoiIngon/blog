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
    public class ServerTest<T> where T : Gamnet.Test.Client
    {
        public string Host;
        public int Port;
        public int SessionCount;
        public int LoopCount;
        public Dictionary<string, Action<T>> testcases = new Dictionary<string, Action<T>>();
        private Dictionary<uint, T> clients = new Dictionary<uint, T>();
        public List<Action<T>> executes = new List<Action<T>>();

        public void Init()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] assems = currentDomain.GetAssemblies();

            IEnumerable<Type> childrenTypes = assems.SelectMany(s => s.GetTypes()).Where(p => typeof(Server.IPacketHandler).IsAssignableFrom(p) && p.IsClass && false == p.IsAbstract);
            foreach (var type in childrenTypes)
            {
                Debug.Log(type.Name);
                Server.IPacketHandler obj = Activator.CreateInstance(type) as Server.IPacketHandler;

                // 테스트 메소드들 자동 등록
                MethodInfo[] methodInfos = type.GetMethods();
                foreach (MethodInfo methodInfo in methodInfos)
                {
                    IEnumerable<Attribute> attributes = methodInfo.GetCustomAttributes();
                    foreach (Attribute attr in attributes)
                    {
                        if (attr is Server.TestMethod testMethod)
                        {
                            Action<T> action = (Action<T>)Delegate.CreateDelegate(typeof(Action<T>), obj, methodInfo);
                            testcases.Add(methodInfo.Name, action);
                        }
                    }
                }
            }
        }

        public void Run()
        {
            executes.Add(testcases["Test_HelloWorld"]);

            for (int i = 0; i < SessionCount; i++)
            {
                GameObject go = new GameObject();
                T client = go.AddComponent<T>();

                clients.Add(client.session.session_key, client);
                /*
                client.session.RegisterHandler<.Message>(1, (Assets.Scripts.Message msg) =>
                {
                    Packet p = new Packet();
                    p.Id = 2;
                    p.Serialize(msg);
                    client.session.AsyncSend(p);
                });

                */
                client.session.OnConnectEvent += () =>
                {
                    executes[0](client);
                };
                client.session.AsyncConnect(Host, Port);
            }
        }
    }
}
