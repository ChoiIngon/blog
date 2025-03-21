using System.Collections;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            Debug.Log(Random.Range(0, 2));
        }
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
