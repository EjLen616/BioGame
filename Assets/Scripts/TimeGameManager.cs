using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimeGameManager : MonoBehaviour
{
    public static TimeGameManager Instance { get; private set; }

    // Game state
    public enum GameState { Playing, Lose }
    public GameState CurrentState { get; private set; }

    // UI references
    public GameObject losePanel;
    public Text scoreText;
    public Text bestScoreText;
    public Text timerText; // Timer display

    // Score tracking
    private int score = 0;
    private int bestScore = 0;
    private string bestScoreKey = "TimeBestScore";

    // Timer variables
    private float currentTime = 10f;
    private float maxTime = 10f;
    private bool isTimerRunning = false;

    // Cache baskets for performance
    private TimeBasket[] baskets;

    [Header("Score Settings")]
    public int pointsForCorrectCatch = 20;
    public int pointsForWrongCatch = -10;
    public int pointsForMissedObject = -5;

    [Header("Timer Settings")]
    public float startingTime = 10f;
    public float timeBonusForCorrectCatch = 1f;

    [Header("Lose Conditions")]
    public int scoreLoseThreshold = -100;

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

        // Initialize timer
        currentTime = startingTime;
        maxTime = startingTime;
        isTimerRunning = true;

        UpdateUI();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameMusic();
        }

        Debug.Log($"Time Mode started! Timer: {currentTime}s");
    }

    void Update()
    {
        if (CurrentState != GameState.Playing) return;
        if (!isTimerRunning) return;

        // Count down timer
        currentTime -= Time.deltaTime;

        // Update timer display
        UpdateTimerUI();

        // Check if timer ran out
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            LoseGame("Time ran out!");
        }
    }

    void FindAllBaskets()
    {
        baskets = FindObjectsByType<TimeBasket>(FindObjectsSortMode.None);
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
        foreach (TimeBasket basket in baskets)
        {
            if (basket != null && basket.CurrentHealthStage <= -3)
            {
                worstBaskets++;
            }
        }

        bool hasHealthy = false;
        bool hasWorst = false;

        foreach (TimeBasket basket in baskets)
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
            LoseGame("Two body parts at worst health!");
            return;
        }

        if (hasHealthy && hasWorst)
        {
            LoseGame("One healthy and one worst body part!");
            return;
        }

        if (score <= scoreLoseThreshold)
        {
            LoseGame($"Score reached {score} (below {scoreLoseThreshold})!");
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
            LoseGame($"Score reached {score} (below {scoreLoseThreshold})!");
        }

        UpdateUI();
    }

    public void AddTimeBonus(float bonusTime)
    {
        if (CurrentState != GameState.Playing) return;

        currentTime += bonusTime;

        UpdateTimerUI();
        Debug.Log($"Time bonus: +{bonusTime}s! Time remaining: {currentTime:F1}s");
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

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // Display timer with 1 decimal place
            timerText.text = currentTime.ToString("F1") + "s";

            // Change color based on time remaining
            if (currentTime <= 3f)
            {
                timerText.color = Color.red; // Urgent - red
            }
            else if (currentTime <= 5f)
            {
                timerText.color = Color.yellow; // Warning - yellow
            }
            else
            {
                timerText.color = Color.black; // Safe - BLACK (changed from white)
            }
        }
    }

    void LoseGame(string reason)
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Lose;
        isTimerRunning = false;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayLoseSound();

        if (losePanel != null)
        {
            losePanel.SetActive(true);
            Time.timeScale = 0f;
        }

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