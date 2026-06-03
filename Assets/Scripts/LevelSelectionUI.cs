using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject levelButtonPrefab;
    public Transform levelButtonsContainer;
    public Text completionText;
    public GameObject levelInfoPanel;
    public Text levelInfoName;
    public Text levelInfoDescription;
    public Button playButton;
    public Button resetProgressButton;

    [Header("Button Sprites")]
    public Sprite lockedButtonSprite;
    public Sprite unlockedButtonSprite;
    public Sprite completedButtonSprite;

    [Header("Level Management")]
    public LevelManager levelManager;
    public LevelData[] levelsToDisplay; // If empty, will use LevelManager's allLevels

    private Dictionary<int, LevelButton> levelButtons = new Dictionary<int, LevelButton>();
    private LevelData selectedLevel;

    class LevelButton
    {
        public GameObject gameObject;
        public Button button;
        public Image background;
        public Text levelNumberText;
        public GameObject lockIcon;
        public GameObject checkmarkIcon;
        public int levelNumber;
    }

    void Start()
    {
        if (levelManager == null)
        {
            levelManager = LevelManager.Instance;
            if (levelManager == null)
            {
                Debug.LogError("LevelManager not found!");
                return;
            }
        }

        if (levelInfoPanel != null)
            levelInfoPanel.SetActive(false);

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        if (resetProgressButton != null)
            resetProgressButton.onClick.AddListener(OnResetProgressClicked);

        RefreshLevelSelection();
    }

    public void RefreshLevelSelection()
    {
        // Clear existing buttons
        foreach (var button in levelButtons.Values)
        {
            Destroy(button.gameObject);
        }
        levelButtons.Clear();

        // Get levels to display
        List<LevelData> levels;
        if (levelsToDisplay != null && levelsToDisplay.Length > 0)
        {
            levels = new List<LevelData>(levelsToDisplay);
        }
        else
        {
            levels = levelManager.allLevels;
        }

        // Sort by level number
        levels.Sort((a, b) => a.levelNumber.CompareTo(b.levelNumber));

        // Create buttons for each level
        foreach (var level in levels)
        {
            CreateLevelButton(level);
        }

        // Update stats
        UpdateStats();
    }

    void CreateLevelButton(LevelData level)
    {
        if (levelButtonPrefab == null || levelButtonsContainer == null)
        {
            Debug.LogError("Missing level button prefab or container!");
            return;
        }

        GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonsContainer);
        LevelButton levelButton = new LevelButton();
        levelButton.gameObject = buttonObj;
        levelButton.button = buttonObj.GetComponent<Button>();
        levelButton.levelNumber = level.levelNumber;

        // Find child components
        Transform backgroundTransform = buttonObj.transform.Find("Background");
        if (backgroundTransform != null)
            levelButton.background = backgroundTransform.GetComponent<Image>();

        Transform numberTransform = buttonObj.transform.Find("LevelNumber");
        if (numberTransform != null)
        {
            levelButton.levelNumberText = numberTransform.GetComponent<Text>();
            if (levelButton.levelNumberText != null)
                levelButton.levelNumberText.text = level.levelNumber.ToString();
        }

        // Find lock icon
        Transform lockTransform = buttonObj.transform.Find("LockIcon");
        if (lockTransform != null)
            levelButton.lockIcon = lockTransform.gameObject;

        // Find checkmark icon
        Transform checkTransform = buttonObj.transform.Find("CheckmarkIcon");
        if (checkTransform != null)
            levelButton.checkmarkIcon = checkTransform.gameObject;

        // Set button state based on unlock/completion status
        if (level.isCompleted)
        {
            // Completed level
            if (levelButton.background != null && completedButtonSprite != null)
                levelButton.background.sprite = completedButtonSprite;

            if (levelButton.lockIcon != null)
                levelButton.lockIcon.SetActive(false);

            if (levelButton.checkmarkIcon != null)
                levelButton.checkmarkIcon.SetActive(true);

            // Add click listener
            int levelNumber = level.levelNumber;
            levelButton.button.onClick.AddListener(() => OnLevelSelected(levelNumber));
        }
        else if (level.isUnlocked)
        {
            // Unlocked but not completed
            if (levelButton.background != null && unlockedButtonSprite != null)
                levelButton.background.sprite = unlockedButtonSprite;

            if (levelButton.lockIcon != null)
                levelButton.lockIcon.SetActive(false);

            if (levelButton.checkmarkIcon != null)
                levelButton.checkmarkIcon.SetActive(false);

            // Add click listener
            int levelNumber = level.levelNumber;
            levelButton.button.onClick.AddListener(() => OnLevelSelected(levelNumber));
        }
        else
        {
            // Locked level
            if (levelButton.background != null && lockedButtonSprite != null)
                levelButton.background.sprite = lockedButtonSprite;

            if (levelButton.lockIcon != null)
                levelButton.lockIcon.SetActive(true);

            if (levelButton.checkmarkIcon != null)
                levelButton.checkmarkIcon.SetActive(false);

            // Make button non-interactable
            levelButton.button.interactable = false;
        }

        levelButtons[level.levelNumber] = levelButton;
    }

    void OnLevelSelected(int levelNumber)
    {
        selectedLevel = levelManager.GetLevelByNumber(levelNumber);

        if (selectedLevel != null && selectedLevel.isUnlocked)
        {
            ShowLevelInfo(selectedLevel);
        }
    }

    void ShowLevelInfo(LevelData level)
    {
        if (levelInfoPanel != null)
        {
            levelInfoPanel.SetActive(true);

            if (levelInfoName != null)
            {
                string completedText = level.isCompleted ? " ✓" : "";
                levelInfoName.text = $"{level.levelNumber}. {level.levelName}{completedText}";
            }

            if (levelInfoDescription != null)
                levelInfoDescription.text = level.levelDescription;

            if (playButton != null)
                playButton.interactable = true;
        }
    }

    void OnPlayButtonClicked()
    {
        if (selectedLevel != null && selectedLevel.isUnlocked)
        {
            levelManager.LoadLevel(selectedLevel);
        }
    }

    void OnResetProgressClicked()
    {
        ShowConfirmationDialog();
    }

    void ShowConfirmationDialog()
    {
        // Simple confirmation dialog
        GameObject dialog = new GameObject("ConfirmationDialog");
        dialog.AddComponent<CanvasRenderer>();

        Image background = dialog.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.9f);

        RectTransform rect = dialog.GetComponent<RectTransform>();
        rect.SetParent(levelButtonsContainer.root);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(dialog.transform);
        Text text = textObj.AddComponent<Text>();
        text.text = "Reset all progress?\nThis cannot be undone!";
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 30;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.3f, 0.4f);
        textRect.anchorMax = new Vector2(0.7f, 0.6f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Add Yes button
        Button yesButton = CreateDialogButton(dialog.transform, "Yes", new Vector2(0.35f, 0.3f), new Vector2(0.45f, 0.35f));
        yesButton.onClick.AddListener(() => {
            levelManager.ResetAllProgress();
            RefreshLevelSelection();
            Destroy(dialog);
        });

        // Add No button
        Button noButton = CreateDialogButton(dialog.transform, "No", new Vector2(0.55f, 0.3f), new Vector2(0.65f, 0.35f));
        noButton.onClick.AddListener(() => Destroy(dialog));

        // Close on background click
        Button backgroundButton = dialog.AddComponent<Button>();
        backgroundButton.onClick.AddListener(() => Destroy(dialog));
    }

    Button CreateDialogButton(Transform parent, string buttonText, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject buttonObj = new GameObject(buttonText + "Button");
        buttonObj.transform.SetParent(parent);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = buttonObj.AddComponent<Image>();
        image.color = Color.gray;

        Button button = buttonObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        Text text = textObj.AddComponent<Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    void UpdateStats()
    {
        if (completionText != null)
            completionText.text = $"Completion: {levelManager.GetCompletionPercentage():F0}%";
    }
}