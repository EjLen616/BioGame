using UnityEngine;
using System;

public class TimeBasket : MonoBehaviour
{
    public event Action<TimeBasket> OnBasketCompleted;
    public event Action<TimeBasket> OnBasketFailed;

    public string BasketName => basketType != null ? basketType.basketName : "Unknown";
    public int CurrentHealthStage => currentHealthStage;

    private TimeBasketTypeSO basketType;
    private SpriteRenderer spriteRenderer;
    private Collider2D basketCollider;
    private int currentHealthStage = 0;
    private bool isCompleted = false;
    private bool isFailed = false;
    private Vector3 fixedScale = Vector3.one;
    private GameObject checkmarkInstance;
    private GameObject redXInstance;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        basketCollider = GetComponent<Collider2D>();
        fixedScale = transform.localScale;

        if (basketCollider != null)
        {
            basketCollider.isTrigger = true;
        }
    }

    void Start()
    {
        if (basketCollider != null)
        {
            basketCollider.isTrigger = true;
        }
        transform.localScale = fixedScale;
    }

    public void InitializeBasket(TimeBasketTypeSO type)
    {
        if (type == null)
        {
            Debug.LogError("TimeBasket.InitializeBasket: type is null!");
            return;
        }

        basketType = type;
        currentHealthStage = 0;
        isCompleted = false;
        isFailed = false;
        transform.localScale = fixedScale;
        UpdateAppearance();
        RemoveAllIndicators();

        if (basketCollider != null)
        {
            basketCollider.isTrigger = true;
        }

        Debug.Log($"Initialized basket: {BasketName}");
    }

    public void HandleObjectCaught(TimeFallingObject fallingObject)
    {
        if (fallingObject.isVirus)
        {
            Debug.Log($"Virus cannot be caught by basket! Destroying.");
            Destroy(fallingObject.gameObject);
            return;
        }

        if (isCompleted || isFailed)
        {
            Destroy(fallingObject.gameObject);
            return;
        }

        if (basketType == null)
        {
            Debug.LogError($"Basket type is null! {gameObject.name}");
            Destroy(fallingObject.gameObject);
            return;
        }

        bool isCorrect = basketType.AcceptsVitamin(fallingObject.vitaminType);

        if (isCorrect)
        {
            if (currentHealthStage < 3)
                currentHealthStage++;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCorrectCatchSound();

            if (TimeGameManager.Instance != null)
            {
                int points = TimeGameManager.Instance.pointsForCorrectCatch + fallingObject.objectType.pointValue;
                TimeGameManager.Instance.AddScore(points);

                // NEW: Add time bonus for correct catch
                TimeGameManager.Instance.AddTimeBonus(TimeGameManager.Instance.timeBonusForCorrectCatch);
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayPointGainSound();
        }
        else
        {
            if (currentHealthStage > -3)
                currentHealthStage--;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayWrongCatchSound();

            if (TimeGameManager.Instance != null)
            {
                TimeGameManager.Instance.AddScore(TimeGameManager.Instance.pointsForWrongCatch);
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayPointLoseSound();
        }

        UpdateAppearance();
        CheckCompletion();

        if (TimeGameManager.Instance != null)
        {
            TimeGameManager.Instance.UpdateBasketHealth(GetInstanceID(), currentHealthStage);
        }

        Destroy(fallingObject.gameObject);
    }

    void CheckCompletion()
    {
        if (currentHealthStage >= 3 && !isCompleted)
        {
            isCompleted = true;
            ShowCheckmark();
            OnBasketCompleted?.Invoke(this);
            Debug.Log($"{BasketName} COMPLETED!");
        }
        else if (currentHealthStage <= -3 && !isFailed)
        {
            isFailed = true;
            ShowRedX();
            OnBasketFailed?.Invoke(this);
            Debug.Log($"{BasketName} FAILED!");
        }
    }

    void UpdateAppearance()
    {
        if (spriteRenderer == null || basketType == null) return;

        if (basketType.healthStages == null || basketType.healthStages.Length == 0)
        {
            Debug.LogError($"BasketType {BasketName} has no health stages!");
            return;
        }

        int index = currentHealthStage + 3;
        index = Mathf.Clamp(index, 0, basketType.healthStages.Length - 1);

        spriteRenderer.sprite = basketType.healthStages[index];

        if (basketType.stageColors != null && index < basketType.stageColors.Length)
        {
            spriteRenderer.color = basketType.stageColors[index];
        }

        transform.localScale = fixedScale;
    }

    void ShowCheckmark()
    {
        RemoveAllIndicators();
        if (basketType != null && basketType.checkmarkPrefab != null)
        {
            checkmarkInstance = Instantiate(basketType.checkmarkPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            checkmarkInstance.transform.SetParent(transform);
        }
    }

    void ShowRedX()
    {
        RemoveAllIndicators();
        if (basketType != null && basketType.redXPrefab != null)
        {
            redXInstance = Instantiate(basketType.redXPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            redXInstance.transform.SetParent(transform);
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

    void OnDrawGizmos()
    {
        if (basketCollider != null)
        {
            Gizmos.color = Color.green;
            if (basketCollider is BoxCollider2D box)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
            }
            else if (basketCollider is CircleCollider2D circle)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
        }
    }
}