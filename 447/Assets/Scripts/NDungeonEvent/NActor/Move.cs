using System.Collections;
using UnityEngine;

namespace NDungeonEvent.NActor
{
    public class Move : DungeonEvent
    {
        private Actor actor;
        private Vector3 from;
        private Vector3 to;

        public Move(Actor actor, Vector3 from, Vector3 to)
        {
            this.actor = actor;
            this.from = from;
            this.to = to;
        }

        public IEnumerator OnEvent()
        {
            yield return actor.SetAction(Actor.Action.Walk, (Skin.SpriteSheet spriteSheet, int index) =>
            {
                float spriteCount = spriteSheet.sprites.Count;
                float interpolation = (index + 1) / spriteCount;
				actor.spritePosition = Vector3.Lerp(this.from, this.to, interpolation);
			});
            actor.StartCoroutine(actor.SetAction(Actor.Action.Idle));
        }
    }
}