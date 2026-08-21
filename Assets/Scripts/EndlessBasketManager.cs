using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EndlessBasketManager : MonoBehaviour
{
    public static EndlessBasketManager Instance { get; private set; }

    [Header("Basket Definitions (ScriptableObjects)")]
    public EndlessBasketTypeSO[] allBasketTypes;

    [Header("Scene References")]
    public Transform basketSlot1;
    public Transform basketSlot2;
    public EndlessBasket basketPrefab;

    [Header("Current Active Baskets (Auto-assigned)")]
    public EndlessBasket activeBasket1;
    public EndlessBasket activeBasket2;

    [Header("Basket Scale")]
    public Vector3 basketScale = Vector3.one; // Default scale for all baskets

    private List<EndlessBasketTypeSO> availableBasketTypes = new List<EndlessBasketTypeSO>();
    private EndlessBasketTypeSO currentBasketType1;
    private EndlessBasketTypeSO currentBasketType2;

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
            Debug.LogError("EndlessBasketManager: No basket types assigned!");
            return;
        }

        if (basketSlot1 == null || basketSlot2 == null)
        {
            Debug.LogError("EndlessBasketManager: Basket slots not assigned!");
            return;
        }

        if (basketPrefab == null)
        {
            Debug.LogError("EndlessBasketManager: Basket prefab not assigned!");
            return;
        }

        // Validate each basket type has health stages
        foreach (var type in allBasketTypes)
        {
            if (type.healthStages == null || type.healthStages.Length == 0)
            {
                Debug.LogError($"Basket type {type.basketName} has no health stages assigned!");
                return;
            }
        }

        Debug.Log($"EndlessBasketManager: Found {allBasketTypes.Length} basket types");
        Debug.Log($"EndlessBasketManager: Basket scale set to: {basketScale}");

        availableBasketTypes = new List<EndlessBasketTypeSO>(allBasketTypes);
        SpawnNewBaskets();
    }

    void SpawnNewBaskets()
    {
        Debug.Log("EndlessBasketManager: Spawning new baskets...");

        ClearActiveBaskets();

        List<EndlessBasketTypeSO> types = GetRandomBasketTypes(2);

        if (types.Count >= 2)
        {
            currentBasketType1 = types[0];
            currentBasketType2 = types[1];

            activeBasket1 = SpawnBasket(currentBasketType1, basketSlot1.position, "Slot1");
            activeBasket2 = SpawnBasket(currentBasketType2, basketSlot2.position, "Slot2");

            UpdateAvailableVitamins();

            Debug.Log($"EndlessBasketManager: Spawned baskets: {currentBasketType1.basketName} and {currentBasketType2.basketName}");
        }
        else
        {
            Debug.LogError("EndlessBasketManager: Not enough basket types available!");
        }
    }

    EndlessBasket SpawnBasket(EndlessBasketTypeSO basketType, Vector3 position, string slotName)
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

        // Set the scale immediately
        basketObj.transform.localScale = basketScale;

        EndlessBasket basket = basketObj.GetComponent<EndlessBasket>();

        if (basket != null)
        {
            basket.InitializeBasket(basketType);
            basket.OnBasketCompleted += OnBasketCompleted;
            basket.OnBasketFailed += OnBasketFailed;
            Debug.Log($"EndlessBasketManager: Successfully spawned {basketType.basketName} at scale {basketScale}");
        }
        else
        {
            Debug.LogError($"EndlessBasketManager: No EndlessBasket component on prefab!");
        }

        return basket;
    }

    List<EndlessBasketTypeSO> GetRandomBasketTypes(int count)
    {
        List<EndlessBasketTypeSO> shuffled = new List<EndlessBasketTypeSO>(availableBasketTypes);

        for (int i = 0; i < shuffled.Count; i++)
        {
            EndlessBasketTypeSO temp = shuffled[i];
            int randomIndex = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        return shuffled.Take(count).ToList();
    }

    void OnBasketCompleted(EndlessBasket basket)
    {
        Debug.Log($"Basket {basket.BasketName} completed! Replacing with new basket.");
        ReplaceBasket(basket);
    }

    void OnBasketFailed(EndlessBasket basket)
    {
        Debug.Log($"Basket {basket.BasketName} failed! Replacing with new basket.");
        ReplaceBasket(basket);
    }

    void ReplaceBasket(EndlessBasket oldBasket)
    {
        bool isSlot1 = activeBasket1 == oldBasket;
        bool isSlot2 = activeBasket2 == oldBasket;

        if (!isSlot1 && !isSlot2)
        {
            Debug.LogWarning("Basket not found in active slots!");
            return;
        }

        EndlessBasketTypeSO otherType = isSlot1 ? currentBasketType2 : currentBasketType1;
        EndlessBasketTypeSO newType = GetRandomBasketTypeExcluding(otherType);

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

    EndlessBasketTypeSO GetRandomBasketTypeExcluding(EndlessBasketTypeSO excludeType)
    {
        List<EndlessBasketTypeSO> available = new List<EndlessBasketTypeSO>(availableBasketTypes);
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

        if (EndlessObjectSpawner.Instance != null)
        {
            EndlessObjectSpawner.Instance.SetAllowedVitamins(uniqueVitamins);
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