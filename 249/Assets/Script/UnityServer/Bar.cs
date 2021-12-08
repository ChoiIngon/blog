using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bar : MonoBehaviour
{
    private const int MOUSE_BUTTON_LEFT = 0;
    public Rigidbody rigidBody;
    private int barColliderLayerMask;
    private Plane backPlane;

    public bool isLocal;
    public bool isTouched;
    public float moveSpeed;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        barColliderLayerMask = 1 << LayerMask.NameToLayer("BarTouchCollider");
        backPlane = new Plane(Vector3.forward, 0);

        Init();
    }

    public void Init()
    {
        isTouched = false;
        transform.localPosition = new Vector3(0, -10, 0);

        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
    }

    public void AttachBall()
    {
        GameManager.Instance.ball.transform.SetParent(transform);
    }

    public void DetachBall()
    {
        GameManager.Instance.ball.transform.SetParent(GameManager.Instance.transform);
    }

    // Update is called once per frame
    void Update()
    {
        // https://gamedevbeginner.com/how-to-convert-the-mouse-position-to-world-space-in-unity-2d-3d/#screen_to_world_3d
        if (true == isLocal)
        {
            if (GameManager.GameState.Ready == GameManager.Instance.state)
            {
                if (true == Input.GetMouseButtonDown(MOUSE_BUTTON_LEFT))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (true == Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        Bar bar = hit.transform.GetComponent<Bar>();
                        if (null != bar)
                        {
                            isTouched = true;
                        }
                    }
                }
                if (true == Input.GetMouseButtonUp(MOUSE_BUTTON_LEFT))
                {
                    if (true == isTouched)
                    {
                        GameManager.Instance.Play();
                    }
                }
            }
            if (GameManager.GameState.Init != GameManager.Instance.state)
            {
                if (true == Input.GetMouseButton(MOUSE_BUTTON_LEFT))
                {
                    float distance;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (true == backPlane.Raycast(ray, out distance))
                    {
                        Vector3 worldPosition = ray.GetPoint(distance);
                        if (transform.position.x < worldPosition.x)
                        {
                            transform.position = new Vector3(transform.position.x + moveSpeed * Time.deltaTime, transform.position.y, transform.position.z);
                        }

                        if (transform.position.x > worldPosition.x)
                        {
                            transform.position = new Vector3(transform.position.x - moveSpeed * Time.deltaTime, transform.position.y, transform.position.z);
                        }
                    }
                }
                rigidBody.velocity = Vector3.zero;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

    }
}
