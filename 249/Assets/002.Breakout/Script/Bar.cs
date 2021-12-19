using UnityEngine;

namespace Breakout
{
    public class Bar : MonoBehaviour
    {
        public Room room;

        public uint id;
        public float moveSpeed;
        public Vector3 position;

        public Plane backPlane;

        public void Init(Room room)
        {
            this.room = room;
            transform.localPosition = new Vector3(0, -10, 0);
            backPlane = new Plane(Vector3.forward, 0);
        }

        public void AttachBall(Ball ball)
        {
            ball.transform.SetParent(transform);
        }

        // Update is called once per frame
        void Update()
        {
            if (transform.localPosition.x < position.x)
            {
                transform.localPosition = new Vector3(transform.localPosition.x + moveSpeed * Time.deltaTime, transform.localPosition.y, transform.localPosition.z);
            }

            if (transform.localPosition.x > position.x)
            {
                transform.localPosition = new Vector3(transform.localPosition.x - moveSpeed * Time.deltaTime, transform.localPosition.y, transform.localPosition.z);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Room.State.Play == room.state)
            {
                Ball ball = collision.transform.GetComponent<Ball>();
                if (null == ball)
                {
                    return;
                }

                Vector3 normalVector = Vector3.zero;
                foreach (var contact in collision.contacts)
                {
                    normalVector = contact.normal;
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
            }
        }
    }
}