using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    public bool gameOver;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (true == gameOver)
        {
            GameManager.Instance.Init();
        }

        Ball ball = collision.transform.GetComponent<Ball>();
        if (null == ball)
        {
            return;
        }

        Vector3 reflect = Vector3.Reflect(ball.velocity, transform.forward);
        ball.velocity = reflect.normalized * ball.moveSpeed;
        ball.rigidBody.velocity = ball.velocity;
    }
}
