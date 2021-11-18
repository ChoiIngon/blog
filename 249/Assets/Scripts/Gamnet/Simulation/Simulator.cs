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

namespace Gamnet.Simulation
{
    public class Simulator
    {
        private static IExecuter executer;

        public static void Init<CLIENT_T>(string host, int port, int sessionCount, int loopCount) where CLIENT_T : Gamnet.Simulation.Client
        {
            Executer<CLIENT_T> exec = new Executer<CLIENT_T>();
            exec.Host = host;
            exec.Port = port;
            exec.SessionCount = sessionCount;
            exec.LoopCount = loopCount;
            exec.Init();
            executer = exec;
            exec.Run();
        }

        public static void AddScenario(string scenarioName)
        {
            executer.AddScenario(scenarioName);
        }

        public static void Execute(int scenarioIndex, Client client)
        {
            executer.Execute(scenarioIndex, client);
        }
        public static void Update()
        {
        }

        private interface IExecuter
        {
            void AddScenario(string scenarioName);
            void Execute(int scenarioIndex, Client client);
        }
        private class Executer<CLIENT_T> : IExecuter where CLIENT_T : Gamnet.Simulation.Client
        {
            public string Host;
            public int Port;
            public int SessionCount;
            public int LoopCount;
            public Dictionary<string, Action<CLIENT_T>> scenarios = new Dictionary<string, Action<CLIENT_T>>();
            private Dictionary<uint, CLIENT_T> clients = new Dictionary<uint, CLIENT_T>();
            public List<Action<CLIENT_T>> executes = new List<Action<CLIENT_T>>();

            public void Init()
            {
                string exeAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                AppDomain currentDomain = AppDomain.CurrentDomain;
                Assembly[] assems = currentDomain.GetAssemblies();
                IEnumerable<Assembly> executingAssembly = assems.Where(a => a.GetName().Name.Equals(exeAssemblyName));

                IEnumerable<Type> childrenTypes = executingAssembly.SelectMany(s => s.GetTypes()).Where(p => typeof(Server.IPacketHandler).IsAssignableFrom(p) && p.IsClass);
                foreach (var type in childrenTypes)
                {
                    // 테스트 메소드들 자동 등록
                    MethodInfo[] methodInfos = type.GetMethods();
                    foreach (MethodInfo methodInfo in methodInfos)
                    {
                        IEnumerable<Attribute> attributes = methodInfo.GetCustomAttributes();
                        foreach (Attribute attr in attributes)
                        {
                            if (attr is Server.TestMethod testMethod)
                            {
                                object obj = Activator.CreateInstance(type) as object;
                                Action<CLIENT_T> action = (Action<CLIENT_T>)Delegate.CreateDelegate(typeof(Action<CLIENT_T>), obj, methodInfo);
                                scenarios.Add(methodInfo.Name, action);
                            }
                        }
                    }
                }
            }

            public void AddScenario(string scenarioName)
            {
                Action<CLIENT_T> scenario = null;
                if (false == scenarios.TryGetValue(scenarioName, out scenario))
                {
                    throw new KeyNotFoundException($"can not find scenario:{scenarioName}");
                }
                executes.Add(scenario);
            }
            public void Execute(int scenarioIndex, Client client)
            {
                CLIENT_T client_t = client as CLIENT_T;
                if (scenarioIndex >= executes.Count)
                {
                    client.session.Close();
                    clients.Remove(client.session.session_key);
                    GameObject.Destroy(client.gameObject);
                    return;
                }
                executes[scenarioIndex](client_t);
            }

            public void Run()
            {
                for (int i = 0; i < SessionCount; i++)
                {
                    GameObject go = new GameObject();

                    CLIENT_T client = go.AddComponent<CLIENT_T>();
                    go.name = $"Scenario.Client.{client.session.session_key}";
                    clients.Add(client.session.session_key, client);
                    client.AsyncConnect(Host, Port);
                }
            }
        }
    }


}
