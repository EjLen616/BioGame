using UnityEngine;

[System.Serializable]
public class ObjectType
{
    public string typeName;
    public Sprite sprite;
    public Color color = Color.white;
    public int pointValue = 10;
    public string[] acceptedBaskets;
}

public class FallingObject : MonoBehaviour
{
    public ObjectType objectType;

    [Header("Movement Settings")]
    public float fallSpeed = 3f;
    public float rotationSpeed = 50f;
    public float wiggleAmount = 0.5f;
    public float wiggleSpeed = 2f;

    [Header("Touch Feedback")]
    public float scaleOnDrag = 1.2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private bool isBeingDragged = false;
    private Vector3 startPosition;
    private float initialY;
    private Vector3 originalScale;
    private bool isReleased = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        originalScale = transform.localScale;
    }

    void Start()
    {
        InitializeObject();
        initialY = transform.position.y;
    }

    void InitializeObject()
    {
        if (objectType != null)
        {
            if (spriteRenderer != null && objectType.sprite != null)
            {
                spriteRenderer.sprite = objectType.sprite;
                spriteRenderer.color = objectType.color;
            }
        }

        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    void Update()
    {
        if (!isBeingDragged && !isReleased)
        {
            // Fall downwards
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;

            // Add some rotation
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            // Add slight wiggle for visual interest
            float wiggle = Mathf.Sin(Time.time * wiggleSpeed + initialY) * wiggleAmount * Time.deltaTime;
            transform.position += new Vector3(wiggle, 0, 0);

            // Destroy if below screen
            if (transform.position.y < -10f)
            {
                Destroy(gameObject);
            }
        }
    }

    // Touch/Mouse methods
    public void OnTouchStart()
    {
        isBeingDragged = true;
        startPosition = transform.position;

        // Scale up slightly for visual feedback
        transform.localScale = originalScale * scaleOnDrag;
        spriteRenderer.sortingOrder = 10;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }
    }

    public void OnTouchDrag(Vector3 touchPosition)
    {
        if (isBeingDragged)
        {
            // Optional: Add smooth follow or constraints
        }
    }

    public void OnTouchEnd()
    {
        isBeingDragged = false;
        isReleased = true;

        // Reset scale
        transform.localScale = originalScale;
        spriteRenderer.sortingOrder = 0;
    }

    public void ReleaseFromDrag()
    {
        isBeingDragged = false;
        isReleased = false;
        transform.localScale = originalScale;
        spriteRenderer.sortingOrder = 0;

        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    public void SetObjectType(ObjectType type)
    {
        objectType = type;
        InitializeObject();
    }

    void OnDestroy()
    {
        ObjectSpawner spawner = FindFirstObjectByType<ObjectSpawner>();
        if (spawner != null)
        {
            spawner.ObjectDestroyed();
        }
    }
}