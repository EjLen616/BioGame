using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("UI Panels")]
    public SettingsMenuUI settingsMenu;
    public GameObject levelSelectionPanel; // Optional

    void Start()
    {
        // Setup buttons
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    void OnPlayClicked()
    {
        // Play sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

        // Load level selection or first level
        if (LevelManager.Instance != null && LevelManager.Instance.allLevels.Count > 0)
        {
            LevelData firstLevel = LevelManager.Instance.allLevels[0];
            if (firstLevel.isUnlocked)
            {
                LevelManager.Instance.LoadLevel(firstLevel);
            }
        }
        else
        {
            SceneManager.LoadScene("LevelSelection");
        }
    }

    void OnSettingsClicked()
    {
        if (settingsMenu != null)
        {
            settingsMenu.OpenSettings();
        }

        // Play sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();
    }

    void OnQuitClicked()
    {
        // Play sound
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}