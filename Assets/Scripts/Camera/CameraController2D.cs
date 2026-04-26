using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class CameraController2D : MonoBehaviour
{
    [Header("Camera Movement")]
    public float dragSpeed = 0.5f;
    public float smoothTime = 0.2f;

    [Header("Map Boundaries")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Vector3 dragOrigin;
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;

    [SerializeField] private PannelManager pannelManager;

    void Awake()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Start()
    {
        targetPosition = transform.position;
    }

    void Update()
    {
        if (pannelManager.activePannelObj == null)
        {
            HandleDrag();
        }

        SmoothMove();
        ClampPosition();
    }

    Vector3 GetWorldPoint(Vector2 screenPos)
    {
        Vector3 point = new Vector3(screenPos.x, screenPos.y, -Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(point);
    }

    void HandleDrag()
    {
        // Handle touch (works on Android AND in Editor with touch simulation)
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                dragOrigin = GetWorldPoint(touch.screenPosition);
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                     touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary)
            {
                Vector3 currentWorldPoint = GetWorldPoint(touch.screenPosition);
                Vector3 difference = dragOrigin - currentWorldPoint;
                targetPosition += difference * dragSpeed;
                dragOrigin = currentWorldPoint;
            }
        }
        // Handle mouse (Editor / Standalone fallback)
        else if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                dragOrigin = GetWorldPoint(Mouse.current.position.ReadValue());
            }

            if (Mouse.current.leftButton.isPressed)
            {
                Vector3 currentWorldPoint = GetWorldPoint(Mouse.current.position.ReadValue());
                Vector3 difference = dragOrigin - currentWorldPoint;
                targetPosition += difference * dragSpeed;
                dragOrigin = currentWorldPoint;
            }
        }
    }

    void SmoothMove()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }

    void ClampPosition()
    {
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
        transform.position = pos;
    }
}