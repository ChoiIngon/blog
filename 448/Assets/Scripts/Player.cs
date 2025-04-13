using UnityEngine;

public class Player : MonoBehaviour
{
    public Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(true == Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            animator.SetTrigger("Run");
            position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
        }

        if(true == Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.rotation = Quaternion.Euler(0, 270, 0);
            animator.SetTrigger("Run");
            position = new Vector3(transform.position.x - 1, transform.position.y, transform.position.z);
        }

        if(true == Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.rotation = Quaternion.Euler(0, 90, 0);
            animator.SetTrigger("Run");
            position = new Vector3(transform.position.x + 1, transform.position.y, transform.position.z);
        }

        if(true == Input.GetKeyDown(KeyCode.DownArrow))
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
            animator.SetTrigger("Run");
            position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
        }

        if(true == Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("Slash");
        }
    }

    public Vector3 position
    {
        get { return transform.position; }
        set 
        { 
            transform.position = value;
            Camera.main.transform.position = new Vector3(value.x, 7.0f, value.z - 2.0f);
        }
    }
}
