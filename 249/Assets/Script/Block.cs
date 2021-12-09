using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    // Start is called before the first frame update
    public int id;
    public int durability;

    void Start()
    {
    }

	private void OnCollisionEnter(Collision collision)
	{
        if (GameManager.GameState.Play != GameManager.Instance.state)
        {
            return;
        }

        Ball ball = collision.transform.GetComponent<Ball>();
        if (null == ball)
        {
            return;
        }

        if (0 >= --durability)
        {
            transform.SetParent(null);
            GameObject.Destroy(gameObject);

            if (0 == GameManager.Instance.blocks.childCount)
            {
                GameManager.Instance.Init();
            }
        }
	}
}
