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
    public Text timerText;
    public Image clockIcon;

    // NEW: Button references for lose panel
    [Header("Lose Panel Buttons")]
    public Button retryButton;
    public Button menuButton;

    // Score tracking
    private int score = 0;
    private int bestScore = 0;
    private string bestScoreKey = "TimeBestScore";

    // Timer variables
    private float currentTime = 10f;
    private float maxTime = 10f;
    private bool isTimerRunning = false;

    // Clock pulse variables
    private float pulseTimer = 0f;
    private float pulseSpeed = 0f;
    private bool isPulsing = false;
    private Vector3 originalClockScale;

    // Audio tick variables
    private float tickTimer = 0f;
    private float tickInterval = 0f;
    private bool isTicking = false;

    // Cache baskets for performance
    private TimeBasket[] baskets;

    [Header("Score Settings")]
    public int pointsForCorrectCatch = 20;
    public int pointsForWrongCatch = -10;
    public int pointsForMissedObject = -5;

    [Header("Timer Settings")]
    public float startingTime = 10f;
    public float timeBonusForCorrectCatch = 1f;

    [Header("Clock Settings")]
    public AudioClip tickSoundSlow;
    public AudioClip tickSoundFast;
    public float slowTickInterval = 0.5f;
    public float fastTickInterval = 0.25f;
    public float pulseSlowSpeed = 1f;
    public float pulseFastSpeed = 3f;
    public float pulseAmount = 0.15f;

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

        // Store original clock scale
        if (clockIcon != null)
        {
            originalClockScale = clockIcon.transform.localScale;
        }

        // Setup lose panel buttons
        SetupLosePanelButtons();

        UpdateUI();
        UpdateTimerUI();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameMusic();
        }

        Debug.Log($"Time Mode started! Timer: {currentTime}s");
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
            Debug.LogWarning("Retry button not assigned in TimeGameManager!");
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(GoToMainMenu);
        }
        else
        {
            Debug.LogWarning("Menu button not assigned in TimeGameManager!");
        }
    }

    void Update()
    {
        if (CurrentState != GameState.Playing) return;
        if (!isTimerRunning) return;

        // Count down timer
        currentTime -= Time.deltaTime;

        // Update timer display
        UpdateTimerUI();

        // Update clock ticking and pulsing
        UpdateClockEffects();

        // Check if timer ran out
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            StopClockEffects();
            LoseGame("Time ran out!");
        }
    }

    void UpdateClockEffects()
    {
        // Check time thresholds
        if (currentTime <= 3f)
        {
            // FAST TICKING (less than 3 seconds)
            if (!isTicking || tickInterval != fastTickInterval)
            {
                StartTicking(fastTickInterval);
            }

            // FAST PULSING
            if (!isPulsing || pulseSpeed != pulseFastSpeed)
            {
                StartPulsing(pulseFastSpeed);
            }

            // Update tick timer
            UpdateTickTimer();

            // Update pulse
            UpdatePulse();
        }
        else if (currentTime <= 5f)
        {
            // SLOW TICKING (5 seconds or less)
            if (!isTicking || tickInterval != slowTickInterval)
            {
                StartTicking(slowTickInterval);
            }

            // SLOW PULSING
            if (!isPulsing || pulseSpeed != pulseSlowSpeed)
            {
                StartPulsing(pulseSlowSpeed);
            }

            // Update tick timer
            UpdateTickTimer();

            // Update pulse
            UpdatePulse();
        }
        else
        {
            // NO TICKING (more than 5 seconds)
            if (isTicking)
            {
                StopTicking();
            }

            // NO PULSING
            if (isPulsing)
            {
                StopPulsing();
            }
        }
    }

    void StartTicking(float interval)
    {
        isTicking = true;
        tickInterval = interval;
        tickTimer = 0f;
        Debug.Log($"Clock ticking started at interval: {interval}s");
    }

    void StopTicking()
    {
        isTicking = false;
        tickTimer = 0f;
        Debug.Log("Clock ticking stopped");
    }

    void UpdateTickTimer()
    {
        if (!isTicking) return;

        tickTimer += Time.deltaTime;

        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            PlayTickSound();
        }
    }

    void PlayTickSound()
    {
        if (AudioManager.Instance == null) return;

        if (currentTime <= 3f && tickSoundFast != null)
        {
            AudioManager.Instance.PlayCustomSFX(tickSoundFast);
        }
        else if (tickSoundSlow != null)
        {
            AudioManager.Instance.PlayCustomSFX(tickSoundSlow);
        }
    }

    void StartPulsing(float speed)
    {
        isPulsing = true;
        pulseSpeed = speed;
        pulseTimer = 0f;
        Debug.Log($"Clock pulsing started at speed: {speed}");
    }

    void StopPulsing()
    {
        isPulsing = false;
        pulseTimer = 0f;

        if (clockIcon != null)
        {
            clockIcon.transform.localScale = originalClockScale;
        }
    }

    void UpdatePulse()
    {
        if (!isPulsing || clockIcon == null) return;

        pulseTimer += Time.deltaTime * pulseSpeed;

        float pulseFactor = 1f + Mathf.Sin(pulseTimer * Mathf.PI * 2f) * pulseAmount;
        clockIcon.transform.localScale = originalClockScale * pulseFactor;
    }

    void StopClockEffects()
    {
        StopTicking();
        StopPulsing();
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
            StopClockEffects();
            LoseGame("Two body parts at worst health!");
            return;
        }

        if (hasHealthy && hasWorst)
        {
            StopClockEffects();
            LoseGame("One healthy and one worst body part!");
            return;
        }

        if (score <= scoreLoseThreshold)
        {
            StopClockEffects();
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
            StopClockEffects();
            LoseGame($"Score reached {score} (below {scoreLoseThreshold})!");
        }

        UpdateUI();
    }

    public void AddTimeBonus(float bonusTime)
    {
        if (CurrentState != GameState.Playing) return;

        currentTime += bonusTime;

        if (currentTime > 5f && isTicking)
        {
            StopTicking();
            StopPulsing();
        }

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
            timerText.text = currentTime.ToString("F1") + "s";

            if (currentTime <= 3f)
            {
                timerText.color = Color.red;
            }
            else if (currentTime <= 5f)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.black;
            }
        }
    }

    void LoseGame(string reason)
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Lose;
        isTimerRunning = false;
        StopClockEffects();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayLoseSound();

        if (losePanel != null)
        {
            losePanel.SetActive(true);
            Time.timeScale = 0f;
        }

        Debug.Log($"Game Over! {reason}");
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