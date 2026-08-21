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
    public Text bestScoreText; // NEW: Best score display

    // Score tracking
    private int score = 0;
    private int bestScore = 0;
    private string bestScoreKey = "EndlessBestScore"; // Save key

    // Cache baskets for performance
    private EndlessBasket[] baskets;

    [Header("Score Settings")]
    public int pointsForCorrectCatch = 20;
    public int pointsForWrongCatch = -10;
    public int pointsForMissedObject = -5;

    [Header("Lose Conditions")]
    public int scoreLoseThreshold = -100; // Score at which you lose

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

        // Force game music to play
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameMusic();
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
        // Refresh basket cache if needed
        if (baskets == null || baskets.Length == 0)
        {
            FindAllBaskets();
        }

        if (baskets.Length == 0) return;

        // Check lose conditions
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

        // Count how many baskets are at worst health (red X)
        int worstBaskets = 0;
        foreach (EndlessBasket basket in baskets)
        {
            if (basket != null && basket.CurrentHealthStage <= -3)
            {
                worstBaskets++;
            }
        }

        // Check if any basket is at worst health AND another is at max health
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

        // Lose condition 1: Two worst baskets (red X)
        if (worstBaskets >= 2)
        {
            LoseGame();
            return;
        }

        // Lose condition 2: One healthy (green) AND one worst (red X)
        if (hasHealthy && hasWorst)
        {
            LoseGame();
            return;
        }

        // Lose condition 3: Score drops to threshold or below
        if (score <= scoreLoseThreshold)
        {
            LoseGame();
            return;
        }
    }

    public void AddScore(int points)
    {
        score += points;

        // Update best score if current score is higher
        if (score > bestScore)
        {
            bestScore = score;
            SaveBestScore();
        }

        // Check lose condition immediately after score change
        if (score <= scoreLoseThreshold && CurrentState == GameState.Playing)
        {
            LoseGame();
        }

        UpdateUI();
    }

    public void ObjectMissed()
    {
        // Apply penalty for missed object
        AddScore(pointsForMissedObject);

        // Play point lose sound
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

        // Play lose sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayLoseSound();

        if (losePanel != null)
        {
            losePanel.SetActive(true);
            Time.timeScale = 0f;
        }

        // Determine lose reason for debugging
        string reason = "";

        // Count baskets
        int worstBaskets = 0;
        int healthyBaskets = 0;
        foreach (EndlessBasket basket in baskets)
        {
            if (basket != null)
            {
                if (basket.CurrentHealthStage >= 3)
                    healthyBaskets++;
                if (basket.CurrentHealthStage <= -3)
                    worstBaskets++;
            }
        }

        if (score <= scoreLoseThreshold)
            reason = $"Score reached {score} (below {scoreLoseThreshold})";
        else if (worstBaskets >= 2)
            reason = "Two body parts at worst health";
        else if (healthyBaskets >= 1 && worstBaskets >= 1)
            reason = "One healthy and one worst body part";

        Debug.Log($"Game Over! {reason}");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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