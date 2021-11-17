using UnityEngine;

namespace Assets.Scripts
{
    public class Simulator : MonoBehaviour
    {
        public class Client : Gamnet.Simulation.Client
        {
        }

        private void Start()
        {
            Gamnet.Simulation.Simulator.Init<Client>("127.0.0.1", 4000, 1, 1);
        }
    }
}
