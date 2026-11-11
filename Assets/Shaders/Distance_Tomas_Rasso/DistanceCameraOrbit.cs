using UnityEngine;

/// <summary>
/// Smoothly orbits and zooms the camera around the target character
/// using mouse drag, WASD / arrow / Q-E keys, and scroll wheel.
/// </summary>
public class DistanceCameraOrbit : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.0f, 0f);

    [Header("Distancia y Angulos")]
    [SerializeField] private float distance = 8.0f;
    [SerializeField] private float minDistance = 3.5f;
    [SerializeField] private float maxDistance = 15.0f;
    [SerializeField] private float minPitch = 15.0f;
    [SerializeField] private float maxPitch = 80.0f;

    [Header("Controles")]
    [SerializeField] private float mouseRotateSpeed = 0.28f;
    [SerializeField] private float keyYawSpeed = 90.0f;
    [SerializeField] private float keyPitchSpeed = 55.0f;
    [SerializeField] private float zoomSpeed = 1.2f;
    [SerializeField] private float smoothTime = 0.08f;

    private float targetYaw;
    private float targetPitch;
    private float targetDistance;

    private float currentYaw;
    private float currentPitch;
    private float currentDistance;

    private float yawVelocity;
    private float pitchVelocity;
    private float distanceVelocity;

    private Vector3 lastMousePosition;
    private bool isDragging;

    private void Start()
    {
        if (target == null && transform.parent != null)
        {
            target = transform.parent;
        }

        Vector3 focusPoint = GetFocusPoint();
        Vector3 offset = transform.position - focusPoint;
        if (offset.sqrMagnitude > 0.001f)
        {
            targetDistance = Mathf.Clamp(offset.magnitude, minDistance, maxDistance);
            Vector3 dir = offset / offset.magnitude;
            targetPitch = Mathf.Clamp(Mathf.Asin(Mathf.Clamp(dir.y, -0.99f, 0.99f)) * Mathf.Rad2Deg, minPitch, maxPitch);
            targetYaw = Mathf.Atan2(-dir.x, -dir.z) * Mathf.Rad2Deg;
        }
        else
        {
            targetDistance = Mathf.Clamp(distance, minDistance, maxDistance);
            targetPitch = Mathf.Clamp(36.87f, minPitch, maxPitch);
            targetYaw = 180.0f;
        }

        currentYaw = targetYaw;
        currentPitch = targetPitch;
        currentDistance = targetDistance;

        if (transform.parent != null)
        {
            transform.SetParent(null, true);
        }

        ApplyCameraTransform(focusPoint);
    }

    private void Update()
    {
        HandleMouseInput();
        HandleKeyboardInput();
        HandleZoomInput();

        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, smoothTime);
        currentPitch = Mathf.SmoothDamp(currentPitch, targetPitch, ref pitchVelocity, smoothTime);
        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, smoothTime);

        ApplyCameraTransform(GetFocusPoint());
    }

    private void HandleMouseInput()
    {
        bool buttonDown = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2);
        bool buttonHeld = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2);

        if (buttonDown)
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
            return;
        }

        if (!buttonHeld)
        {
            isDragging = false;
            return;
        }

        if (!isDragging)
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
            return;
        }

        Vector3 delta = Input.mousePosition - lastMousePosition;
        lastMousePosition = Input.mousePosition;

        targetYaw += delta.x * mouseRotateSpeed;
        targetPitch -= delta.y * mouseRotateSpeed;
    }

    private void HandleKeyboardInput()
    {
        float yawInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Q))
            yawInput += 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.E))
            yawInput -= 1f;

        float pitchInput = 0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            pitchInput += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            pitchInput -= 1f;

        targetYaw += yawInput * keyYawSpeed * Time.deltaTime;
        targetPitch += pitchInput * keyPitchSpeed * Time.deltaTime;
    }

    private void HandleZoomInput()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.001f)
        {
            targetDistance -= scroll * zoomSpeed;
        }
    }

    private Vector3 GetFocusPoint()
    {
        if (target != null)
            return target.position + targetOffset;

        return targetOffset;
    }

    private void ApplyCameraTransform(Vector3 focusPoint)
    {
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        transform.position = focusPoint + rotation * (Vector3.back * currentDistance);
        transform.LookAt(focusPoint);
    }
}

