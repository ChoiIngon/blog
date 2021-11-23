using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Gamnet.Simulation
{
    public class Simulator : MonoBehaviour
    {
        public string Host;
        public int Port;
        public int SessionCount;
        public int LoopCount;
        public string[] ScenarioNames;
        private IExecuter executer;

        private static Simulator instance;

        private interface IExecuter
        {
            void AddScenario(string scenarioName);
            void Execute(Client client);
        }

        private class Executer<CLIENT_T> : IExecuter where CLIENT_T : Gamnet.Simulation.Client
        {
            public Dictionary<string, Action<CLIENT_T>> scenarios = new Dictionary<string, Action<CLIENT_T>>();
            public List<Action<CLIENT_T>> executes = new List<Action<CLIENT_T>>();

            public Executer()
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

            public void Execute(Client client)
            {
                if (client.ScenarioIndex >= executes.Count)
                {
                    if (1 < client.LoopCount)
                    {
                        CLIENT_T client_t = CreateClient();
                        client_t.LoopCount = client.LoopCount - 1;
                        client_t.AsyncConnect(instance.Host, instance.Port);
                    }

                    client.session.Close();
                    client.transform.SetParent(null);
                    GameObject.Destroy(client.gameObject);
                    return;
                }

                {
                    CLIENT_T client_t = client as CLIENT_T;
                    executes[client.ScenarioIndex](client_t);
                }
            }

            public CLIENT_T CreateClient()
            {
                GameObject go = new GameObject();
                CLIENT_T client_t = go.AddComponent<CLIENT_T>();
                go.name = $"Scenario.Client.{client_t.session.session_key}";
                client_t.transform.SetParent(instance.transform, false);
                return client_t;
            }
        }

        public void Init<CLIENT_T>() where CLIENT_T : Gamnet.Simulation.Client
        {
            Executer<CLIENT_T> executer = new Executer<CLIENT_T>();
            this.executer = executer;

            instance = (Simulator)GameObject.FindObjectOfType(typeof(Simulator));
            if (null == instance)
            {
                GameObject container = new GameObject();
                container.name = "Simulator";
                instance = container.AddComponent<Simulator>();
            }

            foreach (string scenarioName in ScenarioNames)
            {
                executer.AddScenario(scenarioName);
            }

            for (int i = 0; i < SessionCount; i++)
            {
                CLIENT_T client_t = executer.CreateClient();
                client_t.LoopCount = LoopCount;
                client_t.AsyncConnect(Host, Port);
            }
        }

        public static void Execute(Client client)
        {
            instance.executer.Execute(client);
        }
    }
}
