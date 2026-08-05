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
            if (currentHealthStage < maxHealthStages)
            {
                currentHealthStage++;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(pointsForCorrect + fallingObject.objectType.pointValue);
            }

            // Play correct sound
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCorrectCatchSound();

            // Haptic feedback for correct catch
#if UNITY_IOS
            iOSSettings.TriggerHapticFeedback();
#endif

            Debug.Log("Correct object caught by " + basketName + "!");
        }
        else
        {
            if (currentHealthStage > -maxHealthStages)
            {
                currentHealthStage--;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(pointsForWrong);
            }

            // Play wrong sound
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayWrongCatchSound();

            Debug.Log("Wrong object caught by " + basketName + "!");
        }

        UpdateAppearance();

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
}