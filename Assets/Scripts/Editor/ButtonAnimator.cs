using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Hover Settings")]
    public float hoverScale = 1.1f;      // How big when hovering
    public float hoverSpeed = 10f;       // How fast to scale

    [Header("Click Settings")]
    public float clickScale = 0.95f;     // How small when clicked
    public float clickSpeed = 20f;       // How fast to click scale

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovering = false;
    private bool isPressed = false;

    void Start()
    {
        // Store the original scale set in the editor
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // Smoothly animate towards target scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * GetCurrentSpeed());
    }

    float GetCurrentSpeed()
    {
        if (isPressed) return clickSpeed;
        if (isHovering) return hoverSpeed;
        return hoverSpeed;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        if (!isPressed)
        {
            targetScale = originalScale * hoverScale;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        isPressed = false;
        targetScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        targetScale = originalScale * clickScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        if (isHovering)
        {
            targetScale = originalScale * hoverScale;
        }
        else
        {
            targetScale = originalScale;
        }
    }

    // Reset scale when the component is disabled (e.g., panel closed)
    void OnDisable()
    {
        transform.localScale = originalScale;
        targetScale = originalScale;
        isHovering = false;
        isPressed = false;
    }
}