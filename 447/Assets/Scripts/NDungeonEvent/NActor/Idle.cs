using System.Collections;

namespace NDungeonEvent.NActor
{
    public class Idle : DungeonEvent
    {
        private Actor actor;

        public Idle(Actor actor)
        {
            this.actor = actor;
        }

        public IEnumerator OnEvent()
        {
            if (null == actor.meta.skin)
            {
                yield break;
            }

            actor.StartCoroutine(actor.SetAction(Actor.Action.Idle));
            yield break;
        }
    }
}