using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TimeBasketManager : MonoBehaviour
{
    public static TimeBasketManager Instance { get; private set; }

    [Header("Basket Definitions")]
    public TimeBasketTypeSO[] allBasketTypes;

    [Header("Scene References")]
    public Transform basketSlot1;
    public Transform basketSlot2;
    public TimeBasket basketPrefab;

    [Header("Current Active Baskets")]
    public TimeBasket activeBasket1;
    public TimeBasket activeBasket2;

    [Header("Basket Scale")]
    public Vector3 basketScale = Vector3.one;

    private List<TimeBasketTypeSO> availableBasketTypes = new List<TimeBasketTypeSO>();
    private TimeBasketTypeSO currentBasketType1;
    private TimeBasketTypeSO currentBasketType2;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (allBasketTypes == null || allBasketTypes.Length == 0)
        {
            Debug.LogError("TimeBasketManager: No basket types assigned!");
            return;
        }

        if (basketSlot1 == null || basketSlot2 == null)
        {
            Debug.LogError("TimeBasketManager: Basket slots not assigned!");
            return;
        }

        if (basketPrefab == null)
        {
            Debug.LogError("TimeBasketManager: Basket prefab not assigned!");
            return;
        }

        foreach (var type in allBasketTypes)
        {
            if (type.healthStages == null || type.healthStages.Length == 0)
            {
                Debug.LogError($"Basket type {type.basketName} has no health stages assigned!");
                return;
            }
        }

        Debug.Log($"TimeBasketManager: Found {allBasketTypes.Length} basket types");

        availableBasketTypes = new List<TimeBasketTypeSO>(allBasketTypes);
        SpawnNewBaskets();
    }

    void SpawnNewBaskets()
    {
        Debug.Log("TimeBasketManager: Spawning new baskets...");
        ClearActiveBaskets();

        List<TimeBasketTypeSO> types = GetRandomBasketTypes(2);

        if (types.Count >= 2)
        {
            currentBasketType1 = types[0];
            currentBasketType2 = types[1];

            activeBasket1 = SpawnBasket(currentBasketType1, basketSlot1.position, "Slot1");
            activeBasket2 = SpawnBasket(currentBasketType2, basketSlot2.position, "Slot2");

            UpdateAvailableVitamins();

            Debug.Log($"TimeBasketManager: Spawned baskets: {currentBasketType1.basketName} and {currentBasketType2.basketName}");
        }
        else
        {
            Debug.LogError("TimeBasketManager: Not enough basket types available!");
        }
    }

    TimeBasket SpawnBasket(TimeBasketTypeSO basketType, Vector3 position, string slotName)
    {
        if (basketPrefab == null)
        {
            Debug.LogError("Basket prefab is null!");
            return null;
        }

        if (basketType == null)
        {
            Debug.LogError("BasketType is null!");
            return null;
        }

        GameObject basketObj = Instantiate(basketPrefab.gameObject, position, Quaternion.identity);
        basketObj.name = $"Basket_{basketType.basketName}_{slotName}";
        basketObj.transform.localScale = basketScale;

        TimeBasket basket = basketObj.GetComponent<TimeBasket>();

        if (basket != null)
        {
            basket.InitializeBasket(basketType);
            basket.OnBasketCompleted += OnBasketCompleted;
            basket.OnBasketFailed += OnBasketFailed;
        }
        else
        {
            Debug.LogError($"TimeBasketManager: No TimeBasket component on prefab!");
        }

        return basket;
    }

    List<TimeBasketTypeSO> GetRandomBasketTypes(int count)
    {
        List<TimeBasketTypeSO> shuffled = new List<TimeBasketTypeSO>(availableBasketTypes);

        for (int i = 0; i < shuffled.Count; i++)
        {
            TimeBasketTypeSO temp = shuffled[i];
            int randomIndex = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        return shuffled.Take(count).ToList();
    }

    void OnBasketCompleted(TimeBasket basket)
    {
        Debug.Log($"Basket {basket.BasketName} completed! Replacing with new basket.");
        ReplaceBasket(basket);
    }

    void OnBasketFailed(TimeBasket basket)
    {
        Debug.Log($"Basket {basket.BasketName} failed! Replacing with new basket.");
        ReplaceBasket(basket);
    }

    void ReplaceBasket(TimeBasket oldBasket)
    {
        bool isSlot1 = activeBasket1 == oldBasket;
        bool isSlot2 = activeBasket2 == oldBasket;

        if (!isSlot1 && !isSlot2)
        {
            Debug.LogWarning("Basket not found in active slots!");
            return;
        }

        TimeBasketTypeSO otherType = isSlot1 ? currentBasketType2 : currentBasketType1;
        TimeBasketTypeSO newType = GetRandomBasketTypeExcluding(otherType);

        if (newType == null)
        {
            Debug.LogWarning("No available basket types!");
            return;
        }

        Destroy(oldBasket.gameObject);

        Vector3 position = isSlot1 ? basketSlot1.position : basketSlot2.position;
        string slotName = isSlot1 ? "Slot1" : "Slot2";

        if (isSlot1)
        {
            currentBasketType1 = newType;
            activeBasket1 = SpawnBasket(newType, position, slotName);
        }
        else
        {
            currentBasketType2 = newType;
            activeBasket2 = SpawnBasket(newType, position, slotName);
        }

        UpdateAvailableVitamins();
        Debug.Log($"Replaced basket with: {newType.basketName}");
    }

    TimeBasketTypeSO GetRandomBasketTypeExcluding(TimeBasketTypeSO excludeType)
    {
        List<TimeBasketTypeSO> available = new List<TimeBasketTypeSO>(availableBasketTypes);
        available.Remove(excludeType);

        if (available.Count == 0) return null;

        return available[Random.Range(0, available.Count)];
    }

    void ClearActiveBaskets()
    {
        if (activeBasket1 != null)
        {
            Destroy(activeBasket1.gameObject);
            activeBasket1 = null;
        }
        if (activeBasket2 != null)
        {
            Destroy(activeBasket2.gameObject);
            activeBasket2 = null;
        }
    }

    void UpdateAvailableVitamins()
    {
        List<string> allRequiredVitamins = new List<string>();

        if (currentBasketType1 != null)
            allRequiredVitamins.AddRange(currentBasketType1.requiredVitamins);

        if (currentBasketType2 != null)
            allRequiredVitamins.AddRange(currentBasketType2.requiredVitamins);

        List<string> uniqueVitamins = allRequiredVitamins.Distinct().ToList();

        if (TimeObjectSpawner.Instance != null)
        {
            TimeObjectSpawner.Instance.SetAllowedVitamins(uniqueVitamins);
        }

        Debug.Log($"Available vitamins: {string.Join(", ", uniqueVitamins)}");
    }

    public List<string> GetActiveBasketNames()
    {
        List<string> names = new List<string>();
        if (currentBasketType1 != null) names.Add(currentBasketType1.basketName);
        if (currentBasketType2 != null) names.Add(currentBasketType2.basketName);
        return names;
    }
}