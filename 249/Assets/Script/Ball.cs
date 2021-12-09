using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    // Start is called before the first frame update
    public Rigidbody rigidBody;
    public Vector3 velocity;
    public float moveSpeed;

    private int frameCount;

    public void Init()
    {
        transform.localPosition = new Vector3(0, -9, 0);
        transform.rotation = Quaternion.identity;
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.useGravity = true;
        SetDirection(Vector3.zero);
        frameCount = 0;
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

        if (GameManager.GameState.Play == GameManager.Instance.state)
        {
            if (collision.gameObject.layer != LayerMask.NameToLayer("Collision"))
            {
                return;
            }

            if (Time.frameCount == frameCount)
            {
                return;
            }

            frameCount = Time.frameCount;

            foreach (ContactPoint contact in collision.contacts)
            {
                Vector3 reflect = Vector3.Reflect(velocity, contact.normal.normalized);
                velocity = reflect.normalized * moveSpeed;
                rigidBody.velocity = velocity;
            }
        }
    }
}
