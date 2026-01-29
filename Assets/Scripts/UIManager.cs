using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Text scoreText;
    public Text timerText;
    public GameObject pauseMenu;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("Game References")]
    public GameManager gameManager;

    private float gameTime = 0f;
    private bool isPaused = false;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        UpdateScore(0);
        ShowGameUI();
    }

    void Update()
    {
        if (gameManager != null && gameManager.CurrentState == GameManager.GameState.Playing)
        {
            gameTime += Time.deltaTime;
            UpdateTimer();
        }

        // Pause toggle with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void UpdateTimer()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60);
            int seconds = Mathf.FloorToInt(gameTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void ShowWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    public void ShowGameUI()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (pauseMenu != null) pauseMenu.SetActive(false);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(isPaused);
        }

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }

    public void QuitGame()
    {
        if (gameManager != null)
        {
            gameManager.QuitGame();
        }
    }
}