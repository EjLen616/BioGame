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

    [Header("Music")]
    public AudioClip menuMusic;
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
    public AudioClip vitaminCaughtMidAirSound;
    public AudioClip vitaminDestroyedSound;

    private List<AudioClip> remainingTracks;
    private bool isMusicPlaying = false;
    private float musicVolume = 0.7f;
    private float sfxVolume = 0.7f;
    private bool isPaused = false;
    private string currentSceneType = "Menu";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();

            // Subscribe to scene changes
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Load settings from SettingsManager
        if (SettingsManager.Instance != null)
        {
            musicVolume = SettingsManager.Instance.musicVolume;
            sfxVolume = SettingsManager.Instance.sfxVolume;
            SetMusicVolume(musicVolume);
            SetSFXVolume(sfxVolume);
            Debug.Log($"AudioManager Start - Music: {musicVolume}, SFX: {sfxVolume}");
        }

        DetectSceneAndPlayMusic();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Small delay to let scene fully load
        Invoke(nameof(DetectSceneAndPlayMusic), 0.1f);

        // Re-apply settings after scene load
        if (SettingsManager.Instance != null)
        {
            musicVolume = SettingsManager.Instance.musicVolume;
            sfxVolume = SettingsManager.Instance.sfxVolume;
            SetMusicVolume(musicVolume);
            SetSFXVolume(sfxVolume);
            Debug.Log($"Scene loaded - Music: {musicVolume}, SFX: {sfxVolume}");
        }
    }

    void InitializeAudioSources()
    {
        // Music source
        GameObject musicObj = new GameObject("MusicSource");
        musicObj.transform.SetParent(transform);
        musicSource = musicObj.AddComponent<AudioSource>();
        musicSource.loop = false;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        // SFX source
        GameObject sfxObj = new GameObject("SFXSource");
        sfxObj.transform.SetParent(transform);
        sfxSource = sfxObj.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;

        // UI source (for clicks)
        GameObject uiObj = new GameObject("UISource");
        uiObj.transform.SetParent(transform);
        uiSource = uiObj.AddComponent<AudioSource>();
        uiSource.loop = false;
        uiSource.playOnAwake = false;
        uiSource.volume = sfxVolume;

        Debug.Log("Audio sources initialized");
    }

    public void DetectSceneAndPlayMusic()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"Scene loaded: {sceneName}");

        if (sceneName == "MainMenu" || sceneName == "Menu")
        {
            PlayMenuMusic();
        }
        else
        {
            PlayGameMusic();
        }
    }

    public void PlayMenuMusic()
    {
        if (menuMusic == null)
        {
            Debug.LogWarning("No menu music assigned!");
            return;
        }

        Debug.Log("Playing menu music");
        StopMusic();
        currentSceneType = "Menu";

        musicSource.clip = menuMusic;
        musicSource.loop = true;
        musicSource.volume = musicVolume; // Apply current volume
        musicSource.Play();
        isMusicPlaying = true;
    }

    public void PlayGameMusic()
    {
        if (gameMusicTracks == null || gameMusicTracks.Length == 0)
        {
            Debug.LogWarning("No game music tracks assigned!");
            return;
        }

        Debug.Log("Playing game music");
        StopMusic();
        currentSceneType = "Game";

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
        if (isPaused || currentSceneType != "Game") return;

        if (remainingTracks == null || remainingTracks.Count == 0)
        {
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
                musicSource.volume = musicVolume; // Apply current volume
                musicSource.Play();
                isMusicPlaying = true;
                Debug.Log($"Playing game track: {nextTrack.name}");

                // Schedule next track
                Invoke(nameof(PlayNextRandomTrack), nextTrack.length);
            }
            else
            {
                PlayNextRandomTrack();
            }
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
            Debug.Log($"Music volume set to: {musicVolume}");
        }
        else
        {
            Debug.LogWarning("MusicSource is null!");
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
        if (uiSource != null)
        {
            uiSource.volume = sfxVolume;
        }
        Debug.Log($"SFX volume set to: {sfxVolume}");
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    public void PauseAllAudio()
    {
        isPaused = true;
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Pause();
        if (sfxSource != null && sfxSource.isPlaying)
            sfxSource.Pause();
        if (uiSource != null && uiSource.isPlaying)
            uiSource.Pause();
        CancelInvoke(nameof(PlayNextRandomTrack));
    }

    public void ResumeAllAudio()
    {
        isPaused = false;

        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.UnPause();
            if (!musicSource.isPlaying && !isMusicPlaying)
            {
                if (currentSceneType == "Menu")
                    PlayMenuMusic();
                else
                    PlayGameMusic();
            }
            else if (!musicSource.isPlaying && musicSource.clip != null)
            {
                musicSource.Play();
                isMusicPlaying = true;
            }
        }

        if (sfxSource != null)
            sfxSource.UnPause();
        if (uiSource != null)
            uiSource.UnPause();

        // Restart music scheduling if needed
        if (currentSceneType == "Game" && musicSource != null && musicSource.clip != null && !musicSource.loop)
        {
            float remainingTime = musicSource.clip.length - musicSource.time;
            if (remainingTime > 0)
            {
                Invoke(nameof(PlayNextRandomTrack), remainingTime);
            }
            else
            {
                PlayNextRandomTrack();
            }
        }
    }

    // Sound Effects
    public void PlayButtonClick()
    {
        if (isPaused || buttonClickSound == null || uiSource == null) return;
        uiSource.PlayOneShot(buttonClickSound, sfxVolume);
    }

    public void PlayCustomSFX(AudioClip clip)
    {
        if (isPaused || clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
    public void PlayWinSound()
    {
        if (isPaused || winSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(winSound, sfxVolume);
    }

    public void PlayLoseSound()
    {
        if (isPaused || loseSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(loseSound, sfxVolume);
    }

    public void PlayPointGainSound()
    {
        if (isPaused || pointGainSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(pointGainSound, sfxVolume);
    }

    public void PlayPointLoseSound()
    {
        if (isPaused || pointLoseSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(pointLoseSound, sfxVolume);
    }

    public void PlayCorrectCatchSound()
    {
        if (isPaused || correctCatchSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(correctCatchSound, sfxVolume);
    }

    public void PlayWrongCatchSound()
    {
        if (isPaused || wrongCatchSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(wrongCatchSound, sfxVolume);
    }

    public void PlayVitaminCaughtMidAirSound()
    {
        if (isPaused || vitaminCaughtMidAirSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(vitaminCaughtMidAirSound, sfxVolume);
    }

    public void PlayVitaminDestroyedSound()
    {
        if (isPaused || vitaminDestroyedSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(vitaminDestroyedSound, sfxVolume);
    }

    public void StopMusic()
    {
        musicSource.Stop();
        CancelInvoke(nameof(PlayNextRandomTrack));
        isMusicPlaying = false;
    }
}