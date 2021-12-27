using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breakout
{
    public class Room : MonoBehaviour
    {
        public const int WIDTH = 20;
        public static Vector3[] start_position = new Vector3[]
        {
            new Vector3(-3, -10, 0),
            new Vector3( 3, -10, 0)
        };

        public static uint objectId = 0;

        public enum State
        {
            Init,
            JoinWait,
            Ready,
            Play
        }

        public State state;

        public void Init()
        {
            state = State.Init;
            Boundary[] boundaris = transform.GetComponentsInChildren<Boundary>();
            foreach (Boundary boundary in boundaris)
            {
                boundary.room = this;
            }
        }

        public virtual void SyncBall(Ball ball)
        {
        }

        public virtual void SyncBlock(Block block)
        {
        }
    }
}