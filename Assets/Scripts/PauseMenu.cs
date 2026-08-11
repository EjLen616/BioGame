using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public SettingsMenuUI settingsMenu;

    [Header("Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button mainMenuButton;
    public Button quitButton;

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel == null)
        {
            pausePanel = transform.Find("PausePanel")?.gameObject;
            if (pausePanel == null)
            {
                Debug.LogWarning("PausePanel not assigned and could not be found!");
            }
        }

        if (pausePanel != null)
            pausePanel.SetActive(false);

        SetupButtons();
    }

    void SetupButtons()
    {
        if (resumeButton == null && pausePanel != null)
        {
            resumeButton = pausePanel.transform.Find("ResumeButton")?.GetComponent<Button>();
        }

        if (settingsButton == null && pausePanel != null)
        {
            settingsButton = pausePanel.transform.Find("SettingsButton")?.GetComponent<Button>();
        }

        if (mainMenuButton == null && pausePanel != null)
        {
            mainMenuButton = pausePanel.transform.Find("MainMenuButton")?.GetComponent<Button>();
        }

        if (quitButton == null && pausePanel != null)
        {
            quitButton = pausePanel.transform.Find("QuitButton")?.GetComponent<Button>();
        }

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseAllAudio();
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
        }
    }

    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeAllAudio();
        }

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsMenu != null && settingsMenu.settingsPanel != null)
            settingsMenu.CloseSettings();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    void OpenSettings()
    {
        if (settingsMenu != null)
        {
            settingsMenu.OpenSettings();
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeAllAudio();
            AudioManager.Instance.PlayButtonClick();
        }

        SceneManager.LoadScene("MainMenu");
    }

    void QuitGame()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}