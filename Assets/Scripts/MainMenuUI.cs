using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject mainMenuPanel;
    public Text titleText;
    public float titleFloatAmount = 5f;
    public float titleFloatSpeed = 1f;

    [Header("Buttons")]
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("Game Mode Panel")]
    public GameObject gameModePanel;
    public Button standardModeButton;
    public Button endlessModeButton;
    public Button timeModeButton;
    public Button backFromModeButton;

    [Header("Level Selection Panel")]
    public GameObject levelSelectionPanel;
    public Button backFromLevelsButton;

    [Header("Level Buttons - Assign ALL 36 Manually")]
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Button level4Button;
    public Button level5Button;
    public Button level6Button;
    public Button level7Button;
    public Button level8Button;
    public Button level9Button;
    public Button level10Button;
    public Button level11Button;
    public Button level12Button;
    public Button level13Button;
    public Button level14Button;
    public Button level15Button;
    public Button level16Button;
    public Button level17Button;
    public Button level18Button;
    public Button level19Button;
    public Button level20Button;
    public Button level21Button;
    public Button level22Button;
    public Button level23Button;
    public Button level24Button;
    public Button level25Button;
    public Button level26Button;
    public Button level27Button;
    public Button level28Button;
    public Button level29Button;
    public Button level30Button;
    public Button level31Button;
    public Button level32Button;
    public Button level33Button;
    public Button level34Button;
    public Button level35Button;
    public Button level36Button;

    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Button closeSettingsButton;

    [Header("Scene Names")]
    public string endlessSceneName = "EndlessMode";
    public string timeSceneName = "TimeMode";
    public string levelPrefix = "Level";

    private Vector3 titleOriginalPosition;
    private bool isTitleAnimating = true;

    void Start()
    {
        // Play menu music
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMusic();

        // Store title original position
        if (titleText != null)
            titleOriginalPosition = titleText.transform.position;

        // Setup main menu
        ShowMainMenu();

        // Setup main menu buttons
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);

        // Setup game mode buttons
        if (standardModeButton != null)
            standardModeButton.onClick.AddListener(OnStandardModeClicked);

        if (endlessModeButton != null)
            endlessModeButton.onClick.AddListener(OnEndlessModeClicked);

        if (timeModeButton != null)
            timeModeButton.onClick.AddListener(OnTimeModeClicked);

        if (backFromModeButton != null)
            backFromModeButton.onClick.AddListener(OnBackFromModeClicked);

        // Setup level selection back button
        if (backFromLevelsButton != null)
            backFromLevelsButton.onClick.AddListener(OnBackFromLevelsClicked);

        // Setup ALL level buttons
        SetupLevelButtons();

        // Setup settings
        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);

        SetupSettingsSliders();
    }

    void Update()
    {
        // Animate title floating
        if (isTitleAnimating && titleText != null)
        {
            float floatOffset = Mathf.Sin(Time.time * titleFloatSpeed) * titleFloatAmount;
            Vector3 pos = titleOriginalPosition;
            pos.y += floatOffset;
            titleText.transform.position = pos;
        }
    }

    void SetupLevelButtons()
    {
        // Level 1
        if (level1Button != null)
            level1Button.onClick.AddListener(() => LoadLevel(1));

        // Level 2
        if (level2Button != null)
            level2Button.onClick.AddListener(() => LoadLevel(2));

        // Level 3
        if (level3Button != null)
            level3Button.onClick.AddListener(() => LoadLevel(3));

        // Level 4
        if (level4Button != null)
            level4Button.onClick.AddListener(() => LoadLevel(4));

        // Level 5
        if (level5Button != null)
            level5Button.onClick.AddListener(() => LoadLevel(5));

        // Level 6
        if (level6Button != null)
            level6Button.onClick.AddListener(() => LoadLevel(6));

        // Level 7
        if (level7Button != null)
            level7Button.onClick.AddListener(() => LoadLevel(7));

        // Level 8
        if (level8Button != null)
            level8Button.onClick.AddListener(() => LoadLevel(8));

        // Level 9
        if (level9Button != null)
            level9Button.onClick.AddListener(() => LoadLevel(9));

        // Level 10
        if (level10Button != null)
            level10Button.onClick.AddListener(() => LoadLevel(10));

        // Level 11
        if (level11Button != null)
            level11Button.onClick.AddListener(() => LoadLevel(11));

        // Level 12
        if (level12Button != null)
            level12Button.onClick.AddListener(() => LoadLevel(12));

        // Level 13
        if (level13Button != null)
            level13Button.onClick.AddListener(() => LoadLevel(13));

        // Level 14
        if (level14Button != null)
            level14Button.onClick.AddListener(() => LoadLevel(14));

        // Level 15
        if (level15Button != null)
            level15Button.onClick.AddListener(() => LoadLevel(15));

        // Level 16
        if (level16Button != null)
            level16Button.onClick.AddListener(() => LoadLevel(16));

        // Level 17
        if (level17Button != null)
            level17Button.onClick.AddListener(() => LoadLevel(17));

        // Level 18
        if (level18Button != null)
            level18Button.onClick.AddListener(() => LoadLevel(18));

        // Level 19
        if (level19Button != null)
            level19Button.onClick.AddListener(() => LoadLevel(19));

        // Level 20
        if (level20Button != null)
            level20Button.onClick.AddListener(() => LoadLevel(20));

        // Level 21
        if (level21Button != null)
            level21Button.onClick.AddListener(() => LoadLevel(21));

        // Level 22
        if (level22Button != null)
            level22Button.onClick.AddListener(() => LoadLevel(22));

        // Level 23
        if (level23Button != null)
            level23Button.onClick.AddListener(() => LoadLevel(23));

        // Level 24
        if (level24Button != null)
            level24Button.onClick.AddListener(() => LoadLevel(24));

        // Level 25
        if (level25Button != null)
            level25Button.onClick.AddListener(() => LoadLevel(25));

        // Level 26
        if (level26Button != null)
            level26Button.onClick.AddListener(() => LoadLevel(26));

        // Level 27
        if (level27Button != null)
            level27Button.onClick.AddListener(() => LoadLevel(27));

        // Level 28
        if (level28Button != null)
            level28Button.onClick.AddListener(() => LoadLevel(28));

        // Level 29
        if (level29Button != null)
            level29Button.onClick.AddListener(() => LoadLevel(29));

        // Level 30
        if (level30Button != null)
            level30Button.onClick.AddListener(() => LoadLevel(30));

        // Level 31
        if (level31Button != null)
            level31Button.onClick.AddListener(() => LoadLevel(31));

        // Level 32
        if (level32Button != null)
            level32Button.onClick.AddListener(() => LoadLevel(32));

        // Level 33
        if (level33Button != null)
            level33Button.onClick.AddListener(() => LoadLevel(33));

        // Level 34
        if (level34Button != null)
            level34Button.onClick.AddListener(() => LoadLevel(34));

        // Level 35
        if (level35Button != null)
            level35Button.onClick.AddListener(() => LoadLevel(35));

        // Level 36
        if (level36Button != null)
            level36Button.onClick.AddListener(() => LoadLevel(36));

        Debug.Log("All level buttons configured!");
    }

    void LoadLevel(int levelNumber)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        string sceneName = levelPrefix + levelNumber.ToString();
        Debug.Log($"Loading level: '{sceneName}'");
        SceneManager.LoadScene(sceneName);
    }

    void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (gameModePanel != null)
            gameModePanel.SetActive(false);

        if (levelSelectionPanel != null)
            levelSelectionPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void SetupSettingsSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            if (SettingsManager.Instance != null)
                musicSlider.value = SettingsManager.Instance.musicVolume;
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            if (SettingsManager.Instance != null)
                sfxSlider.value = SettingsManager.Instance.sfxVolume;
        }
    }

    void OnStartClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (gameModePanel != null)
            gameModePanel.SetActive(true);
    }

    void OnStandardModeClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (gameModePanel != null)
            gameModePanel.SetActive(false);

        if (levelSelectionPanel != null)
            levelSelectionPanel.SetActive(true);
    }

    void OnEndlessModeClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        SceneManager.LoadScene(endlessSceneName);
    }

    void OnTimeModeClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        SceneManager.LoadScene(timeSceneName);
    }

    void OnBackFromModeClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        ShowMainMenu();
    }

    void OnBackFromLevelsClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (levelSelectionPanel != null)
            levelSelectionPanel.SetActive(false);

        if (gameModePanel != null)
            gameModePanel.SetActive(true);
    }

    void OnSettingsClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
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

    void OnExitClicked()
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