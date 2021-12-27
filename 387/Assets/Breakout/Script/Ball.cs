using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breakout
{
    public class Ball : MonoBehaviour
    {
        public Room room;

        public uint id;
        public Rigidbody rigidBody;

        public void Init(Room room)
        {
            this.room = room;

            this.rigidBody = GetComponent<Rigidbody>();
            this.rigidBody.useGravity = true;

            transform.rotation = Quaternion.identity;

            SetDirection(Vector3.zero);
        }

        public void SetDirection(Vector3 direction)
        {
            rigidBody.velocity = direction.normalized * GameMeta.Instance.ballSpeed;
        }

        private void OnCollisionEnter(Collision collision)
        {
            rigidBody.velocity = rigidBody.velocity.normalized * GameMeta.Instance.ballSpeed;
        }
    }
}