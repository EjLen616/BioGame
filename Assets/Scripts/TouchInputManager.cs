using UnityEngine;
using UnityEngine.EventSystems;

public class TouchInputManager : MonoBehaviour
{
    public static TouchInputManager Instance { get; private set; }

    [Header("Touch Settings")]
    public float touchDragThreshold = 10f;
    public bool enableMouseSimulation = true; // For editor testing

    private Vector2 touchStartPos;
    private bool isDragging = false;
    private GameObject draggedObject;
    private Vector3 dragOffset;
    private Camera mainCamera;
    private Vector2 lastTouchPos;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
        }
    }

    void Update()
    {
        // Handle touch input for mobile
        if (Input.touchCount > 0)
        {
            HandleTouchInput();
        }
        // Handle mouse input for editor testing
        else if (enableMouseSimulation)
        {
            HandleMouseInput();
        }
    }

    void HandleTouchInput()
    {
        Touch touch = Input.GetTouch(0);
        Vector3 touchWorldPos = mainCamera.ScreenToWorldPoint(touch.position);
        touchWorldPos.z = 0;

        switch (touch.phase)
        {
            case TouchPhase.Began:
                // Check if touching UI
                if (IsPointerOverUI(touch.position))
                    return;

                // Find object under touch
                RaycastHit2D hit = Physics2D.Raycast(touchWorldPos, Vector2.zero);
                if (hit.collider != null)
                {
                    FallingObject fallingObject = hit.collider.GetComponent<FallingObject>();
                    if (fallingObject != null)
                    {
                        isDragging = true;
                        draggedObject = hit.collider.gameObject;
                        dragOffset = (Vector3)touchWorldPos - draggedObject.transform.position;

                        // Notify object it's being dragged
                        fallingObject.OnTouchStart();
                    }
                }
                break;

            case TouchPhase.Moved:
                if (isDragging && draggedObject != null)
                {
                    // Move the object with touch
                    Vector3 newPos = touchWorldPos - dragOffset;
                    newPos.z = 0;
                    draggedObject.transform.position = newPos;

                    // Notify object it's being dragged
                    FallingObject fallingObject = draggedObject.GetComponent<FallingObject>();
                    if (fallingObject != null)
                    {
                        fallingObject.OnTouchDrag(touchWorldPos);
                    }
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (isDragging && draggedObject != null)
                {
                    // Check if dropped on basket
                    CheckDropTarget(touchWorldPos);

                    // Notify object touch ended
                    FallingObject fallingObject = draggedObject.GetComponent<FallingObject>();
                    if (fallingObject != null)
                    {
                        fallingObject.OnTouchEnd();
                    }

                    isDragging = false;
                    draggedObject = null;
                }
                break;
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            if (IsPointerOverUI(Input.mousePosition))
                return;

            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null)
            {
                FallingObject fallingObject = hit.collider.GetComponent<FallingObject>();
                if (fallingObject != null)
                {
                    isDragging = true;
                    draggedObject = hit.collider.gameObject;
                    dragOffset = mousePos - draggedObject.transform.position;
                    fallingObject.OnTouchStart();
                }
            }
        }
        else if (Input.GetMouseButton(0) && isDragging && draggedObject != null)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector3 newPos = mousePos - dragOffset;
            newPos.z = 0;
            draggedObject.transform.position = newPos;

            FallingObject fallingObject = draggedObject.GetComponent<FallingObject>();
            if (fallingObject != null)
            {
                fallingObject.OnTouchDrag(mousePos);
            }
        }
        else if (Input.GetMouseButtonUp(0) && isDragging && draggedObject != null)
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            CheckDropTarget(mousePos);

            FallingObject fallingObject = draggedObject.GetComponent<FallingObject>();
            if (fallingObject != null)
            {
                fallingObject.OnTouchEnd();
            }

            isDragging = false;
            draggedObject = null;
        }
    }

    void CheckDropTarget(Vector3 dropPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(dropPosition, 0.5f);

        foreach (Collider2D collider in colliders)
        {
            Basket basket = collider.GetComponent<Basket>();
            if (basket != null)
            {
                FallingObject fallingObject = draggedObject.GetComponent<FallingObject>();
                if (fallingObject != null)
                {
                    basket.HandleObjectCaught(fallingObject);
                    return;
                }
            }
        }

        // If not caught by basket, release object to fall
        if (draggedObject != null)
        {
            FallingObject fallingObject = draggedObject.GetComponent<FallingObject>();
            if (fallingObject != null)
            {
                fallingObject.ReleaseFromDrag();
            }
        }
    }

    bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        // Check if pointer is over UI
        return EventSystem.current.IsPointerOverGameObject();
    }

    public bool IsCurrentlyDragging()
    {
        return isDragging;
    }

    public GameObject GetDraggedObject()
    {
        return draggedObject;
    }
}