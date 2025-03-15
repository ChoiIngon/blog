using System.Collections;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        //DungeonEventQueue.Instance.Enqueue(new DungeonEventQueue.Test(1));
        //DungeonEventQueue.Instance.Enqueue(new DungeonEventQueue.Test(2));
        StartCoroutine(PrintNumber());
    }

    IEnumerator PrintNumber()
    {
        Coroutine coroutine = StartCoroutine(PrintNumber(1));
        yield return coroutine;
        StartCoroutine(PrintNumber(2));
        yield return StartCoroutine(PrintNumber(3));
    }

    IEnumerator PrintNumber(int number)
    {
        for (int i = 0; i < 10; i++)
        {
            Debug.Log($"print number {number}-{i}");
            yield return new WaitForSeconds(1.0f);
        }
    }
}
