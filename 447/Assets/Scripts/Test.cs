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
        yield return PrintNumber(1);
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
