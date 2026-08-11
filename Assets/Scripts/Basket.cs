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
    public string[] acceptedObjectTypes;

    [Header("Health Stages")]
    public HealthStage[] unhealthyStages = new HealthStage[3];
    public HealthStage neutralStage;
    public HealthStage[] healthyStages = new HealthStage[3];

    [Header("References")]
    private SpriteRenderer spriteRenderer;
    private int currentHealthStage = 0;
    public int CurrentHealthStage => currentHealthStage;

    [Header("Checkmark Settings")]
    public GameObject checkmarkPrefab; // Green checkmark
    public GameObject redXPrefab; // Red X for worst state

    private GameObject checkmarkInstance;
    private GameObject redXInstance;

    public Vector3 checkmarkOffset = new Vector3(0, 1.5f, 0);
    public Vector3 redXOffset = new Vector3(0, 1.5f, 0);

    private bool isAtMaxHealth = false;
    private bool isAtWorstHealth = false;
    private Vector3 originalScale;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    void Start()
    {
        UpdateAppearance();
        RemoveAllIndicators();
    }

    public void HandleObjectCaught(FallingObject fallingObject)
    {
        // Check if basket is already at max health
        if (isAtMaxHealth)
        {
            Debug.Log($"{basketName} is already at max health! Cannot add more.");
            Destroy(fallingObject.gameObject);
            return;
        }

        // Check if basket is already at worst health
        if (isAtWorstHealth)
        {
            Debug.Log($"{basketName} is already at worst health! Cannot add more.");
            Destroy(fallingObject.gameObject);
            return;
        }

        bool isCorrectObject = false;

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
            if (currentHealthStage < 3)
            {
                currentHealthStage++;
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCorrectCatchSound();

            if (GameManager.Instance != null)
            {
                int pointsToAdd = GameManager.Instance.pointsForCorrectCatch + fallingObject.objectType.pointValue;
                GameManager.Instance.AddScore(pointsToAdd);
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayPointGainSound();

            Debug.Log("Correct object caught by " + basketName + "!");
        }
        else
        {
            if (currentHealthStage > -3)
            {
                currentHealthStage--;
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayWrongCatchSound();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(GameManager.Instance.pointsForWrongCatch);
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayPointLoseSound();

            Debug.Log("Wrong object caught by " + basketName + "!");
        }

        UpdateAppearance();
        CheckHealthStates();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateBasketHealth(GetInstanceID(), currentHealthStage);
        }

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
            transform.localScale = originalScale;
        }
    }

    HealthStage GetCurrentHealthStage()
    {
        if (currentHealthStage == 0)
        {
            return neutralStage;
        }
        else if (currentHealthStage > 0)
        {
            int index = Mathf.Min(currentHealthStage - 1, healthyStages.Length - 1);
            if (index >= 0 && index < healthyStages.Length)
                return healthyStages[index];
        }
        else
        {
            int index = Mathf.Min(Mathf.Abs(currentHealthStage) - 1, unhealthyStages.Length - 1);
            if (index >= 0 && index < unhealthyStages.Length)
                return unhealthyStages[index];
        }

        return neutralStage;
    }

    void CheckHealthStates()
    {
        // Check for max health (Green checkmark)
        if (currentHealthStage >= 3 && !isAtMaxHealth)
        {
            isAtMaxHealth = true;
            ShowCheckmark();
            Debug.Log($"{basketName} reached max health! Green checkmark added.");
        }

        // Check for worst health (Red X)
        if (currentHealthStage <= -3 && !isAtWorstHealth)
        {
            isAtWorstHealth = true;
            ShowRedX();
            Debug.Log($"{basketName} reached worst health! Red X added.");
        }
    }

    void ShowCheckmark()
    {
        RemoveAllIndicators();

        if (checkmarkPrefab != null)
        {
            checkmarkInstance = Instantiate(checkmarkPrefab, transform.position + checkmarkOffset, Quaternion.identity);
            checkmarkInstance.transform.SetParent(transform);
            Debug.Log($"Green checkmark added to {basketName}");
        }
        else
        {
            Debug.LogWarning($"No checkmark prefab assigned to {basketName}!");
        }
    }

    void ShowRedX()
    {
        RemoveAllIndicators();

        if (redXPrefab != null)
        {
            redXInstance = Instantiate(redXPrefab, transform.position + redXOffset, Quaternion.identity);
            redXInstance.transform.SetParent(transform);
            Debug.Log($"Red X added to {basketName}");
        }
        else
        {
            Debug.LogWarning($"No red X prefab assigned to {basketName}!");
        }
    }

    void RemoveAllIndicators()
    {
        if (checkmarkInstance != null)
        {
            Destroy(checkmarkInstance);
            checkmarkInstance = null;
        }

        if (redXInstance != null)
        {
            Destroy(redXInstance);
            redXInstance = null;
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

    void RemoveRedX()
    {
        if (redXInstance != null)
        {
            Destroy(redXInstance);
            redXInstance = null;
        }
    }

    public void ResetBasket()
    {
        currentHealthStage = 0;
        isAtMaxHealth = false;
        isAtWorstHealth = false;
        RemoveAllIndicators();
        transform.localScale = originalScale;
        UpdateAppearance();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}