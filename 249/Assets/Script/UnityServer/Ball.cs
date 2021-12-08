using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody rigidBody;
    public Vector3 velocity;
    public float moveSpeed;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        Init();
    }

    public void Init()
    {
        transform.localPosition = new Vector3(0, -9, 0);
        transform.rotation = Quaternion.identity;
        rigidBody.useGravity = true;
        SetDirection(Vector3.zero);
    }

    public void SetDirection(Vector3 direction)
    {
        velocity = direction.normalized * moveSpeed;
        rigidBody.velocity = velocity;
        rigidBody.angularVelocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (GameManager.GameState.Init == GameManager.Instance.state)
        {
            if (null != collision.transform.GetComponent<Bar>())
            {
                rigidBody.useGravity = false;
                SetDirection(Vector3.zero);
                GameManager.Instance.Ready();
            }
        }
    }
}
