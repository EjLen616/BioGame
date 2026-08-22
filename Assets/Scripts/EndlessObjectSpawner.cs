using UnityEngine;
using System.Collections.Generic;

public class EndlessObjectSpawner : MonoBehaviour
{
    public static EndlessObjectSpawner Instance { get; private set; }

    [System.Serializable]
    public class SpawnSettings
    {
        public EndlessObjectType objectType;
        public string vitaminType; // A, B, C, D, E, K
        public float spawnWeight = 1f;
        public bool isVirus = false; // NEW: Flag to identify virus
    }

    [Header("Spawn Settings")]
    public List<SpawnSettings> allObjectTypes = new List<SpawnSettings>();
    public GameObject fallingObjectPrefab;

    [Header("Virus Settings")]
    public EndlessObjectType virusObjectType; // NEW: Virus type data
    public GameObject virusPrefab; // NEW: Separate prefab for virus
    public float virusSpawnIntervalMin = 10f; // Minimum seconds between virus spawns
    public float virusSpawnIntervalMax = 20f; // Maximum seconds between virus spawns
    public int virusPointPenalty = -50; // Points lost when virus is clicked

    [Header("Spawn Parameters")]
    public float initialSpawnRate = 1f;
    public float spawnRateIncrease = 0.01f;
    public float minSpawnRate = 0.3f;
    public int maxObjectsOnScreen = 15;

    [Header("Spawn Area")]
    public float spawnWidth = 8f;
    public float spawnYPosition = 6f;

    [Header("Global Speed Settings")]
    public float baseFallSpeed = 3f;
    public float speedIncreaseInterval = 3f;
    public float speedIncreaseAmount = 0.5f;
    public float maxFallSpeed = 10f;

    private List<string> allowedVitaminTypes = new List<string>();
    private float currentSpawnRate;
    private float nextSpawnTime = 0f;
    private float nextVirusSpawnTime = 0f;
    private int currentObjectsCount = 0;

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
        currentSpawnRate = initialSpawnRate;
        nextSpawnTime = Time.time + currentSpawnRate;
        ScheduleNextVirus();

        Debug.Log($"EndlessObjectSpawner started with {allObjectTypes.Count} vitamin types");
    }

    void ScheduleNextVirus()
    {
        float interval = Random.Range(virusSpawnIntervalMin, virusSpawnIntervalMax);
        nextVirusSpawnTime = Time.time + interval;
        Debug.Log($"Next virus scheduled in {interval} seconds");
    }

    void Update()
    {
        // Check for virus spawn (only if virus prefab is assigned)
        if (virusPrefab != null && Time.time >= nextVirusSpawnTime && currentObjectsCount < maxObjectsOnScreen)
        {
            SpawnVirus();
            ScheduleNextVirus();
        }

        // Check for vitamin spawn
        if (Time.time >= nextSpawnTime && currentObjectsCount < maxObjectsOnScreen && allowedVitaminTypes.Count > 0)
        {
            SpawnObject();
            nextSpawnTime = Time.time + currentSpawnRate;
            currentSpawnRate = Mathf.Max(minSpawnRate, currentSpawnRate - spawnRateIncrease);
        }
    }

    void SpawnVirus()
    {
        if (virusPrefab == null)
        {
            Debug.LogWarning("Virus prefab not assigned!");
            return;
        }

        float spawnX = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        Vector3 spawnPosition = new Vector3(spawnX, spawnYPosition, 0);

        GameObject newVirus = Instantiate(virusPrefab, spawnPosition, Quaternion.identity);

        EndlessFallingObject fallingObject = newVirus.GetComponent<EndlessFallingObject>();
        if (fallingObject != null)
        {
            // Set as virus
            fallingObject.SetAsVirus(virusObjectType, virusPointPenalty);
            fallingObject.baseFallSpeed = baseFallSpeed * 0.8f; // Virus falls slightly slower
            fallingObject.speedIncreaseInterval = speedIncreaseInterval;
            fallingObject.speedIncreaseAmount = speedIncreaseAmount;
            fallingObject.maxFallSpeed = maxFallSpeed;
            fallingObject.ResetFallSpeed();

            Debug.Log($"Virus spawned at {spawnPosition}");
        }

        currentObjectsCount++;
    }

    void SpawnObject()
    {
        if (fallingObjectPrefab == null || allObjectTypes.Count == 0)
        {
            Debug.LogError("Missing prefab or spawnable objects!");
            return;
        }

        // Get all spawnable types that are currently allowed (excluding viruses)
        List<SpawnSettings> availableTypes = new List<SpawnSettings>();
        foreach (var setting in allObjectTypes)
        {
            if (!setting.isVirus && allowedVitaminTypes.Contains(setting.vitaminType))
            {
                availableTypes.Add(setting);
            }
        }

        if (availableTypes.Count == 0)
        {
            Debug.LogWarning($"No available vitamin types for current baskets! Allowed: {string.Join(", ", allowedVitaminTypes)}");
            return;
        }

        // Calculate total weight
        float totalWeight = 0f;
        foreach (SpawnSettings setting in availableTypes)
        {
            totalWeight += setting.spawnWeight;
        }

        // Random selection based on weight
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        SpawnSettings selected = availableTypes[0];

        foreach (SpawnSettings setting in availableTypes)
        {
            cumulativeWeight += setting.spawnWeight;
            if (randomValue <= cumulativeWeight)
            {
                selected = setting;
                break;
            }
        }

        // Spawn position
        float spawnX = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        Vector3 spawnPosition = new Vector3(spawnX, spawnYPosition, 0);

        GameObject newObject = Instantiate(fallingObjectPrefab, spawnPosition, Quaternion.identity);

        EndlessFallingObject fallingObject = newObject.GetComponent<EndlessFallingObject>();
        if (fallingObject != null)
        {
            fallingObject.SetObjectType(selected.objectType, selected.vitaminType);
            fallingObject.baseFallSpeed = baseFallSpeed;
            fallingObject.speedIncreaseInterval = speedIncreaseInterval;
            fallingObject.speedIncreaseAmount = speedIncreaseAmount;
            fallingObject.maxFallSpeed = maxFallSpeed;
            fallingObject.ResetFallSpeed();

            Debug.Log($"Spawned vitamin: {selected.objectType.typeName} ({selected.vitaminType})");
        }

        currentObjectsCount++;
    }

    public void SetAllowedVitamins(List<string> vitamins)
    {
        allowedVitaminTypes = vitamins;
        Debug.Log($"Allowed vitamins updated: {string.Join(", ", allowedVitaminTypes)}");
    }

    public void ObjectDestroyed()
    {
        currentObjectsCount = Mathf.Max(0, currentObjectsCount - 1);
    }

    public void AddObjectType(EndlessObjectType type, string vitaminType, float weight = 1f)
    {
        SpawnSettings newSetting = new SpawnSettings
        {
            objectType = type,
            vitaminType = vitaminType,
            spawnWeight = weight,
            isVirus = false
        };
        allObjectTypes.Add(newSetting);
        Debug.Log($"Added vitamin type: {vitaminType} ({type.typeName}) with weight: {weight}");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(0, spawnYPosition, 0);
        Vector3 size = new Vector3(spawnWidth, 0.5f, 0);
        Gizmos.DrawWireCube(center, size);
    }
}