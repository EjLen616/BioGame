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
    public float spawnHeight = 2f;

    private float currentSpawnRate;
    private float nextSpawnTime = 0f;
    private int currentObjectsCount = 0;

    void Start()
    {
        currentSpawnRate = initialSpawnRate;
        nextSpawnTime = Time.time + currentSpawnRate;

        // Ensure we have at least 3 object types
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

            // Gradually increase spawn rate (make game harder)
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

        // Calculate total weight for random selection
        float totalWeight = 0f;
        foreach (SpawnSettings settings in spawnableObjects)
        {
            totalWeight += settings.spawnWeight;
        }

        // Randomly select object type based on weight
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

        // Calculate spawn position
        float spawnX = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        Vector3 spawnPosition = transform.position + new Vector3(spawnX, Random.Range(0, spawnHeight), 0);

        // Instantiate object
        GameObject newObject = Instantiate(fallingObjectPrefab, spawnPosition, Quaternion.identity);

        // Set object type
        FallingObject fallingObject = newObject.GetComponent<FallingObject>();
        if (fallingObject != null)
        {
            fallingObject.SetObjectType(selectedType);
        }

        currentObjectsCount++;
    }

    public void ObjectDestroyed()
    {
        currentObjectsCount = Mathf.Max(0, currentObjectsCount - 1);
    }

    void OnDrawGizmos()
    {
        // Draw spawn area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + new Vector3(0, spawnHeight / 2f, 0),
                           new Vector3(spawnWidth, spawnHeight, 0));
    }

    // Public method to add new object types at runtime
    public void AddObjectType(ObjectType newType, float weight = 1f)
    {
        SpawnSettings newSettings = new SpawnSettings
        {
            objectType = newType,
            spawnWeight = weight
        };

        spawnableObjects.Add(newSettings);
    }
}