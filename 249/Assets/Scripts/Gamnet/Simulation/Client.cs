
using System;
using UnityEngine;

namespace Gamnet.Simulation
{
    public class Client : MonoBehaviour
    {
        public Gamnet.Client.Session session = new Gamnet.Client.Session();
        private int scenario_index;
        public string Host;
        public int Port;
        private void Start()
        {
            session.AsyncConnect(Host, Port);
        }

        private void OnApplicationPause(bool pause)
        {
            if (true == pause)
            {
                session.OnPause();
            }
            else
            {
                session.OnResume();
            }
        }
        private void OnApplicationQuit()
        {
            session.OnClose();
        }

        public void AsyncConnect(string host, int port)
        {
            session.OnConnectEvent += () =>
            {
                this.scenario_index = 0;
                Simulator.Execute(scenario_index, this);
            };
            Host = host;
            Port = port;
        }
        public void MoveNext()
        {
            scenario_index++;
            Simulator.Execute(scenario_index, this);
        }
        private void Update()
        {
            session.Update();
        }
    }
}