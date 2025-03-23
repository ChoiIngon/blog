using System.Collections;
using UnityEngine;

namespace NDungeonEvent.NActor
{
    public class Destroy : DungeonEvent
    {
        public Actor actor;

        public Destroy(Actor actor)
        {
            this.actor = actor;
        }

        public IEnumerator OnEvent()
        {
            actor.transform.parent = null;
            GameObject.DestroyImmediate(this.actor.gameObject);

            yield break;
        }
    }
}