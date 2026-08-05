using UnityEngine;
using UnityEngine.UI;

public class TouchFeedbackIndicator : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color startColor = new Color(1f, 1f, 1f, 0.5f);
    public Color endColor = new Color(1f, 1f, 1f, 0f);
    public float fadeDuration = 0.2f;

    private Image image;
    private RectTransform rectTransform;
    private float currentTime = 0f;

    void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        if (image != null)
        {
            image.color = startColor;
        }
    }

    void Update()
    {
        currentTime += Time.deltaTime;
        float progress = currentTime / fadeDuration;

        if (image != null)
        {
            image.color = Color.Lerp(startColor, endColor, progress);
        }

        // Scale up slightly
        if (rectTransform != null)
        {
            float scale = 1f + (progress * 0.5f);
            rectTransform.localScale = new Vector3(scale, scale, 1f);
        }

        if (progress >= 1f)
        {
            Destroy(gameObject);
        }
    }
}