using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Settings")]
    public List<LevelData> allLevels = new List<LevelData>();

    [Header("Save Settings")]
    public string saveFileName = "levelprogress.json";

    private LevelProgress saveData;
    private LevelData currentLevel;

    [System.Serializable]
    public class LevelProgress
    {
        public List<int> unlockedLevels = new List<int>();
        public List<int> completedLevels = new List<int>();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
            InitializeLevelUnlocks();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeLevelUnlocks()
    {
        // Unlock first level by default
        if (allLevels.Count > 0)
        {
            UnlockLevel(allLevels[0].levelNumber);
        }

        // Apply saved progress
        foreach (var level in allLevels)
        {
            level.isUnlocked = saveData.unlockedLevels.Contains(level.levelNumber);
            level.isCompleted = saveData.completedLevels.Contains(level.levelNumber);
        }
    }

    public void CompleteLevel(int levelNumber)
    {
        LevelData level = GetLevelByNumber(levelNumber);
        if (level == null) return;

        // Mark current level as completed
        level.isCompleted = true;
        if (!saveData.completedLevels.Contains(levelNumber))
        {
            saveData.completedLevels.Add(levelNumber);
        }

        // Unlock next level
        LevelData nextLevel = GetLevelByNumber(levelNumber + 1);
        if (nextLevel != null)
        {
            UnlockLevel(nextLevel.levelNumber);
        }

        SaveProgress();
        Debug.Log($"Level {levelNumber} completed! Next level unlocked.");
    }

    public void UnlockLevel(int levelNumber)
    {
        LevelData level = GetLevelByNumber(levelNumber);
        if (level != null && !level.isUnlocked)
        {
            level.isUnlocked = true;
            if (!saveData.unlockedLevels.Contains(levelNumber))
            {
                saveData.unlockedLevels.Add(levelNumber);
            }
            SaveProgress();
            Debug.Log($"Level {levelNumber} unlocked!");
        }
    }

    public bool IsLevelUnlocked(int levelNumber)
    {
        LevelData level = GetLevelByNumber(levelNumber);
        return level != null && level.isUnlocked;
    }

    public bool IsLevelCompleted(int levelNumber)
    {
        LevelData level = GetLevelByNumber(levelNumber);
        return level != null && level.isCompleted;
    }

    public LevelData GetLevelByNumber(int levelNumber)
    {
        return allLevels.Find(l => l.levelNumber == levelNumber);
    }

    public void SetCurrentLevel(LevelData level)
    {
        currentLevel = level;
    }

    public LevelData GetCurrentLevel()
    {
        return currentLevel;
    }

    public void LoadLevel(LevelData level)
    {
        if (level == null)
        {
            Debug.LogError("Cannot load null level!");
            return;
        }

        if (!level.isUnlocked)
        {
            Debug.LogWarning($"Level {level.levelNumber} is locked!");
            return;
        }

        currentLevel = level;

        // Load the scene
        if (!string.IsNullOrEmpty(level.sceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(level.sceneName);

            // Notify AudioManager about level load - using public method
            if (AudioManager.Instance != null)
            {
                // Use invoke to let scene load first
                AudioManager.Instance.Invoke(nameof(AudioManager.DetectSceneTypeAndPlayMusic), 0.2f);
            }
        }
        else
        {
            Debug.LogError($"Level {level.levelNumber} has no scene assigned!");
        }
    }

    public void LoadLevelByNumber(int levelNumber)
    {
        LevelData level = GetLevelByNumber(levelNumber);
        if (level != null && level.isUnlocked)
        {
            LoadLevel(level);
        }
    }

    void SaveProgress()
    {
        string json = JsonUtility.ToJson(saveData);
        string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
        File.WriteAllText(filePath, json);
    }

    void LoadProgress()
    {
        string filePath = Path.Combine(Application.persistentDataPath, saveFileName);

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            saveData = JsonUtility.FromJson<LevelProgress>(json);

            if (saveData == null)
            {
                saveData = new LevelProgress();
            }
        }
        else
        {
            saveData = new LevelProgress();
            Debug.Log("No save file found, creating new progress.");
        }
    }

    public void ResetAllProgress()
    {
        saveData = new LevelProgress();
        foreach (var level in allLevels)
        {
            level.isUnlocked = false;
            level.isCompleted = false;
        }

        InitializeLevelUnlocks();
        SaveProgress();
        Debug.Log("All progress reset!");
    }

    public List<LevelData> GetAllUnlockedLevels()
    {
        return allLevels.Where(l => l.isUnlocked).ToList();
    }

    public float GetCompletionPercentage()
    {
        if (allLevels.Count == 0) return 0;

        int completedCount = saveData.completedLevels.Count;
        return (float)completedCount / allLevels.Count * 100f;
    }
}