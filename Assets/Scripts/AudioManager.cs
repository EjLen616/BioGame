using UnityEngine;
using System.Collections.Generic;

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
    private string currentScene;
    private bool isPlayingRandom = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAudioSources()
    {
        // Create music source
        GameObject musicObj = new GameObject("MusicSource");
        musicObj.transform.SetParent(transform);
        musicSource = musicObj.AddComponent<AudioSource>();
        musicSource.loop = false;
        musicSource.playOnAwake = false;

        // Create SFX source
        GameObject sfxObj = new GameObject("SFXSource");
        sfxObj.transform.SetParent(transform);
        sfxSource = sfxObj.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        // Create UI source (for clicks and UI sounds)
        GameObject uiObj = new GameObject("UISource");
        uiObj.transform.SetParent(transform);
        uiSource = uiObj.AddComponent<AudioSource>();
        uiSource.loop = false;
        uiSource.playOnAwake = false;
    }

    void LoadSettings()
    {
        if (SettingsManager.Instance != null)
        {
            SetMusicVolume(SettingsManager.Instance.musicVolume);
            SetSFXVolume(SettingsManager.Instance.sfxVolume);
        }
        else
        {
            // Default volumes
            SetMusicVolume(0.7f);
            SetSFXVolume(0.7f);
        }
    }

    void Start()
    {
        // Subscribe to scene changes
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        currentScene = scene.name;
        HandleSceneMusic();
    }

    void HandleSceneMusic()
    {
        if (currentScene.Contains("Game") || currentScene.Contains("Level"))
        {
            // Game scene - play random game tracks
            if (gameMusicTracks != null && gameMusicTracks.Length > 0)
            {
                StartRandomGameMusic();
            }
        }
        else if (currentScene.Contains("Menu") || currentScene.Contains("Selection"))
        {
            // Menu scene - play menu music
            if (menuMusicTracks != null && menuMusicTracks.Length > 0)
            {
                PlayMenuMusic();
            }
        }
    }

    void StartRandomGameMusic()
    {
        if (isPlayingRandom) return;

        isPlayingRandom = true;
        remainingTracks = new List<AudioClip>(gameMusicTracks);

        if (shuffleMusic)
        {
            ShuffleRemainingTracks();
        }

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
        if (remainingTracks == null || remainingTracks.Count == 0)
        {
            // Refill the list
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

            musicSource.clip = nextTrack;
            musicSource.Play();

            // Schedule next track
            Invoke(nameof(PlayNextRandomTrack), nextTrack.length);
        }
    }

    void PlayMenuMusic()
    {
        isPlayingRandom = false;

        // Play first menu track or random
        if (menuMusicTracks.Length > 0)
        {
            int trackIndex = shuffleMusic ? Random.Range(0, menuMusicTracks.Length) : 0;
            musicSource.clip = menuMusicTracks[trackIndex];
            musicSource.Play();

            // Loop menu music
            musicSource.loop = true;
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        uiSource.volume = volume;
    }

    // Sound Effect Play Methods
    public void PlayButtonClick()
    {
        if (buttonClickSound != null)
            uiSource.PlayOneShot(buttonClickSound);
    }

    public void PlayWinSound()
    {
        if (winSound != null)
            sfxSource.PlayOneShot(winSound);
    }

    public void PlayLoseSound()
    {
        if (loseSound != null)
            sfxSource.PlayOneShot(loseSound);
    }

    public void PlayPointGainSound()
    {
        if (pointGainSound != null)
            sfxSource.PlayOneShot(pointGainSound);
    }

    public void PlayPointLoseSound()
    {
        if (pointLoseSound != null)
            sfxSource.PlayOneShot(pointLoseSound);
    }

    public void PlayCorrectCatchSound()
    {
        if (correctCatchSound != null)
            sfxSource.PlayOneShot(correctCatchSound);
    }

    public void PlayWrongCatchSound()
    {
        if (wrongCatchSound != null)
            sfxSource.PlayOneShot(wrongCatchSound);
    }

    public void PlayCustomSFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void StopMusic()
    {
        musicSource.Stop();
        CancelInvoke();
        isPlayingRandom = false;
    }

    public void ResumeMusic()
    {
        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }
}