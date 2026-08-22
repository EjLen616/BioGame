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
    public bool isVirus = false; // NEW: Flag to identify virus
    public int virusPenalty = -50; // NEW: Points lost when virus is clicked

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
    private bool isBeingClicked = false; // NEW: Prevent double virus penalty

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        if (col != null)
        {
            col.isTrigger = true;
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

        // Add pulsing effect for virus
        if (isVirus)
        {
            InvokeRepeating(nameof(PulseVirus), 0f, 0.5f);
        }
    }

    void PulseVirus()
    {
        if (spriteRenderer != null && isVirus)
        {
            // Quick flash effect to make virus stand out
            Color currentColor = spriteRenderer.color;
            float alpha = Mathf.PingPong(Time.time * 2f, 0.3f) + 0.7f;
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
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

        // Viruses don't lose points when missed
        if (!isVirus)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayVitaminDestroyedSound();

            if (EndlessGameManager.Instance != null)
                EndlessGameManager.Instance.ObjectMissed();
        }
        else
        {
            Debug.Log("Virus missed - no penalty");
        }

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
        Debug.Log($"{(isVirus ? "Virus" : "Vitamin")} clicked!");
        if (IsGamePaused() || Time.timeScale == 0f) return;

        // Virus handling - instant penalty, no dragging
        if (isVirus)
        {
            if (!isBeingClicked)
            {
                isBeingClicked = true;
                HandleVirusClicked();
            }
            return;
        }

        // Normal vitamin handling
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

    void HandleVirusClicked()
    {
        Debug.Log($"VIRUS CLICKED! Penalty: {virusPenalty} points");

        // Apply penalty
        if (EndlessGameManager.Instance != null)
        {
            EndlessGameManager.Instance.AddScore(virusPenalty);
        }

        // Play sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayWrongCatchSound();

        // Destroy virus
        isBeingDestroyed = true;
        Destroy(gameObject);
    }

    void OnMouseDrag()
    {
        if (IsGamePaused() || Time.timeScale == 0f) return;
        if (isVirus) return; // Viruses can't be dragged

        if (isBeingDragged)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            transform.position = mousePos;
        }
    }

    void OnMouseUp()
    {
        if (IsGamePaused() || Time.timeScale == 0f) return;
        if (isVirus) return; // Viruses can't be dragged

        if (isBeingDragged)
        {
            isBeingDragged = false;

            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = 0;

            // Check if dropped on a basket
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1.5f);

            foreach (Collider2D collider in colliders)
            {
                EndlessBasket basket = collider.GetComponent<EndlessBasket>();
                if (basket != null)
                {
                    basket.HandleObjectCaught(this);
                    return;
                }
            }

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
        isVirus = false;
        InitializeObject();
        Debug.Log($"Vitamin set: {type.typeName} with type {vitamin}");
    }

    public void SetAsVirus(EndlessObjectType type, int penalty)
    {
        objectType = type;
        vitaminType = "VIRUS";
        isVirus = true;
        virusPenalty = penalty;
        InitializeObject();

        // Make virus stand out - red tint
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.2f, 0.2f, 1f);
            originalColor = spriteRenderer.color;
        }

        Debug.Log($"Virus set with penalty: {penalty}");
    }

    public void ResetFallSpeed()
    {
        currentFallSpeed = baseFallSpeed;
        nextSpeedIncreaseTime = Time.time + speedIncreaseInterval;

        if (spriteRenderer != null && objectType != null && !isVirus)
        {
            spriteRenderer.color = originalColor;
        }
    }

    void OnDestroy()
    {
        if (!isBeingDestroyed)
        {
            isBeingDestroyed = true;
            if (AudioManager.Instance != null && !hasBeenMissed && !isVirus)
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