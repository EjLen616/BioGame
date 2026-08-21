using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;

    [Header("Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Settings - On Same Panel")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public GameObject settingsGroup;
    public Button closeSettingsButton;

    private bool isPaused = false;
    private bool settingsVisible = true;

    void Start()
    {
        // Find pause panel if not assigned
        if (pausePanel == null)
        {
            pausePanel = transform.Find("PausePanel")?.gameObject;
            if (pausePanel == null)
                Debug.LogWarning("PausePanel not assigned and could not be found!");
        }

        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Find settings group if not assigned
        if (settingsGroup == null && pausePanel != null)
        {
            settingsGroup = pausePanel.transform.Find("SettingsGroup")?.gameObject;
            if (settingsGroup == null)
                Debug.LogWarning("SettingsGroup not found inside PausePanel!");
        }

        // Settings visible by default
        if (settingsGroup != null)
            settingsGroup.SetActive(true);

        SetupButtons();
        SetupSettingsSliders();
    }

    void SetupButtons()
    {
        // Find buttons if not assigned
        if (resumeButton == null && pausePanel != null)
            resumeButton = pausePanel.transform.Find("ResumeButton")?.GetComponent<Button>();

        if (settingsButton == null && pausePanel != null)
            settingsButton = pausePanel.transform.Find("SettingsButton")?.GetComponent<Button>();

        if (mainMenuButton == null && pausePanel != null)
            mainMenuButton = pausePanel.transform.Find("MainMenuButton")?.GetComponent<Button>();

        if (quitButton == null && pausePanel != null)
            quitButton = pausePanel.transform.Find("QuitButton")?.GetComponent<Button>();

        if (closeSettingsButton == null && pausePanel != null)
            closeSettingsButton = pausePanel.transform.Find("CloseSettingsButton")?.GetComponent<Button>();

        // Add listeners
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ToggleSettings);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(ToggleSettings);
    }

    void SetupSettingsSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            if (SettingsManager.Instance != null)
            {
                musicSlider.value = SettingsManager.Instance.musicVolume;
                Debug.Log($"Music slider set to: {musicSlider.value}");
            }
        }
        else
        {
            Debug.LogWarning("Music Slider is not assigned in PauseMenu!");
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            if (SettingsManager.Instance != null)
            {
                sfxSlider.value = SettingsManager.Instance.sfxVolume;
                Debug.Log($"SFX slider set to: {sfxSlider.value}");
            }
        }
        else
        {
            Debug.LogWarning("SFX Slider is not assigned in PauseMenu!");
        }
    }

    void OnMusicVolumeChanged(float value)
    {
        Debug.Log($"Music slider changed to: {value}");
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetMusicVolume(value);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    void OnSFXVolumeChanged(float value)
    {
        Debug.Log($"SFX slider changed to: {value}");
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetSFXVolume(value);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
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
        Debug.Log("Pausing game");
        isPaused = true;
        Time.timeScale = 0f;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PauseAllAudio();

        if (pausePanel != null)
            pausePanel.SetActive(true);

        // Refresh slider values from SettingsManager when pausing
        if (musicSlider != null && SettingsManager.Instance != null)
        {
            musicSlider.value = SettingsManager.Instance.musicVolume;
        }
        if (sfxSlider != null && SettingsManager.Instance != null)
        {
            sfxSlider.value = SettingsManager.Instance.sfxVolume;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    void ResumeGame()
    {
        Debug.Log("Resuming game");
        isPaused = false;
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeAllAudio();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    void ToggleSettings()
    {
        Debug.Log("Toggling settings");
        settingsVisible = !settingsVisible;

        if (settingsGroup != null)
            settingsGroup.SetActive(settingsVisible);

        if (settingsVisible)
        {
            if (musicSlider != null && SettingsManager.Instance != null)
                musicSlider.value = SettingsManager.Instance.musicVolume;
            if (sfxSlider != null && SettingsManager.Instance != null)
                sfxSlider.value = SettingsManager.Instance.sfxVolume;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    // Public method to check if game is paused
    public bool IsPaused()
    {
        return isPaused;
    }

    void GoToMainMenu()
    {
        Debug.Log("Going to main menu");
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
        Debug.Log("Quitting game");
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}