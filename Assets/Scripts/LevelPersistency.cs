using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPersistency : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int totalLevels = 30;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string levelPrefix = "Level "; // "Level 1", "Level 2", etc. (with space)

    private static LevelPersistency instance;
    private string currentSceneName = "";

    void Awake()
    {
        // Singleton pattern - keep this persistent across scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Try to detect current level from scene name
        DetectCurrentLevel();
    }

    void OnEnable()
    {
        // Subscribe to scene loaded event to detect level changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Update current scene name
        currentSceneName = scene.name;
        DetectCurrentLevel();

        // Reset time scale when scene loads
        Time.timeScale = 1f;

        Debug.Log($"Scene loaded: {currentSceneName}, Current Level: {currentLevel}");
    }

    void DetectCurrentLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        currentSceneName = sceneName;

        // Check if scene name contains level prefix (with space)
        if (sceneName.StartsWith(levelPrefix))
        {
            string numberPart = sceneName.Substring(levelPrefix.Length);
            if (int.TryParse(numberPart, out int level))
            {
                currentLevel = level;
                Debug.Log($"Detected current level: {currentLevel} from scene: {sceneName}");
            }
        }
    }

    // Go to Main Menu
    public void GoToMainMenu()
    {
        Debug.Log("Going to Main Menu");

        // Play sound if available
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        // Reset time scale if paused
        Time.timeScale = 1f;

        // Resume audio if paused
        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeAllAudio();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Go to Next Level
    public void GoToNextLevel()
    {
        int nextLevel = currentLevel + 1;

        if (nextLevel <= totalLevels)
        {
            Debug.Log($"Going to Level {nextLevel}");
            LoadLevel(nextLevel);
        }
        else
        {
            Debug.Log("No more levels! You completed all levels!");
            // Optional: Go to main menu or victory screen
            GoToMainMenu();
        }
    }

    // Go to Previous Level
    public void GoToPreviousLevel()
    {
        int previousLevel = currentLevel - 1;

        if (previousLevel >= 1)
        {
            Debug.Log($"Going to Level {previousLevel}");
            LoadLevel(previousLevel);
        }
        else
        {
            Debug.Log("Already at Level 1!");
        }
    }

    // Retry Current Level - RELOADS the scene from scratch
    public void RetryLevel()
    {
        Debug.Log($"Retrying Level {currentLevel} - Reloading scene");

        // Play sound if available
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        // Reset time scale
        Time.timeScale = 1f;

        // Resume audio if paused
        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeAllAudio();

        // Build scene name with space
        string sceneName = levelPrefix + currentLevel.ToString();

        // RELOAD the scene - this resets everything
        SceneManager.LoadScene(sceneName);
    }

    // Load specific level by number
    public void LoadLevel(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > totalLevels)
        {
            Debug.LogWarning($"Level {levelNumber} is out of range! (1-{totalLevels})");
            return;
        }

        Debug.Log($"Loading Level {levelNumber}");

        // Play sound if available
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        // Reset time scale if paused
        Time.timeScale = 1f;

        // Resume audio if paused
        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeAllAudio();

        // Update current level
        currentLevel = levelNumber;

        // Build scene name with space
        string sceneName = levelPrefix + levelNumber.ToString();

        SceneManager.LoadScene(sceneName);
    }

    // Load level by scene name directly (handles spaces)
    public void LoadLevelByName(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");

        // Play sound if available
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        // Reset time scale if paused
        Time.timeScale = 1f;

        // Resume audio if paused
        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeAllAudio();

        SceneManager.LoadScene(sceneName);

        // Try to detect level number from scene name (handles spaces)
        if (sceneName.StartsWith(levelPrefix))
        {
            string numberPart = sceneName.Substring(levelPrefix.Length);
            if (int.TryParse(numberPart, out int level))
            {
                currentLevel = level;
                Debug.Log($"Updated current level to: {currentLevel}");
            }
        }
    }

    // Reload current scene (alternative to RetryLevel)
    public void ReloadCurrentScene()
    {
        Debug.Log($"Reloading current scene: {currentSceneName}");

        // Play sound if available
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        // Reset time scale
        Time.timeScale = 1f;

        // Resume audio if paused
        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeAllAudio();

        SceneManager.LoadScene(currentSceneName);
    }

    // Get current level number
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    // Get total levels count
    public int GetTotalLevels()
    {
        return totalLevels;
    }

    // Check if there is a next level
    public bool HasNextLevel()
    {
        return currentLevel < totalLevels;
    }

    // Check if there is a previous level
    public bool HasPreviousLevel()
    {
        return currentLevel > 1;
    }

    // Called when a level is loaded to update current level
    public void UpdateCurrentLevel(int newLevel)
    {
        if (newLevel >= 1 && newLevel <= totalLevels)
        {
            currentLevel = newLevel;
            Debug.Log($"Updated current level to: {currentLevel}");
        }
    }
}