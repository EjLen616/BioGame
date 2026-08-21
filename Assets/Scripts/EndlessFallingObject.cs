using UnityEngine;

[System.Serializable]
public class EndlessObjectType
{
    public string typeName;
    public Sprite sprite;
    public Color color = Color.white;
    public int pointValue = 10;
}

public class EndlessFallingObject : MonoBehaviour
{
    public EndlessObjectType objectType;
    public string vitaminType; // "A", "B", "C", etc.

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

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Color originalColor;

    private bool isBeingDragged = false;
    private float initialY;
    private float nextSpeedIncreaseTime;
    private bool hasBeenMissed = false;
    private bool isBeingDestroyed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        // Make sure collider is set as trigger for detection
        if (col != null)
        {
            col.isTrigger = true;
            Debug.Log($"Vitamin {gameObject.name} collider set as trigger");
        }
        else
        {
            Debug.LogError($"Vitamin {gameObject.name} has no Collider2D!");
        }
    }

    void Start()
    {
        InitializeObject();
        initialY = transform.position.y;
        currentFallSpeed = baseFallSpeed;
        nextSpeedIncreaseTime = Time.time + speedIncreaseInterval;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void InitializeObject()
    {
        if (objectType != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = objectType.sprite;
            spriteRenderer.color = objectType.color;
            originalColor = objectType.color;
        }

        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    void Update()
    {
        if (IsGamePaused() || isBeingDragged) return;

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

    bool IsGamePaused()
    {
        if (Time.timeScale == 0f) return true;

        PauseMenu pauseMenu = FindFirstObjectByType<PauseMenu>();
        if (pauseMenu != null && pauseMenu.IsPaused()) return true;

        return false;
    }

    void HandleObjectMissed()
    {
        if (hasBeenMissed || isBeingDestroyed) return;
        hasBeenMissed = true;
        isBeingDestroyed = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayVitaminDestroyedSound();

        if (EndlessGameManager.Instance != null)
            EndlessGameManager.Instance.ObjectMissed();

        Destroy(gameObject);
    }

    void UpdateFallSpeed()
    {
        if (Time.time >= nextSpeedIncreaseTime && currentFallSpeed < maxFallSpeed)
        {
            currentFallSpeed = Mathf.Min(currentFallSpeed + speedIncreaseAmount, maxFallSpeed);
            nextSpeedIncreaseTime = Time.time + speedIncreaseInterval;
        }
    }

    void OnMouseDown()
    {
        Debug.Log($"Vitamin {vitaminType} clicked!");
        if (IsGamePaused() || Time.timeScale == 0f) return;

        isBeingDragged = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = 10;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayVitaminCaughtMidAirSound();
    }

    void OnMouseDrag()
    {
        if (IsGamePaused() || Time.timeScale == 0f) return;

        if (isBeingDragged)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            transform.position = mousePos;
        }
    }

    void OnMouseUp()
    {
        Debug.Log($"Vitamin {vitaminType} released!");
        if (IsGamePaused() || Time.timeScale == 0f) return;

        if (isBeingDragged)
        {
            isBeingDragged = false;

            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = 0;

            // Check if dropped on a basket using a larger radius
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1.5f);
            Debug.Log($"Found {colliders.Length} colliders near drop position");

            foreach (Collider2D collider in colliders)
            {
                Debug.Log($"Collider found: {collider.gameObject.name} with tag {collider.gameObject.tag}");

                EndlessBasket basket = collider.GetComponent<EndlessBasket>();
                if (basket != null)
                {
                    Debug.Log($"Dropped on basket: {basket.BasketName}");
                    basket.HandleObjectCaught(this);
                    return;
                }
            }

            Debug.Log($"Vitamin {vitaminType} not caught by any basket");

            // If not caught, resume falling
            if (rb != null)
            {
                rb.gravityScale = 0f;
            }
        }
    }

    public void SetObjectType(EndlessObjectType type, string vitamin)
    {
        objectType = type;
        vitaminType = vitamin;
        InitializeObject();
        Debug.Log($"Vitamin set: {type.typeName} with type {vitamin}");
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

        EndlessObjectSpawner spawner = FindFirstObjectByType<EndlessObjectSpawner>();
        if (spawner != null)
        {
            spawner.ObjectDestroyed();
        }
    }
}