using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Text musicVolumeText;
    public Text sfxVolumeText;
    public Button closeButton;
    public Button defaultButton;
    public GameObject settingsPanel;

    [Header("Settings")]
    public bool startClosed = true;

    void Start()
    {
        if (startClosed && settingsPanel != null)
            settingsPanel.SetActive(false);

        // Setup sliders
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            musicSlider.value = SettingsManager.Instance != null ? SettingsManager.Instance.musicVolume : 0.7f;
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            sfxSlider.value = SettingsManager.Instance != null ? SettingsManager.Instance.sfxVolume : 0.7f;
        }

        // Setup buttons
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseSettings);

        if (defaultButton != null)
            defaultButton.onClick.AddListener(ResetToDefault);

        // Update volume displays
        UpdateVolumeDisplay();
    }

    void OnMusicVolumeChanged(float value)
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetMusicVolume(value);

        UpdateVolumeDisplay();

        // Play click sound for feedback
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    void OnSFXVolumeChanged(float value)
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.SetSFXVolume(value);

        UpdateVolumeDisplay();

        // Play click sound for feedback
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    void UpdateVolumeDisplay()
    {
        if (musicVolumeText != null && SettingsManager.Instance != null)
            musicVolumeText.text = Mathf.RoundToInt(SettingsManager.Instance.musicVolume * 100) + "%";

        if (sfxVolumeText != null && SettingsManager.Instance != null)
            sfxVolumeText.text = Mathf.RoundToInt(SettingsManager.Instance.sfxVolume * 100) + "%";
    }

    void ResetToDefault()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.ResetToDefaults();

        if (musicSlider != null && SettingsManager.Instance != null)
            musicSlider.value = SettingsManager.Instance.musicVolume;

        if (sfxSlider != null && SettingsManager.Instance != null)
            sfxSlider.value = SettingsManager.Instance.sfxVolume;

        UpdateVolumeDisplay();

        // Play click sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        // Refresh values
        if (musicSlider != null && SettingsManager.Instance != null)
            musicSlider.value = SettingsManager.Instance.musicVolume;

        if (sfxSlider != null && SettingsManager.Instance != null)
            sfxSlider.value = SettingsManager.Instance.sfxVolume;

        UpdateVolumeDisplay();

        // Play click sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Play click sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    void Update()
    {
        // Close settings with Escape key
        if (settingsPanel != null && settingsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseSettings();
        }
    }
}