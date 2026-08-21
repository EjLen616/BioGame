using UnityEngine;

public class EndlessGameInitializer : MonoBehaviour
{
    [Header("Vitamin Types")]
    public EndlessObjectTypeSO[] objectTypes;

    [Header("References")]
    public EndlessObjectSpawner objectSpawner;

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        if (objectSpawner != null && objectTypes.Length > 0)
        {
            objectSpawner.allObjectTypes.Clear();

            foreach (var objTypeSO in objectTypes)
            {
                EndlessObjectType objectType = objTypeSO.ToEndlessObjectType();
                objectSpawner.AddObjectType(objectType, objTypeSO.vitaminType, 1f);
            }

            Debug.Log($"Endless Game Initializer: Added {objectTypes.Length} vitamin types");
        }
        else
        {
            Debug.LogWarning("EndlessGameInitializer: Missing references!");
        }

        if (EndlessBasketManager.Instance == null)
        {
            Debug.LogWarning("EndlessBasketManager not found!");
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (objectSpawner == null)
            objectSpawner = FindFirstObjectByType<EndlessObjectSpawner>();
    }
#endif
}