using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndlessGameManager : MonoBehaviour
{
    public static EndlessGameManager Instance { get; private set; }

    // Game state
    public enum GameState { Playing, Lose }
    public GameState CurrentState { get; private set; }

    // UI references
    public GameObject losePanel;
    public Text scoreText;
    public Text bestScoreText;

    // Score tracking
    private int score = 0;
    private int bestScore = 0;
    private string bestScoreKey = "EndlessBestScore";

    // Cache baskets for performance
    private EndlessBasket[] baskets;

    [Header("Score Settings")]
    public int pointsForCorrectCatch = 20;
    public int pointsForWrongCatch = -10;
    public int pointsForMissedObject = -5;

    [Header("Lose Conditions")]
    public int scoreLoseThreshold = -100;

    // NEW: Button references for lose panel
    [Header("Lose Panel Buttons")]
    public Button retryButton;
    public Button menuButton;

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
        CurrentState = GameState.Playing;
        FindAllBaskets();
        LoadBestScore();
        UpdateUI();

        // Setup lose panel buttons
        SetupLosePanelButtons();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameMusic();
        }
    }

    void SetupLosePanelButtons()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogWarning("Retry button not assigned in EndlessGameManager!");
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(GoToMainMenu);
        }
        else
        {
            Debug.LogWarning("Menu button not assigned in EndlessGameManager!");
        }
    }

    void FindAllBaskets()
    {
        baskets = FindObjectsByType<EndlessBasket>(FindObjectsSortMode.None);
        if (baskets.Length == 0)
        {
            Debug.LogWarning("No baskets found in the scene!");
        }
    }

    public void UpdateBasketHealth(int basketIndex, int healthStage)
    {
        if (baskets == null || baskets.Length == 0)
        {
            FindAllBaskets();
        }

        if (baskets.Length == 0) return;

        CheckLoseCondition();
        UpdateUI();
    }

    void CheckLoseCondition()
    {
        if (CurrentState != GameState.Playing) return;

        if (baskets == null || baskets.Length == 0)
        {
            FindAllBaskets();
            if (baskets == null || baskets.Length == 0) return;
        }

        int worstBaskets = 0;
        foreach (EndlessBasket basket in baskets)
        {
            if (basket != null && basket.CurrentHealthStage <= -3)
            {
                worstBaskets++;
            }
        }

        bool hasHealthy = false;
        bool hasWorst = false;

        foreach (EndlessBasket basket in baskets)
        {
            if (basket != null)
            {
                if (basket.CurrentHealthStage >= 3)
                    hasHealthy = true;
                if (basket.CurrentHealthStage <= -3)
                    hasWorst = true;
            }
        }

        if (worstBaskets >= 2)
        {
            LoseGame();
            return;
        }

        if (hasHealthy && hasWorst)
        {
            LoseGame();
            return;
        }

        if (score <= scoreLoseThreshold)
        {
            LoseGame();
            return;
        }
    }

    public void AddScore(int points)
    {
        score += points;

        if (score > bestScore)
        {
            bestScore = score;
            SaveBestScore();
        }

        if (score <= scoreLoseThreshold && CurrentState == GameState.Playing)
        {
            LoseGame();
        }

        UpdateUI();
    }

    public void ObjectMissed()
    {
        AddScore(pointsForMissedObject);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPointLoseSound();

        Debug.Log($"Object missed! Score: {pointsForMissedObject} points");
    }

    void LoadBestScore()
    {
        bestScore = PlayerPrefs.GetInt(bestScoreKey, 0);
        Debug.Log($"Loaded best score: {bestScore}");
    }

    void SaveBestScore()
    {
        PlayerPrefs.SetInt(bestScoreKey, bestScore);
        PlayerPrefs.Save();
        Debug.Log($"Saved best score: {bestScore}");
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (bestScoreText != null)
            bestScoreText.text = "Best: " + bestScore;
    }

    void LoseGame()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Lose;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayLoseSound();

        if (losePanel != null)
        {
            losePanel.SetActive(true);
            Time.timeScale = 0f;
        }

        Debug.Log("Game Over!");
    }

    // NEW: Retry method
    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // NEW: Go to Main Menu method
    public void GoToMainMenu()
    {
        Debug.Log("Going to Main Menu...");
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}