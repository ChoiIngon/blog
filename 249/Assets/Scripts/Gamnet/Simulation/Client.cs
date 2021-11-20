
using System;
using UnityEngine;

namespace Gamnet.Simulation
{
    public class Client : MonoBehaviour
    {
        public Gamnet.Client.Session session = new Gamnet.Client.Session();
        public int ScenarioIndex;
        public int LoopCount;

        private void OnApplicationPause(bool pause)
        {
            if (true == pause)
            {
                session.Pause();
            }
            else
            {
                session.Resume();
            }
        }

        private void OnApplicationQuit()
        {
            session.Close();
        }

        public void AsyncConnect(string host, int port)
        {
            session.OnConnectEvent += () =>
            {
                Simulator.Execute(this);
            };
            session.AsyncConnect(host, port);
        }
        public void MoveNext()
        {
            ScenarioIndex++;
            Simulator.Execute(this);
        }
    }
}