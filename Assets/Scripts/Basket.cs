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

    [Header("Settings")]
    public int maxHealthStages = 3;
    public int pointsForCorrect = 20;
    public int pointsForWrong = -10;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateAppearance();
    }

    public void HandleObjectCaught(FallingObject fallingObject)
    {
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
            if (currentHealthStage < maxHealthStages)
            {
                currentHealthStage++;
            }

            // Add points
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(pointsForCorrect + fallingObject.objectType.pointValue);
            }

            Debug.Log("Correct object caught by " + basketName + "!");
        }
        else
        {
            // Move towards unhealthier stage
            if (currentHealthStage > -maxHealthStages)
            {
                currentHealthStage--;
            }

            // Deduct points
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(pointsForWrong);
            }

            Debug.Log("Wrong object caught by " + basketName + "!");
        }

        // Update basket appearance
        UpdateAppearance();

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

    void OnDrawGizmos()
    {
        // Visualize basket area
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}