using UnityEngine;

namespace Breakout
{
    public class Boundary : MonoBehaviour
    {
        public Room room;
        private void OnCollisionEnter(Collision collision)
        {
            room.SyncWorld();
        }
    }
}