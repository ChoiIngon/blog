using System.Collections;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(AddLog());
    }

    IEnumerator AddLog()
    {
        for (int i = 0; i < 200; i++)
        {
            DungeonLog.Write($"{i}:Hello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello worldHello world");
            yield return new WaitForSeconds(0.1f);
        }
    }
}
