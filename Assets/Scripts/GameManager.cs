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

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    void WinGame()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Win;
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
        Debug.Log("You Win!");
    }

    void LoseGame()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Lose;
        if (losePanel != null) losePanel.SetActive(true);
        Time.timeScale = 0f;
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