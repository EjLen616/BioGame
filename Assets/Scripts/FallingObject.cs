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
    public float baseFallSpeed = 3f;
    public float currentFallSpeed;
    public float rotationSpeed = 50f;
    public float wiggleAmount = 0.5f;
    public float wiggleSpeed = 2f;

    [Header("Speed Increase Settings")]
    public float speedIncreaseInterval = 3f;
    public float speedIncreaseAmount = 0.5f;
    public float maxFallSpeed = 10f;

    [Header("References")]
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Color originalColor;

    private bool isBeingDragged = false;
    private Vector3 startPosition;
    private float initialY;
    private float nextSpeedIncreaseTime;
    private bool hasBeenMissed = false;
    private bool isBeingDestroyed = false;

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
        isBeingDestroyed = false;

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

        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    void Update()
    {
        if (!isBeingDragged)
        {
            UpdateFallSpeed();

            transform.position += Vector3.down * currentFallSpeed * Time.deltaTime;
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            float wiggle = Mathf.Sin(Time.time * wiggleSpeed + initialY) * wiggleAmount * Time.deltaTime;
            transform.position += new Vector3(wiggle, 0, 0);

            if (transform.position.y < -10f)
            {
                HandleObjectMissed();
            }
        }
    }

    void HandleObjectMissed()
    {
        if (hasBeenMissed || isBeingDestroyed) return;
        hasBeenMissed = true;
        isBeingDestroyed = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVitaminDestroyedSound();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ObjectMissed();
        }

        Destroy(gameObject);
    }

    void UpdateFallSpeed()
    {
        if (Time.time >= nextSpeedIncreaseTime && currentFallSpeed < maxFallSpeed)
        {
            currentFallSpeed = Mathf.Min(currentFallSpeed + speedIncreaseAmount, maxFallSpeed);
            nextSpeedIncreaseTime = Time.time + speedIncreaseInterval;
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

        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = 10;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVitaminCaughtMidAirSound();
        }
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

            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = 0;

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

        if (spriteRenderer != null && objectType != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    void OnDestroy()
    {
        if (!isBeingDestroyed)
        {
            isBeingDestroyed = true;
            if (AudioManager.Instance != null && !hasBeenMissed)
            {
                AudioManager.Instance.PlayVitaminDestroyedSound();
            }
        }

        ObjectSpawner spawner = FindFirstObjectByType<ObjectSpawner>();
        if (spawner != null)
        {
            spawner.ObjectDestroyed();
        }
    }
}