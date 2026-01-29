using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("Object Types")]
    public ObjectTypeSO[] objectTypes;

    [Header("References")]
    public ObjectSpawner objectSpawner;
    public Basket[] baskets;

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        if (objectSpawner != null && objectTypes.Length >= 3)
        {
            // Clear existing spawn settings
            objectSpawner.spawnableObjects.Clear();

            // Add object types to spawner
            foreach (var objTypeSO in objectTypes)
            {
                ObjectType objectType = new ObjectType
                {
                    typeName = objTypeSO.typeName,
                    sprite = objTypeSO.sprite,
                    color = objTypeSO.color,
                    pointValue = objTypeSO.pointValue,
                    acceptedBaskets = objTypeSO.acceptedBaskets
                };

                // Add with default weight
                objectSpawner.AddObjectType(objectType, 1f);
            }
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Auto-find references in editor
        if (objectSpawner == null)
            objectSpawner = FindAnyObjectByType<ObjectSpawner>();

        if (baskets == null || baskets.Length == 0)
            baskets = FindObjectsByType<Basket>(FindObjectsSortMode.None);
    }
#endif
}