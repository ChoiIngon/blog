using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

class DungeonEventQueue : MonoBehaviour
{
    public interface DungeonEvent
    {
        public IEnumerator OnEvent();
    }

    public class Idle : DungeonEvent
    {
        Actor actor;
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

    public class Move : DungeonEvent
    {
        Actor actor;
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

    public class Attack : DungeonEvent
    {
        Actor actor;
        Actor target;

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

    public class Test : DungeonEvent
    {
        int id;
        public Test(int id)
        {
            this.id = id;
        }

        private IEnumerator InfiniteLoop()
        {
            int i = 0;
            while (true)
            {
                Debug.Log($"log:{id}-{i++}");
                yield return new WaitForSeconds(1.0f);
            }
        }

        public IEnumerator OnEvent()
        {
            Instance.StartCoroutine(InfiniteLoop());
            yield break;
        }
    }

    

    private Coroutine coroutine;
    private Queue<DungeonEvent> events = new Queue<DungeonEvent>();

    public void Clear()
    {
        events.Clear();
        if (null != coroutine)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    public void Enqueue(DungeonEvent e)
    {
        events.Enqueue(e);
        if (null == coroutine)
        {
            coroutine = StartCoroutine(Dequeue());
        }
    }

    private IEnumerator Dequeue()
    {
        while (0 < events.Count)
        {
            var evt = events.Dequeue();
            yield return evt.OnEvent();
        }

        StopCoroutine(coroutine);
        coroutine = null;
    }

    public bool Active
    {
        get
        {
            return 0 < events.Count || null != coroutine;
        }
    }


    private static DungeonEventQueue _instance = null;
    public static DungeonEventQueue Instance
    {
        get
        {
            if (null == _instance)
            {
                _instance = (DungeonEventQueue)GameObject.FindObjectOfType(typeof(DungeonEventQueue));
                if (null == _instance)
                {
                    GameObject container = new GameObject();
                    container.name = typeof(DungeonEventQueue).Name;
                    _instance = container.AddComponent<DungeonEventQueue>();
                }
            }

            return _instance;
        }
    }
}