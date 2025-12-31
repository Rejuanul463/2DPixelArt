using UnityEngine;

public class CameraController2D : MonoBehaviour
{
    [Header("Camera Movement")]
    public float dragSpeed = 0.5f;       // how fast camera moves
    public float smoothTime = 0.2f;      // smooth damping

    [Header("Map Boundaries")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Vector3 dragOrigin;
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;

    [SerializeField] private PannelManager pannelManager;

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

    void HandleDrag()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // MOUSE DRAG
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPosition += difference * dragSpeed;
        }
#else
        // TOUCH DRAG
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                dragOrigin = Camera.main.ScreenToWorldPoint(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector3 difference = dragOrigin - Camera.main.ScreenToWorldPoint(touch.position);
                targetPosition += difference * dragSpeed;
            }
        }
#endif
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
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);

        transform.position = pos;
        targetPosition = pos; // so smoothing doesn't push camera outside
    }
}
