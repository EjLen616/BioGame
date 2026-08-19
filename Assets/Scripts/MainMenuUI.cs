using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Button closeSettingsButton;

    void Start()
    {
        // Play menu music
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMusic();

        // Setup buttons
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // Setup settings panel
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);

        // Setup sliders
        if (musicSlider != null && SettingsManager.Instance != null)
        {
            musicSlider.value = SettingsManager.Instance.musicVolume;
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxSlider != null && SettingsManager.Instance != null)
        {
            sfxSlider.value = SettingsManager.Instance.sfxVolume;
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    void OnPlayClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        // Load your first level scene
        SceneManager.LoadScene("Level1"); // Change to your first level name
    }

    void OnSettingsClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            // Refresh slider values
            if (musicSlider != null && SettingsManager.Instance != null)
                musicSlider.value = SettingsManager.Instance.musicVolume;
            if (sfxSlider != null && SettingsManager.Instance != null)
                sfxSlider.value = SettingsManager.Instance.sfxVolume;
        }
    }

    void CloseSettings()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void OnMusicVolumeChanged(float value)
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetMusicVolume(value);
    }

    void OnSFXVolumeChanged(float value)
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetSFXVolume(value);
    }

    void OnQuitClicked()
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