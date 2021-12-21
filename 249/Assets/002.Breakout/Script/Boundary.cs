using UnityEngine;

namespace Breakout
{
    public class Boundary : MonoBehaviour
    {
        public Room room;
        private void OnCollisionEnter(Collision collision)
        {
            Ball ball = collision.gameObject.GetComponent<Ball>();
            if (null == ball)
            {
                return;
            }
            room.SyncBall(ball);
        }
    }
}