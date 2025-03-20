using System.Collections;
using System.Numerics;

namespace NDungeonEvent.NActor
{
    public class Create : DungeonEvent
    {
        private Actor actor;

        public Create(Actor actor)
        {
            this.actor = actor;
        }

        public IEnumerator OnEvent()
        {
            if (null == actor.meta.skin)
            {
                yield break;
            }

            actor.Move((int)actor.position.x, (int)actor.position.y);
            yield break;
        }
    }
}