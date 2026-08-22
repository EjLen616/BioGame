using UnityEngine;
using System.Collections.Generic;

public class TimeObjectSpawner : MonoBehaviour
{
    public static TimeObjectSpawner Instance { get; private set; }

    [System.Serializable]
    public class SpawnSettings
    {
        public TimeObjectType objectType;
        public string vitaminType;
        public float spawnWeight = 1f;
        public bool isVirus = false;
    }

    [Header("Spawn Settings")]
    public List<SpawnSettings> allObjectTypes = new List<SpawnSettings>();
    public GameObject fallingObjectPrefab;

    [Header("Virus Settings")]
    public TimeObjectType virusObjectType;
    public GameObject virusPrefab;
    public float virusSpawnIntervalMin = 10f;
    public float virusSpawnIntervalMax = 20f;
    public int virusPointPenalty = -50;

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

        Debug.Log($"TimeObjectSpawner started with {allObjectTypes.Count} vitamin types");
    }

    void ScheduleNextVirus()
    {
        float interval = Random.Range(virusSpawnIntervalMin, virusSpawnIntervalMax);
        nextVirusSpawnTime = Time.time + interval;
        Debug.Log($"Next virus scheduled in {interval} seconds");
    }

    void Update()
    {
        if (virusPrefab != null && Time.time >= nextVirusSpawnTime && currentObjectsCount < maxObjectsOnScreen)
        {
            SpawnVirus();
            ScheduleNextVirus();
        }

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

        TimeFallingObject fallingObject = newVirus.GetComponent<TimeFallingObject>();
        if (fallingObject != null)
        {
            fallingObject.SetAsVirus(virusObjectType, virusPointPenalty);
            fallingObject.baseFallSpeed = baseFallSpeed * 0.8f;
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

        float totalWeight = 0f;
        foreach (SpawnSettings setting in availableTypes)
        {
            totalWeight += setting.spawnWeight;
        }

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

        float spawnX = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        Vector3 spawnPosition = new Vector3(spawnX, spawnYPosition, 0);

        GameObject newObject = Instantiate(fallingObjectPrefab, spawnPosition, Quaternion.identity);

        TimeFallingObject fallingObject = newObject.GetComponent<TimeFallingObject>();
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

    public void AddObjectType(TimeObjectType type, string vitaminType, float weight = 1f)
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