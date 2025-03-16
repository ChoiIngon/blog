using System.Collections;

namespace NDungeonEvent.NActor
{
    public class Move : DungeonEvent
    {
        private Actor actor;
        private int x;
        private int y;

        public Move(Actor actor, int x, int y)
        {
            this.actor = actor;
            this.x = x;
            this.y = y;
        }

        public IEnumerator OnEvent()
        {
            actor.Move(x, y);

            yield return actor.SetAction(Actor.Action.Walk);
            actor.StartCoroutine(actor.SetAction(Actor.Action.Idle));
        }
    }
}