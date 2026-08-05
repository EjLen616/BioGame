using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobileInputHelper : MonoBehaviour
{
    [Header("Touch Feedback")]
    public GameObject touchFeedbackPrefab;
    public float feedbackDuration = 0.2f;
    public bool showTouchCircles = true;

    [Header("UI Settings")]
    public float minButtonSize = 44f; // iOS minimum recommended touch target
    public float defaultButtonScale = 1f;

    [Header("Drag Indicators")]
    public Image dragIndicator;
    public Color dragStartColor = Color.white;
    public Color dragValidColor = Color.green;
    public Color dragInvalidColor = Color.red;

    private TouchInputManager touchManager;
    private GameObject currentFeedback;

    void Start()
    {
        touchManager = TouchInputManager.Instance;

        if (touchManager == null)
        {
            Debug.LogWarning("TouchInputManager not found. Creating one...");
            GameObject touchManagerObj = new GameObject("TouchInputManager");
            touchManager = touchManagerObj.AddComponent<TouchInputManager>();
        }

        // Scale UI elements for mobile touch targets
        ScaleUIForTouch();

        // Setup drag indicator
        if (dragIndicator != null)
        {
            dragIndicator.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Show touch feedback
        if (showTouchCircles && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                ShowTouchFeedback(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                HideTouchFeedback();
            }

            // Update drag indicator
            if (touchManager != null && touchManager.IsCurrentlyDragging())
            {
                UpdateDragIndicator(touch.position);
            }
        }
        else if (Input.GetMouseButton(0))
        {
            // Mouse simulation for testing
            if (touchManager != null && touchManager.IsCurrentlyDragging())
            {
                UpdateDragIndicator(Input.mousePosition);
            }
        }
        else
        {
            HideTouchFeedback();
            if (dragIndicator != null)
            {
                dragIndicator.gameObject.SetActive(false);
            }
        }
    }

    void ShowTouchFeedback(Vector2 position)
    {
        if (touchFeedbackPrefab != null)
        {
            if (currentFeedback != null)
                Destroy(currentFeedback);

            currentFeedback = Instantiate(touchFeedbackPrefab, position, Quaternion.identity);
            currentFeedback.transform.SetParent(transform);

            // Auto-destroy after duration
            Destroy(currentFeedback, feedbackDuration);
        }
    }

    void HideTouchFeedback()
    {
        // Feedback will auto-destroy
    }

    void UpdateDragIndicator(Vector2 position)
    {
        if (dragIndicator != null)
        {
            dragIndicator.gameObject.SetActive(true);
            dragIndicator.transform.position = position;

            // Check if over valid target
            GameObject draggedObject = touchManager.GetDraggedObject();
            if (draggedObject != null)
            {
                // Check if over basket
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(position);
                worldPos.z = 0;

                Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPos, 0.5f);
                bool overBasket = false;

                foreach (Collider2D collider in colliders)
                {
                    if (collider.GetComponent<Basket>() != null)
                    {
                        overBasket = true;
                        break;
                    }
                }

                dragIndicator.color = overBasket ? dragValidColor : dragInvalidColor;
            }
        }
    }

    void ScaleUIForTouch()
    {
        // Find all buttons and scale them to minimum touch target
        Button[] buttons = FindObjectsOfType<Button>();

        foreach (Button button in buttons)
        {
            RectTransform rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Ensure button is at least 44x44 (iOS recommendation)
                float minSize = minButtonSize * defaultButtonScale;

                if (rect.rect.width < minSize || rect.rect.height < minSize)
                {
                    // Scale up the button
                    float scaleX = Mathf.Max(1f, minSize / rect.rect.width);
                    float scaleY = Mathf.Max(1f, minSize / rect.rect.height);

                    rect.localScale = new Vector3(scaleX, scaleY, 1f);
                }
            }
        }
    }

    // Call this when device orientation changes
    public void OnOrientationChanged(ScreenOrientation newOrientation)
    {
        // Handle orientation changes
        ResponsiveCanvasScaler scaler = GetComponent<ResponsiveCanvasScaler>();
        if (scaler != null)
        {
            scaler.OnScreenOrientationChanged();
        }

        // Recalculate UI scale if needed
        ScaleUIForTouch();
    }
}