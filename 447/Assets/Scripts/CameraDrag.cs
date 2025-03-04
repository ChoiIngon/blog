using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    private const float dragSpeed = 2;
    private Vector3 dragOrigin;

    private void Update()
    {
        if (true == Input.GetMouseButtonDown(1))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (false == Input.GetMouseButton(1))
        {
            return;
        }

        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(pos.x * dragSpeed, pos.y * dragSpeed, 0.0f);

        Camera.main.transform.Translate(-move, Space.World);
    }
}