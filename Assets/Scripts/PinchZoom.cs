using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 initialCameraRotation;

    private Vector2 touchStartPos;
    private Vector3 cameraStartPos;
    private float cameraRotationX;
    private float cameraRotationY;

    public float minOrthographicSize = 4.0f;
    public float maxOrthographicSize = 20.0f;
    public float zoomSpeed = 0.1f;
    public float moveSpeed = 0.1f;

    public bool allowCameraMovement = true;

    public void StopCamera() => allowCameraMovement = false;
    public void MoveCamera() => allowCameraMovement = true;

    void Start()
    {
        mainCamera = Camera.main;
        initialCameraRotation = new Vector3(30.0f, 45.0f, 0.0f);
        mainCamera.transform.rotation = Quaternion.Euler(initialCameraRotation);
        cameraRotationX = initialCameraRotation.x;
        cameraRotationY = initialCameraRotation.y;
    }

    void Update()
    {
        if (allowCameraMovement)
        {
            if (Input.touchCount == 2)
            {
                // Handle two-finger zoom (pinch) for camera zoom
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
                Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

                float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
                float touchDeltaMag = (touch1.position - touch2.position).magnitude;

                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                float newOrthographicSize = mainCamera.orthographicSize + deltaMagnitudeDiff * zoomSpeed;
                mainCamera.orthographicSize = Mathf.Clamp(newOrthographicSize, minOrthographicSize, maxOrthographicSize);
            }
            else if (Input.touchCount == 1)
            {
                // Handle one-finger drag for camera movement
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    touchStartPos = touch.position;
                    cameraStartPos = mainCamera.transform.position;
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    Vector2 touchDelta = touch.position - touchStartPos;
                    Vector3 moveDirection = new Vector3(-touchDelta.x, -touchDelta.y);

                    // Rotate the moveDirection based on camera rotation
                    Quaternion cameraRotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);
                    moveDirection = cameraRotation * moveDirection;

                    mainCamera.transform.position = cameraStartPos + moveDirection * moveSpeed;
                }
            }
        }        
    }
}
