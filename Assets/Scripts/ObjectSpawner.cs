using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnSettings
    {
        public ObjectType objectType;
        public float spawnWeight = 1f;
    }

    [Header("Spawn Settings")]
    public List<SpawnSettings> spawnableObjects = new List<SpawnSettings>();
    public GameObject fallingObjectPrefab;

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

    private float currentSpawnRate;
    private float nextSpawnTime = 0f;
    private int currentObjectsCount = 0;

    void Start()
    {
        currentSpawnRate = initialSpawnRate;
        nextSpawnTime = Time.time + currentSpawnRate;

        if (spawnableObjects.Count < 3)
        {
            Debug.LogWarning("Add at least 3 object types to the spawner!");
        }
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime && currentObjectsCount < maxObjectsOnScreen)
        {
            SpawnObject();
            nextSpawnTime = Time.time + currentSpawnRate;
            currentSpawnRate = Mathf.Max(minSpawnRate, currentSpawnRate - spawnRateIncrease);
        }
    }

    void SpawnObject()
    {
        if (fallingObjectPrefab == null || spawnableObjects.Count == 0)
        {
            Debug.LogError("Missing prefab or spawnable objects!");
            return;
        }

        float totalWeight = 0f;
        foreach (SpawnSettings settings in spawnableObjects)
        {
            totalWeight += settings.spawnWeight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        ObjectType selectedType = spawnableObjects[0].objectType;

        foreach (SpawnSettings settings in spawnableObjects)
        {
            cumulativeWeight += settings.spawnWeight;
            if (randomValue <= cumulativeWeight)
            {
                selectedType = settings.objectType;
                break;
            }
        }

        float spawnX = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        Vector3 spawnPosition = new Vector3(spawnX, spawnYPosition, 0);

        GameObject newObject = Instantiate(fallingObjectPrefab, spawnPosition, Quaternion.identity);

        FallingObject fallingObject = newObject.GetComponent<FallingObject>();
        if (fallingObject != null)
        {
            fallingObject.SetObjectType(selectedType);
            fallingObject.baseFallSpeed = baseFallSpeed;
            fallingObject.speedIncreaseInterval = speedIncreaseInterval;
            fallingObject.speedIncreaseAmount = speedIncreaseAmount;
            fallingObject.maxFallSpeed = maxFallSpeed;
            fallingObject.ResetFallSpeed();
        }

        currentObjectsCount++;
    }

    public void ObjectDestroyed()
    {
        currentObjectsCount = Mathf.Max(0, currentObjectsCount - 1);
    }

    public void UpdateGlobalSpeedSettings(float newBaseSpeed, float newInterval, float newIncrease, float newMax)
    {
        baseFallSpeed = newBaseSpeed;
        speedIncreaseInterval = newInterval;
        speedIncreaseAmount = newIncrease;
        maxFallSpeed = newMax;

        FallingObject[] allObjects = FindObjectsByType<FallingObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            obj.baseFallSpeed = baseFallSpeed;
            obj.speedIncreaseInterval = speedIncreaseInterval;
            obj.speedIncreaseAmount = speedIncreaseAmount;
            obj.maxFallSpeed = maxFallSpeed;
            obj.ResetFallSpeed();
        }
    }

    public void AddObjectType(ObjectType newType, float weight = 1f)
    {
        if (newType == null)
        {
            Debug.LogError("Cannot add null ObjectType!");
            return;
        }

        SpawnSettings newSettings = new SpawnSettings
        {
            objectType = newType,
            spawnWeight = weight
        };

        spawnableObjects.Add(newSettings);
        Debug.Log($"Added object type: {newType.typeName} with weight: {weight}");
    }

    public void RemoveObjectType(string typeName)
    {
        for (int i = spawnableObjects.Count - 1; i >= 0; i--)
        {
            if (spawnableObjects[i].objectType.typeName == typeName)
            {
                spawnableObjects.RemoveAt(i);
                Debug.Log($"Removed object type: {typeName}");
                return;
            }
        }
        Debug.LogWarning($"Object type '{typeName}' not found!");
    }

    public void ClearAllObjectTypes()
    {
        spawnableObjects.Clear();
        Debug.Log("All object types cleared!");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(0, spawnYPosition, 0);
        Vector3 size = new Vector3(spawnWidth, 0.5f, 0);
        Gizmos.DrawWireCube(center, size);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(-spawnWidth / 2f, spawnYPosition, 0), new Vector3(spawnWidth / 2f, spawnYPosition, 0));
    }
}