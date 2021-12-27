using UnityEngine;

namespace Breakout
{
    public class Bar : MonoBehaviour
    {
        public Room room;

        public uint id;
        public float moveSpeed;
        public Vector3 destination;

        public void Init(Room room)
        {
            this.room = room;
        }

        void Update()
        {
            if (transform.localPosition.x < destination.x)
            {
                transform.localPosition = new Vector3(transform.localPosition.x + moveSpeed * Time.deltaTime, transform.localPosition.y, transform.localPosition.z);
            }

            if (transform.localPosition.x > destination.x)
            {
                transform.localPosition = new Vector3(transform.localPosition.x - moveSpeed * Time.deltaTime, transform.localPosition.y, transform.localPosition.z);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Room.State.Play == room.state)
            {
                Ball ball = collision.gameObject.GetComponent<Ball>();
                if (null == ball)
                {
                    return;
                }

                // https://answers.unity.com/questions/24012/find-size-of-gameobject.html
                float width = GetComponent<Collider>().bounds.size.x;
                float start = transform.position.x - (width / 2);
                float point = Mathf.Abs(start - collision.contacts[0].point.x);
                float contactRate = 1.0f - (point / width);
                if (contactRate < 0.2f)
                {
                    contactRate = 0.2f;
                }
                if (contactRate > 0.8f)
                {
                    contactRate = 0.8f;
                }
                float theta = contactRate * Mathf.PI;
                float x = Mathf.Cos(theta);
                float y = Mathf.Sin(theta);
                ball.SetDirection(new Vector3(x, y, 0));

                Vector3 normalVector = collision.contacts[0].normal;
                if (0f < normalVector.y)
                {
                    float height = GetComponent<Collider>().bounds.size.y;
                    Vector3 barPosition = transform.position;
                    barPosition.y += height;
                    ball.transform.position = new Vector3(ball.transform.position.x, barPosition.y, ball.transform.position.z);
                }

                room.SyncBall(ball);
            }
        }
    }
}