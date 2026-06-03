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

    [Header("Save Settings")]
    public string saveFileName = "gamesettings.json";

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
            LoadSettings();
            ApplySettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplySettings();
        SaveSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplySettings();
        SaveSettings();
    }

    void ApplySettings()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(musicVolume);
            AudioManager.Instance.SetSFXVolume(sfxVolume);
        }
    }

    void SaveSettings()
    {
        SettingsData data = new SettingsData
        {
            musicVolume = musicVolume,
            sfxVolume = sfxVolume
        };

        string json = JsonUtility.ToJson(data, true);
        string filePath = Path.Combine(Application.persistentDataPath, saveFileName);
        File.WriteAllText(filePath, json);
    }

    void LoadSettings()
    {
        string filePath = Path.Combine(Application.persistentDataPath, saveFileName);

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            SettingsData data = JsonUtility.FromJson<SettingsData>(json);

            if (data != null)
            {
                musicVolume = data.musicVolume;
                sfxVolume = data.sfxVolume;
            }
        }
    }

    public void ResetToDefaults()
    {
        musicVolume = 0.7f;
        sfxVolume = 0.7f;
        ApplySettings();
        SaveSettings();
    }
}