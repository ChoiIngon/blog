using System.Collections;

namespace NDungeonEvent.NActor
{
    public class Attack : DungeonEvent
    {
        private Actor actor;
        private Actor target;

        public Attack(Actor actor, Actor target)
        {
            this.actor = actor;
            this.target = target;
        }

        public IEnumerator OnEvent()
        {
            actor.Attack(target);

            yield return actor.SetAction(Actor.Action.Attack);
            actor.StartCoroutine(actor.SetAction(Actor.Action.Idle));
        }
    }
}