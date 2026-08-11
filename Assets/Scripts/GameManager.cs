using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Game state
    public enum GameState { Playing, Win, Lose }
    public GameState CurrentState { get; private set; }

    // UI references
    public GameObject winPanel;
    public GameObject losePanel;
    public Text scoreText;

    // Score tracking
    private int score = 0;

    // Cache baskets for performance
    private Basket[] baskets;

    [Header("Score Settings")]
    public int pointsForCorrectCatch = 20;
    public int pointsForWrongCatch = -10;
    public int pointsForMissedObject = -5;

    [Header("Win/Lose Conditions")]
    public int scoreLoseThreshold = -100; // Score at which you lose
    public int basketsNeededToWin = 2; // How many baskets need green checkmarks to win

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
        UpdateUI();

        // Force game music to play
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameMusic();
        }
    }

    void FindAllBaskets()
    {
        baskets = FindObjectsByType<Basket>(FindObjectsSortMode.None);
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

        // Check win conditions
        CheckWinCondition();

        // Check lose conditions
        CheckLoseCondition();

        UpdateUI();
    }

    void CheckWinCondition()
    {
        if (CurrentState != GameState.Playing) return;

        if (baskets == null || baskets.Length == 0)
        {
            FindAllBaskets();
            if (baskets == null || baskets.Length == 0) return;
        }

        // Count how many baskets are at max health (green checkmark)
        int healthyBaskets = 0;
        foreach (Basket basket in baskets)
        {
            if (basket != null && basket.CurrentHealthStage >= 3)
            {
                healthyBaskets++;
            }
        }

        // Win if we have enough healthy baskets
        if (healthyBaskets >= basketsNeededToWin)
        {
            WinGame();
        }
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
        foreach (Basket basket in baskets)
        {
            if (basket != null && basket.CurrentHealthStage <= -3)
            {
                worstBaskets++;
            }
        }

        // Check if any basket is at worst health AND another is at max health
        // OR if we have two worst baskets
        bool hasHealthy = false;
        bool hasWorst = false;

        foreach (Basket basket in baskets)
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

        // Lose condition 3: Score drops to -100 or below
        if (score <= scoreLoseThreshold)
        {
            LoseGame();
            return;
        }
    }

    public void AddScore(int points)
    {
        score += points;

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

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    void WinGame()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Win;

        // Play win sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayWinSound();

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        Debug.Log("You Win! Two body parts are healthy!");
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
        foreach (Basket basket in baskets)
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