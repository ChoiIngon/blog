using UnityEngine;

namespace UnityServer
{
    public class Simulator : MonoBehaviour
    {
        public string Host;
        public int Port;
        public int SessionCount;
        public int LoopCount;
        public string[] ScenarioNames;
        public class Client : Gamnet.Simulation.Client
        {
            public int number;
        }

        public void Init()
        {
            Gamnet.Simulation.Simulator.Init<Client>(Host, Port, SessionCount, LoopCount);
            foreach (string scenarioName in ScenarioNames)
            {
                Gamnet.Simulation.Simulator.AddScenario(scenarioName);
            }
            enabled = true;
        }

        private void Start()
        {
            enabled = false;
        }

        private void Update()
        {
            Gamnet.Simulation.Simulator.Update();
        }
    }
}
