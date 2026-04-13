using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 8f;

    [Header("Rotate")]
    public float rotateSpeed = 180f; // 度/秒

    [Header("Rotate Pivot")]
    public float pivotPlaneY = 0f;

    [Header("Zoom")]
    public float zoomSpeed = 20f;
    public float minHeight = 3f;
    public float maxHeight = 30f;

    private Vector3 rotatePivot;
    private bool isRotatingLastFrame;

    private void Start()
    {
        rotatePivot = GetViewCenterPointOnPlane();
    }

    private void Update()
    {
        HandleMove();
        HandleRotate();
        HandleZoom();
    }

    private void HandleMove()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.W)) vertical += 1f;

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 moveDir = (right * horizontal + forward * vertical).normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    private void HandleRotate()
    {
        float rotateInput = 0f;
        if (Input.GetKey(KeyCode.Q)) rotateInput -= 1f;
        if (Input.GetKey(KeyCode.E)) rotateInput += 1f;

        bool isRotatingNow = Mathf.Abs(rotateInput) > 0.01f;

        // 开始旋转时锁定一次视角中心，避免旋转过程中枢轴抖动
        if (isRotatingNow && !isRotatingLastFrame)
        {
            rotatePivot = GetViewCenterPointOnPlane();
        }

        if (isRotatingNow)
        {
            float deltaYaw = rotateInput * rotateSpeed * Time.deltaTime;

            Vector3 offset = transform.position - rotatePivot;
            offset = Quaternion.Euler(0f, deltaYaw, 0f) * offset;
            transform.position = rotatePivot + offset;

            transform.Rotate(Vector3.up, deltaYaw, Space.World);
        }

        isRotatingLastFrame = isRotatingNow;
    }


    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.0001f)
        {
            return;
        }

        Vector3 target = transform.position + transform.forward * (scroll * zoomSpeed);
        target.y = Mathf.Clamp(target.y, minHeight, maxHeight);
        transform.position = target;
    }

    private Vector3 GetViewCenterPointOnPlane()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        if (cam == null)
        {
            return new Vector3(transform.position.x, pivotPlaneY, transform.position.z);
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Plane plane = new Plane(Vector3.up, new Vector3(0f, pivotPlaneY, 0f));
        if (plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }

        return new Vector3(transform.position.x, pivotPlaneY, transform.position.z);
    }
}
