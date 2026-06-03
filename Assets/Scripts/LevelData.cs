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

    [System.NonSerialized]
    public bool isUnlocked = false;
    [System.NonSerialized]
    public bool isCompleted = false;
}