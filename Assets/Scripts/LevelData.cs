using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int levelNumber;
    public string levelName;
    public string sceneName; // Name of the scene for this level

    [TextArea(2, 4)]
    public string levelDescription;

    // Level difficulty settings
    public float spawnRate = 1.5f;
    public int maxObjectsOnScreen = 15;

    // Speed settings
    public float baseFallSpeed = 3f;
    public float speedIncreaseInterval = 3f;
    public float speedIncreaseAmount = 0.5f;
    public float maxFallSpeed = 10f;

    // Score settings
    public int pointsForCorrectCatch = 20;
    public int pointsForWrongCatch = -10;
    public int pointsForMissedObject = -5;

    [System.NonSerialized]
    public bool isUnlocked = false;
    [System.NonSerialized]
    public bool isCompleted = false;
}