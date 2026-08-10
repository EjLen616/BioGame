using UnityEngine;

[System.Serializable]
public class ObjectType
{
    public string typeName;
    public Sprite sprite;
    public Color color = Color.white;
    public int pointValue = 10;
    public string[] acceptedBaskets; // Which baskets accept this object
}

public class FallingObject : MonoBehaviour
{
    public ObjectType objectType;

    [Header("Movement Settings")]
    public float baseFallSpeed = 3f;
    public float currentFallSpeed;
    public float rotationSpeed = 50f;
    public float wiggleAmount = 0.5f;
    public float wiggleSpeed = 2f;

    [Header("Speed Increase Settings")]
    public float speedIncreaseInterval = 3f; // How often speed increases (seconds)
    public float speedIncreaseAmount = 0.5f; // How much speed increases each time
    public float maxFallSpeed = 10f; // Maximum speed limit

    [Header("References")]
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Color originalColor; // Store original color

    private bool isBeingDragged = false;
    private Vector3 startPosition;
    private float initialY;
    private float nextSpeedIncreaseTime;
    private bool hasBeenMissed = false; // Prevents double penalty

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void Start()
    {
        InitializeObject();
        initialY = transform.position.y;
        currentFallSpeed = baseFallSpeed;
        nextSpeedIncreaseTime = Time.time + speedIncreaseInterval;
        hasBeenMissed = false;

        // Store original color
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void InitializeObject()
    {
        if (objectType != null)
        {
            if (spriteRenderer != null && objectType.sprite != null)
            {
                spriteRenderer.sprite = objectType.sprite;
                spriteRenderer.color = objectType.color;
                originalColor = objectType.color;
            }
        }

        // Set gravity scale to 0 since we'll control falling manually
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    void Update()
    {
        if (!isBeingDragged)
        {
            // Increase speed over time
            UpdateFallSpeed();

            // Fall downwards with current speed
            transform.position += Vector3.down * currentFallSpeed * Time.deltaTime;

            // Add some rotation
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            // Add slight wiggle for visual interest
            float wiggle = Mathf.Sin(Time.time * wiggleSpeed + initialY) * wiggleAmount * Time.deltaTime;
            transform.position += new Vector3(wiggle, 0, 0);

            // Destroy if below screen and apply penalty
            if (transform.position.y < -10f)
            {
                HandleObjectMissed();
            }
        }
    }

    void HandleObjectMissed()
    {
        // Prevent double penalty
        if (hasBeenMissed) return;
        hasBeenMissed = true;

        // Notify GameManager about missed object
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ObjectMissed();
        }

        // Destroy the object
        Destroy(gameObject);
    }

    void UpdateFallSpeed()
    {
        // Check if it's time to increase speed
        if (Time.time >= nextSpeedIncreaseTime && currentFallSpeed < maxFallSpeed)
        {
            currentFallSpeed = Mathf.Min(currentFallSpeed + speedIncreaseAmount, maxFallSpeed);
            nextSpeedIncreaseTime = Time.time + speedIncreaseInterval;

            // REMOVED: Color change that was making vitamins red
            // Now just log the speed change
            Debug.Log($"Speed increased to {currentFallSpeed}");
        }
    }

    void OnMouseDown()
    {
        isBeingDragged = true;
        startPosition = transform.position;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        // Bring to front while dragging
        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = 10;
    }

    void OnMouseDrag()
    {
        if (isBeingDragged)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            transform.position = mousePos;
        }
    }

    void OnMouseUp()
    {
        if (isBeingDragged)
        {
            isBeingDragged = false;

            // Reset sorting order
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = 0;

            // Check if over a basket
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);

            bool caughtByBasket = false;
            foreach (Collider2D collider in colliders)
            {
                Basket basket = collider.GetComponent<Basket>();
                if (basket != null)
                {
                    basket.HandleObjectCaught(this);
                    caughtByBasket = true;
                    break;
                }
            }

            // If not caught by basket, resume falling
            if (!caughtByBasket)
            {
                if (rb != null)
                {
                    rb.gravityScale = 0f;
                }
            }
        }
    }

    public void SetObjectType(ObjectType type)
    {
        objectType = type;
        InitializeObject();
    }

    public void ResetFallSpeed()
    {
        currentFallSpeed = baseFallSpeed;
        nextSpeedIncreaseTime = Time.time + speedIncreaseInterval;

        // Reset color to original
        if (spriteRenderer != null && objectType != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    void OnDestroy()
    {
        // Notify ObjectSpawner if needed
        ObjectSpawner spawner = FindFirstObjectByType<ObjectSpawner>();
        if (spawner != null)
        {
            spawner.ObjectDestroyed();
        }
    }
}