using UnityEngine;

public class TimeGameInitializer : MonoBehaviour
{
    [Header("Vitamin Types")]
    public TimeObjectTypeSO[] objectTypes;

    [Header("References")]
    public TimeObjectSpawner objectSpawner;

    [Header("Timer Settings")]
    public float startingTime = 10f;
    public float timeBonusForCorrectCatch = 1f;

    void Start()
    {
        InitializeGame();
        ApplyTimerSettings();
    }

    void InitializeGame()
    {
        if (objectSpawner != null && objectTypes.Length > 0)
        {
            objectSpawner.allObjectTypes.Clear();

            foreach (var objTypeSO in objectTypes)
            {
                TimeObjectType objectType = objTypeSO.ToTimeObjectType();
                objectSpawner.AddObjectType(objectType, objTypeSO.vitaminType, 1f);
            }

            Debug.Log($"Time Game Initializer: Added {objectTypes.Length} vitamin types");
        }
        else
        {
            Debug.LogWarning("TimeGameInitializer: Missing references!");
        }

        if (TimeBasketManager.Instance == null)
        {
            Debug.LogWarning("TimeBasketManager not found!");
        }
    }

    void ApplyTimerSettings()
    {
        if (TimeGameManager.Instance != null)
        {
            TimeGameManager.Instance.startingTime = startingTime;
            TimeGameManager.Instance.timeBonusForCorrectCatch = timeBonusForCorrectCatch;
            Debug.Log($"Timer settings applied: Start={startingTime}s, Bonus={timeBonusForCorrectCatch}s");
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (objectSpawner == null)
            objectSpawner = FindFirstObjectByType<TimeObjectSpawner>();
    }
#endif
}