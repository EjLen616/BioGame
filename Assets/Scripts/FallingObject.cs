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
    public float fallSpeed = 3f;
    public float rotationSpeed = 50f;
    public float wiggleAmount = 0.5f;
    public float wiggleSpeed = 2f;

    [Header("References")]
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private bool isBeingDragged = false;
    private Vector3 startPosition;
    private float initialY;

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