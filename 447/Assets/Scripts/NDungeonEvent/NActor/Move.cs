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
			actor.transform.position = to;
            yield return actor.SetAction(Actor.Action.Walk);
            actor.StartCoroutine(actor.SetAction(Actor.Action.Idle));
        }

        
    }
}