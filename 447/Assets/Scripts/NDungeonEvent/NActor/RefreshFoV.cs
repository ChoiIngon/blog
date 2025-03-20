using System.Collections;
using UnityEngine;

namespace NDungeonEvent.NActor
{
    public class RefreshFoV : DungeonEvent
    {
        private Player player;
        
        public RefreshFoV(Player player)
        {
            this.player = player;
        }

        public IEnumerator OnEvent()
        {
            player.RefreshFoV();
            yield break;
        }
    }
}