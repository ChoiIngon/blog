using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breakout
{
    public class GameMeta : Gamnet.Util.MonoSingleton<GameMeta>
    {
        public float barSpeed;
        public float ballSpeed;
        public string host;
        public int port;

        public bool useDeadReckoning;
        [Range(0, 1000)]
        public int packetDelay; //ms
    }
}