using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NDungeonEvent
{
    public interface DungeonEvent
    {
        public IEnumerator OnEvent();
    }
}

class DungeonEventQueue : MonoBehaviour
{
    private Coroutine coroutine;
    private Queue<NDungeonEvent.DungeonEvent> events = new Queue<NDungeonEvent.DungeonEvent>();

    public void Clear()
    {
        events.Clear();
        if (null != coroutine)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    public void Enqueue(NDungeonEvent.DungeonEvent e)
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