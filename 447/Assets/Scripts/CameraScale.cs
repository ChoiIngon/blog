using UnityEngine;

public class CameraScale : MonoBehaviour
{
    private const float mouseWheelSpeed = 10.0f;
    private const float minFieldOfView = 20.0f;
    private const float maxFieldOfView = 120.0f;

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel") * mouseWheelSpeed;
        if (Camera.main.fieldOfView < minFieldOfView && scroll < 0.0f)
        {
            Camera.main.fieldOfView = minFieldOfView;
        }
        else if (Camera.main.fieldOfView > maxFieldOfView && scroll > 0.0f)
        {
            Camera.main.fieldOfView = maxFieldOfView;
        }
        else
        {
            Camera.main.fieldOfView -= scroll;
        }
    }
}