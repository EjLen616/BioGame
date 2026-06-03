using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    [Header("Level References")]
    public LevelData currentLevel;
    public GameManager gameManager;
    public ObjectSpawner objectSpawner;

    [Header("Level Completion")]
    public GameObject levelCompletePanel;
    public Button nextLevelButton;
    public Button menuButton;
    public Button restartButton;

    private bool levelCompleted = false;

    void Start()
    {
        // Get current level from LevelManager
        if (LevelManager.Instance != null)
        {
            currentLevel = LevelManager.Instance.GetCurrentLevel();
        }

        if (currentLevel == null)
        {
            Debug.LogError("No current level set!");
            return;
        }

        // Apply level settings
        ApplyLevelSettings();

        // Setup UI
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);

        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
    }

    void ApplyLevelSettings()
    {
        if (objectSpawner != null)
        {
            objectSpawner.initialSpawnRate = currentLevel.spawnRate;
            objectSpawner.maxObjectsOnScreen = currentLevel.maxObjectsOnScreen;
        }
    }

    void Update()
    {
        // Check for level completion (win condition from GameManager)
        if (!levelCompleted && gameManager != null && gameManager.CurrentState == GameManager.GameState.Win)
        {
            CompleteLevel();
        }
    }

    void CompleteLevel()
    {
        levelCompleted = true;

        // Save progress
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CompleteLevel(currentLevel.levelNumber);
        }

        // Show completion UI
        ShowCompletionUI();
    }

    void ShowCompletionUI()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);

            // Enable/disable next level button
            if (nextLevelButton != null)
            {
                LevelData nextLevel = LevelManager.Instance.GetLevelByNumber(currentLevel.levelNumber + 1);
                nextLevelButton.interactable = nextLevel != null && nextLevel.isUnlocked;
            }
        }

        Time.timeScale = 0f; // Pause game
    }

    void OnNextLevelClicked()
    {
        Time.timeScale = 1f;
        LevelData nextLevel = LevelManager.Instance.GetLevelByNumber(currentLevel.levelNumber + 1);

        if (nextLevel != null && nextLevel.isUnlocked)
        {
            LevelManager.Instance.LoadLevel(nextLevel);
        }
        else
        {
            Debug.Log("No next level available or level is locked!");
            // Optional: Load level selection menu
            SceneManager.LoadScene("LevelSelection");
        }
    }

    void OnMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelSelection");
    }

    void OnRestartClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}