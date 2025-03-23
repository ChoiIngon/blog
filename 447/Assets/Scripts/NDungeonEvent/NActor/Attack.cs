using System.Collections;

namespace NDungeonEvent.NActor
{
    public class Attack : DungeonEvent
    {
        private Actor actor;
        private Actor target;
        private int health;
        private int damage;

        public Attack(Actor actor, Actor target, int health, int damage)
        {
            this.actor = actor;
            this.target = target;
            this.health = health;
            this.damage = damage;
        }

        public IEnumerator OnEvent()
        {
            yield return actor.SetAction(Actor.Action.Attack);
            actor.StartCoroutine(actor.SetAction(Actor.Action.Idle));
        }
    }
}