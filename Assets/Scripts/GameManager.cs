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
    public int pointsForMissedObject = -5; // Points lost when object falls off screen

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

        // Check win/lose conditions
        bool allWorst = true;
        bool allBest = true;

        foreach (Basket basket in baskets)
        {
            if (basket != null)
            {
                if (basket.CurrentHealthStage > -3) allWorst = false;
                if (basket.CurrentHealthStage < 3) allBest = false;
            }
        }

        if (allWorst)
        {
            LoseGame();
        }
        else if (allBest)
        {
            WinGame();
        }

        UpdateUI();
    }

    public void AddScore(int points)
    {
        score += points;
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

        Debug.Log("You Win!");
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

        Debug.Log("Game Over!");
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