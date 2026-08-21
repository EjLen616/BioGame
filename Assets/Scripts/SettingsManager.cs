using UnityEngine;
using System.IO;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.7f;

    private string saveFilePath;

    [System.Serializable]
    public class SettingsData
    {
        public float musicVolume;
        public float sfxVolume;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "settings.json");
            LoadSettings();
            Debug.Log($"SettingsManager Awake - Music: {musicVolume}, SFX: {sfxVolume}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Apply settings after everything is loaded
        ApplySettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplySettings();
        SaveSettings();
        Debug.Log($"Music volume set to: {musicVolume}");
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplySettings();
        SaveSettings();
        Debug.Log($"SFX volume set to: {sfxVolume}");
    }

    void ApplySettings()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(musicVolume);
            AudioManager.Instance.SetSFXVolume(sfxVolume);
            Debug.Log($"Applied settings - Music: {musicVolume}, SFX: {sfxVolume}");
        }
        else
        {
            Debug.LogWarning("AudioManager not found, settings will apply when it's created");
        }
    }

    void SaveSettings()
    {
        try
        {
            SettingsData data = new SettingsData
            {
                musicVolume = musicVolume,
                sfxVolume = sfxVolume
            };

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"Settings saved to: {saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save settings: {e.Message}");
        }
    }

    void LoadSettings()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                SettingsData data = JsonUtility.FromJson<SettingsData>(json);

                if (data != null)
                {
                    musicVolume = data.musicVolume;
                    sfxVolume = data.sfxVolume;
                    Debug.Log($"Settings loaded - Music: {musicVolume}, SFX: {sfxVolume}");
                }
            }
            else
            {
                Debug.Log("No settings file found, using defaults.");
                // Save defaults immediately
                SaveSettings();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load settings: {e.Message}");
        }
    }

    public void ResetToDefaults()
    {
        musicVolume = 0.7f;
        sfxVolume = 0.7f;
        ApplySettings();
        SaveSettings();
        Debug.Log("Settings reset to defaults!");
    }
}