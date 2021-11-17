
using System;
using UnityEngine;

namespace Gamnet.Simulation
{
    public class Client : MonoBehaviour
    {
        public Gamnet.Client.Session session = new Gamnet.Client.Session();
        public int test_seq;

        public void MoveNext()
        {
        }
        private void Update()
        {
            session.Update();
        }
    }
}