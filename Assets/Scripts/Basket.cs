using UnityEngine;

[System.Serializable]
public class HealthStage
{
    public Sprite sprite;
    public Color color = Color.white;
}

public class Basket : MonoBehaviour
{
    public string basketName;
    public string[] acceptedObjectTypes; // Which object types this basket accepts

    [Header("Health Stages")]
    public HealthStage[] unhealthyStages = new HealthStage[3]; // Index 0 = worst, 2 = least bad
    public HealthStage neutralStage;
    public HealthStage[] healthyStages = new HealthStage[3]; // Index 0 = least healthy, 2 = healthiest

    [Header("References")]
    private SpriteRenderer spriteRenderer;
    private int currentHealthStage = 0; // 0 = neutral, negative = unhealthy, positive = healthy
    public int CurrentHealthStage => currentHealthStage;

    [Header("Checkmark Settings")]
    public GameObject checkmarkPrefab; // Assign a green checkmark prefab
    private GameObject checkmarkInstance;
    public Vector3 checkmarkOffset = new Vector3(0, 1.5f, 0);
    private bool isAtMaxHealth = false;

    // Store the original scale to prevent size changes
    private Vector3 originalScale;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Store original scale
        originalScale = transform.localScale;
    }

    void Start()
    {
        UpdateAppearance();
        RemoveCheckmark();
    }

    public void HandleObjectCaught(FallingObject fallingObject)
    {
        // Check if basket is already at max health
        if (isAtMaxHealth)
        {
            Debug.Log($"{basketName} is already at max health! Cannot add more.");
            // Destroy the object but don't change health
            Destroy(fallingObject.gameObject);
            return;
        }

        bool isCorrectObject = false;

        // Check if object type is accepted
        foreach (string acceptedType in acceptedObjectTypes)
        {
            if (fallingObject.objectType.typeName == acceptedType)
            {
                isCorrectObject = true;
                break;
            }
        }

        if (isCorrectObject)
        {
            // Move towards healthier stage
            if (currentHealthStage < 3)
            {
                currentHealthStage++;
            }

            // Play correct catch sound
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCorrectCatchSound();

            // Add points using GameManager's settings
            if (GameManager.Instance != null)
            {
                int pointsToAdd = GameManager.Instance.pointsForCorrectCatch + fallingObject.objectType.pointValue;
                GameManager.Instance.AddScore(pointsToAdd);
            }

            // Play point gain sound
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayPointGainSound();

            Debug.Log("Correct object caught by " + basketName + "!");
        }
        else
        {
            // Move towards unhealthier stage
            if (currentHealthStage > -3)
            {
                currentHealthStage--;
            }

            // Play wrong catch sound
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayWrongCatchSound();

            // Deduct points using GameManager's settings
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(GameManager.Instance.pointsForWrongCatch);
            }

            // Play point lose sound
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayPointLoseSound();

            Debug.Log("Wrong object caught by " + basketName + "!");
        }

        // Update basket appearance
        UpdateAppearance();

        // Check if at max health
        CheckMaxHealth();

        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateBasketHealth(GetInstanceID(), currentHealthStage);
        }

        // Destroy the object
        Destroy(fallingObject.gameObject);
    }

    void UpdateAppearance()
    {
        if (spriteRenderer == null) return;

        HealthStage currentStage = GetCurrentHealthStage();

        if (currentStage != null)
        {
            spriteRenderer.sprite = currentStage.sprite;
            spriteRenderer.color = currentStage.color;

            // IMPORTANT: Reset scale to original to prevent shrinking
            transform.localScale = originalScale;
        }
    }

    HealthStage GetCurrentHealthStage()
    {
        if (currentHealthStage == 0)
        {
            return neutralStage;
        }
        else if (currentHealthStage > 0) // Healthy stages
        {
            int index = Mathf.Min(currentHealthStage - 1, healthyStages.Length - 1);
            if (index >= 0 && index < healthyStages.Length)
                return healthyStages[index];
        }
        else // Unhealthy stages
        {
            int index = Mathf.Min(Mathf.Abs(currentHealthStage) - 1, unhealthyStages.Length - 1);
            if (index >= 0 && index < unhealthyStages.Length)
                return unhealthyStages[index];
        }

        return neutralStage;
    }

    void CheckMaxHealth()
    {
        if (currentHealthStage >= 3 && !isAtMaxHealth)
        {
            isAtMaxHealth = true;
            ShowCheckmark();
            Debug.Log($"{basketName} reached max health! Checkmark added.");
        }
    }

    void ShowCheckmark()
    {
        RemoveCheckmark(); // Remove any existing checkmark

        if (checkmarkPrefab != null)
        {
            checkmarkInstance = Instantiate(checkmarkPrefab, transform.position + checkmarkOffset, Quaternion.identity);
            checkmarkInstance.transform.SetParent(transform);

            Debug.Log($"Checkmark added to {basketName}");
        }
        else
        {
            Debug.LogWarning($"No checkmark prefab assigned to {basketName}!");
        }
    }

    void RemoveCheckmark()
    {
        if (checkmarkInstance != null)
        {
            Destroy(checkmarkInstance);
            checkmarkInstance = null;
        }
    }

    public void ResetBasket()
    {
        currentHealthStage = 0;
        isAtMaxHealth = false;
        RemoveCheckmark();
        transform.localScale = originalScale; // Reset scale
        UpdateAppearance();
    }

    void OnDrawGizmos()
    {
        // Visualize basket area
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}