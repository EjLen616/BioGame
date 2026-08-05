using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class ResponsiveCanvasScaler : MonoBehaviour
{
    [Header("Screen Size Settings")]
    public bool scaleForMobile = true;
    public float referenceWidth = 1080f;
    public float referenceHeight = 1920f;

    [Header("Safe Area")]
    public bool adjustForSafeArea = true;
    public RectTransform[] uiElementsToAdjust;

    private CanvasScaler canvasScaler;
    private RectTransform rectTransform;
    private Rect lastSafeArea;

    void Awake()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        rectTransform = GetComponent<RectTransform>();

        // Set reference resolution for mobile
        if (scaleForMobile)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f; // Balance between width and height
        }

        // Adjust for safe area (notch phones)
        if (adjustForSafeArea)
        {
            ApplySafeArea();
        }
    }

    void Update()
    {
        // Check if safe area changed (orientation change)
        if (adjustForSafeArea && Screen.safeArea != lastSafeArea)
        {
            ApplySafeArea();
        }
    }

    void ApplySafeArea()
    {
        lastSafeArea = Screen.safeArea;

        // Convert safe area to anchor positions
        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Apply to UI elements that need safe area adjustment
        foreach (RectTransform element in uiElementsToAdjust)
        {
            if (element != null)
            {
                element.anchorMin = anchorMin;
                element.anchorMax = anchorMax;
            }
        }

        // Also adjust the main canvas
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }

    // Call this method if screen orientation changes
    public void OnScreenOrientationChanged()
    {
        if (adjustForSafeArea)
        {
            ApplySafeArea();
        }
    }
}