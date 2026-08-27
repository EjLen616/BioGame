using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPersistency : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int totalLevels = 30;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string levelPrefix = "Level";

    private string currentSceneName = "";

    void Start()
    {
        DetectCurrentLevel();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        DetectCurrentLevel();
        Time.timeScale = 1f;
        Debug.Log($"Scene loaded: {currentSceneName}, Current Level: {currentLevel}");
    }

    void DetectCurrentLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        currentSceneName = sceneName;

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

    public void GoToMainMenu()
    {
        Debug.Log("Going to Main Menu");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeAllAudio();

        SceneManager.LoadScene(mainMenuSceneName);
    }

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
            GoToMainMenu();
        }
    }

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

    public void RetryLevel()
    {
        Debug.Log($"Retrying Level {currentLevel} - Reloading scene");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeAllAudio();

        string sceneName = levelPrefix + currentLevel.ToString();
        SceneManager.LoadScene(sceneName);
    }

    public void LoadLevel(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > totalLevels)
        {
            Debug.LogWarning($"Level {levelNumber} is out of range! (1-{totalLevels})");
            return;
        }

        Debug.Log($"Loading Level {levelNumber}");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeAllAudio();

        currentLevel = levelNumber;
        string sceneName = levelPrefix + levelNumber.ToString();
        SceneManager.LoadScene(sceneName);
    }

    public void LoadLevelByName(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeAllAudio();

        SceneManager.LoadScene(sceneName);

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

    public void ReloadCurrentScene()
    {
        Debug.Log($"Reloading current scene: {currentSceneName}");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeAllAudio();

        SceneManager.LoadScene(currentSceneName);
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public int GetTotalLevels()
    {
        return totalLevels;
    }

    public bool HasNextLevel()
    {
        return currentLevel < totalLevels;
    }

    public bool HasPreviousLevel()
    {
        return currentLevel > 1;
    }

    public void UpdateCurrentLevel(int newLevel)
    {
        if (newLevel >= 1 && newLevel <= totalLevels)
        {
            currentLevel = newLevel;
            Debug.Log($"Updated current level to: {currentLevel}");
        }
    }
}