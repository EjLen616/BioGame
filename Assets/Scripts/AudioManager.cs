using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource uiSource;

    [Header("Music Settings")]
    public AudioClip[] menuMusicTracks;
    public AudioClip[] gameMusicTracks;
    public bool shuffleMusic = true;

    [Header("Sound Effects")]
    public AudioClip buttonClickSound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip pointGainSound;
    public AudioClip pointLoseSound;
    public AudioClip correctCatchSound;
    public AudioClip wrongCatchSound;

    private List<AudioClip> remainingTracks;
    private string currentSceneType = ""; // "Menu" or "Game"
    private bool isMusicPlaying = false;
    private float musicVolume = 0.7f;
    private float sfxVolume = 0.7f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            LoadSettings();

            // Subscribe to scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initial music play
        Invoke(nameof(DetectSceneTypeAndPlayMusic), 0.1f);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void InitializeAudioSources()
    {
        // Create music source
        GameObject musicObj = new GameObject("MusicSource");
        musicObj.transform.SetParent(transform);
        musicSource = musicObj.AddComponent<AudioSource>();
        musicSource.loop = false;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        // Create SFX source
        GameObject sfxObj = new GameObject("SFXSource");
        sfxObj.transform.SetParent(transform);
        sfxSource = sfxObj.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;

        // Create UI source (for clicks and UI sounds)
        GameObject uiObj = new GameObject("UISource");
        uiObj.transform.SetParent(transform);
        uiSource = uiObj.AddComponent<AudioSource>();
        uiSource.loop = false;
        uiSource.playOnAwake = false;
        uiSource.volume = sfxVolume;
    }

    void LoadSettings()
    {
        if (SettingsManager.Instance != null)
        {
            musicVolume = SettingsManager.Instance.musicVolume;
            sfxVolume = SettingsManager.Instance.sfxVolume;
            SetMusicVolume(musicVolume);
            SetSFXVolume(sfxVolume);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Small delay to ensure everything is loaded
        Invoke(nameof(DetectSceneTypeAndPlayMusic), 0.1f);
    }

    // Changed from private to public
    public void DetectSceneTypeAndPlayMusic()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"Scene loaded: {sceneName} - Detecting music type...");

        // Check if this is a menu scene
        if (sceneName.Contains("Menu") || sceneName.Contains("Selection") || sceneName == "MainMenu")
        {
            Debug.Log("Menu scene detected - Playing menu music");
            PlayMenuMusic();
        }
        // Check if this is a game/level scene
        else if (sceneName.Contains("Level") || sceneName.Contains("Game") || sceneName == "GameScene")
        {
            Debug.Log("Game/Level scene detected - Playing game music");
            PlayGameMusic();
        }
        else
        {
            // Default to menu music if can't determine
            Debug.LogWarning($"Unknown scene type: {sceneName} - Defaulting to menu music");
            PlayMenuMusic();
        }
    }

    public void PlayMenuMusic()
    {
        if (menuMusicTracks == null || menuMusicTracks.Length == 0)
        {
            Debug.LogWarning("No menu music tracks assigned!");
            return;
        }

        Debug.Log($"Starting menu music. Found {menuMusicTracks.Length} tracks.");

        // Stop current music
        StopMusic();
        currentSceneType = "Menu";

        // Play first menu track or random
        int trackIndex = shuffleMusic ? Random.Range(0, menuMusicTracks.Length) : 0;
        AudioClip selectedTrack = menuMusicTracks[trackIndex];

        if (selectedTrack != null)
        {
            musicSource.clip = selectedTrack;
            musicSource.loop = true; // Loop menu music
            musicSource.Play();
            isMusicPlaying = true;
            Debug.Log($"Playing menu track: {selectedTrack.name}");
        }
        else
        {
            Debug.LogError("Selected menu track is null!");
        }
    }

    public void PlayGameMusic()
    {
        if (gameMusicTracks == null || gameMusicTracks.Length == 0)
        {
            Debug.LogWarning("No game music tracks assigned! Will use menu music instead.");
            // Fallback to menu music if no game tracks
            if (menuMusicTracks.Length > 0)
            {
                PlayMenuMusic();
            }
            return;
        }

        Debug.Log($"Starting game music. Found {gameMusicTracks.Length} tracks.");

        // Stop current music
        StopMusic();
        currentSceneType = "Game";

        // Reset remaining tracks for random play
        remainingTracks = new List<AudioClip>(gameMusicTracks);
        if (shuffleMusic)
        {
            ShuffleRemainingTracks();
        }

        // Play first track
        PlayNextRandomTrack();
    }

    void ShuffleRemainingTracks()
    {
        for (int i = 0; i < remainingTracks.Count; i++)
        {
            AudioClip temp = remainingTracks[i];
            int randomIndex = Random.Range(i, remainingTracks.Count);
            remainingTracks[i] = remainingTracks[randomIndex];
            remainingTracks[randomIndex] = temp;
        }
    }

    void PlayNextRandomTrack()
    {
        if (currentSceneType != "Game")
            return;

        if (remainingTracks == null || remainingTracks.Count == 0)
        {
            // Refill the list for continuous random play
            remainingTracks = new List<AudioClip>(gameMusicTracks);
            if (shuffleMusic)
            {
                ShuffleRemainingTracks();
            }
        }

        if (remainingTracks.Count > 0)
        {
            AudioClip nextTrack = remainingTracks[0];
            remainingTracks.RemoveAt(0);

            if (nextTrack != null)
            {
                musicSource.clip = nextTrack;
                musicSource.loop = false;
                musicSource.Play();
                isMusicPlaying = true;
                Debug.Log($"Playing game track: {nextTrack.name}");

                // Schedule next track
                Invoke(nameof(PlayNextRandomTrack), nextTrack.length);
            }
            else
            {
                Debug.LogError("Next game track is null!");
                // Try next track
                PlayNextRandomTrack();
            }
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
        if (uiSource != null)
        {
            uiSource.volume = volume;
        }
    }

    // Sound Effect Play Methods
    public void PlayButtonClick()
    {
        if (buttonClickSound != null && uiSource != null)
        {
            uiSource.PlayOneShot(buttonClickSound);
        }
    }

    public void PlayWinSound()
    {
        if (winSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(winSound);
        }
    }

    public void PlayLoseSound()
    {
        if (loseSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(loseSound);
        }
    }

    public void PlayPointGainSound()
    {
        if (pointGainSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(pointGainSound);
        }
    }

    public void PlayPointLoseSound()
    {
        if (pointLoseSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(pointLoseSound);
        }
    }

    public void PlayCorrectCatchSound()
    {
        if (correctCatchSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(correctCatchSound);
        }
    }

    public void PlayWrongCatchSound()
    {
        if (wrongCatchSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(wrongCatchSound);
        }
    }

    public void PlayCustomSFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
        CancelInvoke(nameof(PlayNextRandomTrack));
        isMusicPlaying = false;
    }

    public void ResumeMusic()
    {
        if (!isMusicPlaying && musicSource.clip != null)
        {
            musicSource.Play();
            isMusicPlaying = true;

            // Resume random track scheduling if in game mode
            if (currentSceneType == "Game" && !musicSource.loop)
            {
                float remainingTime = musicSource.clip.length - musicSource.time;
                Invoke(nameof(PlayNextRandomTrack), remainingTime);
            }
        }
    }
}