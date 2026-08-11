using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource uiSource;

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip[] gameMusicTracks; // 3 tracks for random play in game

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
    private string currentSceneType = "Menu"; // "Menu" or "Game"

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

    void Start()
    {
        // Detect current scene type
        DetectSceneAndPlayMusic();
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

    public void DetectSceneAndPlayMusic()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

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
        ShuffleRemainingTracks();
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
            ShuffleRemainingTracks();
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

                // Schedule next track
                Invoke(nameof(PlayNextRandomTrack), nextTrack.length);
            }
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (musicSource != null)
            musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        if (sfxSource != null)
            sfxSource.volume = volume;
        if (uiSource != null)
            uiSource.volume = volume;
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
        }

        if (sfxSource != null)
            sfxSource.UnPause();
        if (uiSource != null)
            uiSource.UnPause();
    }

    // Sound Effects
    public void PlayButtonClick()
    {
        if (isPaused || buttonClickSound == null || uiSource == null) return;
        uiSource.PlayOneShot(buttonClickSound);
    }

    public void PlayWinSound()
    {
        if (isPaused || winSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(winSound);
    }

    public void PlayLoseSound()
    {
        if (isPaused || loseSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(loseSound);
    }

    public void PlayPointGainSound()
    {
        if (isPaused || pointGainSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(pointGainSound);
    }

    public void PlayPointLoseSound()
    {
        if (isPaused || pointLoseSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(pointLoseSound);
    }

    public void PlayCorrectCatchSound()
    {
        if (isPaused || correctCatchSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(correctCatchSound);
    }

    public void PlayWrongCatchSound()
    {
        if (isPaused || wrongCatchSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(wrongCatchSound);
    }

    public void PlayVitaminCaughtMidAirSound()
    {
        if (isPaused || vitaminCaughtMidAirSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(vitaminCaughtMidAirSound);
    }

    public void PlayVitaminDestroyedSound()
    {
        if (isPaused || vitaminDestroyedSound == null || sfxSource == null) return;
        sfxSource.PlayOneShot(vitaminDestroyedSound);
    }

    public void StopMusic()
    {
        musicSource.Stop();
        CancelInvoke(nameof(PlayNextRandomTrack));
        isMusicPlaying = false;
    }
}