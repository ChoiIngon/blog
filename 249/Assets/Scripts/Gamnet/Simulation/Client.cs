
using System;
using UnityEngine;

namespace Gamnet.Simulation
{
    public class Client : MonoBehaviour
    {
        public Gamnet.Client.Session session = new Gamnet.Client.Session();
        public int ScenarioIndex;
        public int LoopCount;

        private void Start()
        {
            session.OnConnectEvent += () =>
            {
                Debug.Log($"UnityServer.Simulation.Client:OnConnectEvent");
                Simulator.Execute(this);
            };
            session.OnPauseEvent += () =>
            {
                Debug.Log($"UnityServer.Simulation.Client:OnPauseEvent");
            };
            session.OnResumeEvent += () =>
            {
                Debug.Log($"UnityServer.Simulation.Client:OnResumeEvent");
            };
            session.OnCloseEvent += () =>
            {
                Debug.Log($"UnityServer.Simulation.Client:OnCloseEvent");
            };
        }

        private void OnDestroy()
        {
            session.OnConnectEvent = null;
            session.OnPauseEvent = null;
            session.OnResumeEvent = null;
            session.OnConnectEvent = null;
        }

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

            session.AsyncConnect(host, port);
        }

        public void MoveNext()
        {
            ScenarioIndex++;
            Simulator.Execute(this);
        }
    }
}