using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public AudioMixerGroup audioMixGroup;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0.1f, 3f)]
    public float pitch = 1f;

    public bool loop = false;
    public bool playOnAwake = false;

    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : Singleton<AudioManager>
{

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Sound Effects")]
    public Sound[] sounds;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float masterVolume;
    [Range(0f, 1f)]
    public float musicVolume;
    [Range(0f, 1f)]
    public float sfxVolume;

    private Dictionary<string, Sound> soundDictionary;

    private Queue<AudioSource> audioSourcePool;
    private List<AudioSource> activeAudioSources;

    [Header("Pool Settings")]
    public int poolSize = 10;

    private void Awake()
    {
        if(Exists())
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this);
    }


    public void InitializeAudioManager()
    {
        // Tạo dictionary cho sounds
        soundDictionary = new Dictionary<string, Sound>();

        // Tạo AudioSource pool
        audioSourcePool = new Queue<AudioSource>();
        activeAudioSources = new List<AudioSource>();

        // Setup sounds
        foreach (Sound sound in sounds)
        {
            // Tạo AudioSource cho mỗi sound
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.outputAudioMixerGroup = sound.audioMixGroup;
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.playOnAwake = sound.playOnAwake;

            // Thêm vào dictionary
            if (!soundDictionary.ContainsKey(sound.name))
            {
                soundDictionary.Add(sound.name, sound);
            }
        }

        // Tạo pool AudioSource
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource pooledSource = gameObject.AddComponent<AudioSource>();
            pooledSource.playOnAwake = false;
            audioSourcePool.Enqueue(pooledSource);
        }

        // Load saved volume settings
        LoadVolumeSettings();
    }

    #region Play Sounds

    /// <summary>
    /// Phát sound effect một lần
    /// </summary>
    public void PlaySFX(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            Sound sound = soundDictionary[soundName];
            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(sound.clip, sound.volume * sfxVolume);
            }
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
        }
    }

    /// <summary>
    /// Phát sound effect với volume tùy chỉnh
    /// </summary>
    public void PlaySFX(string soundName, float volume)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            Sound sound = soundDictionary[soundName];
            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(sound.clip, volume * sfxVolume);
            }
        }
    }

    /// <summary>
    /// Phát sound với AudioSource riêng (có thể loop)
    /// </summary>
    public void PlaySound(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            Sound sound = soundDictionary[soundName];
            if (sound.source != null)
            {
                sound.source.volume = sound.volume * sfxVolume;
                sound.source.Play();
            }
        }
    }

    /// <summary>
    /// Phát nhạc nền
    /// </summary>
    public void PlayMusic(string musicName, bool loop = true)
    {
        if(soundDictionary.ContainsKey(musicName))
        {
            Sound sound = soundDictionary[musicName];
            if(sound.source != null)
            {
                sound.source.loop = loop;
                sound.source.volume = sound.volume * musicVolume;
                sound.source.Play();
            }
        }
    }

    /// <summary>
    /// Phát sound tại vị trí 3D
    /// </summary>
    public void PlaySoundAtPosition(string soundName, Vector3 position)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            Sound sound = soundDictionary[soundName];
            AudioSource pooledSource = GetPooledAudioSource();

            if (pooledSource != null)
            {
                pooledSource.transform.position = position;
                pooledSource.clip = sound.clip;
                pooledSource.volume = sound.volume * sfxVolume;
                pooledSource.pitch = sound.pitch;
                pooledSource.spatialBlend = 1f; // 3D sound
                pooledSource.Play();

                StartCoroutine(ReturnToPoolAfterPlay(pooledSource));
            }
        }
    }

    #endregion

    #region Stop/Pause Sounds

    public void StopSound(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            Sound sound = soundDictionary[soundName];
            if (sound.source != null && sound.source.isPlaying)
            {
                sound.source.Stop();
            }
        }
    }

    public void PauseSound(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            Sound sound = soundDictionary[soundName];
            if (sound.source != null && sound.source.isPlaying)
            {
                sound.source.Pause();
            }
        }
    }

    public void ResumeSound(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            Sound sound = soundDictionary[soundName];
            if (sound.source != null)
            {
                sound.source.UnPause();
            }
        }
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    #endregion

    #region Volume Control

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
        }
        SaveVolumeSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume > 0.5f ? 1f : 0f;

        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
        if (musicVolume > 0)
        {
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
        }
        else
        {
            audioMixer.SetFloat("MusicVolume", -80f); // Giá trị rất nhỏ để tắt âm thanh
        }
        SaveVolumeSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume > 0.5f ? 1f : 0f;
        if (sfxVolume > 0)
        {
            audioMixer.SetFloat("SoundVolume", Mathf.Log10(sfxVolume) * 20);
        }
        else
        {
            audioMixer.SetFloat("SoundVolume", -80f); // Giá trị rất nhỏ để tắt âm thanh
        }
        SaveVolumeSettings();
    }

    #endregion

    #region Audio Source Pool

    private AudioSource GetPooledAudioSource()
    {
        if (audioSourcePool.Count > 0)
        {
            AudioSource pooledSource = audioSourcePool.Dequeue();
            activeAudioSources.Add(pooledSource);
            return pooledSource;
        }

        // Nếu pool hết, tạo mới
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        activeAudioSources.Add(newSource);
        return newSource;
    }

    private IEnumerator ReturnToPoolAfterPlay(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);

        // Reset AudioSource
        source.clip = null;
        source.volume = 1f;
        source.pitch = 1f;
        source.spatialBlend = 0f;
        source.transform.position = transform.position;

        // Trả về pool
        activeAudioSources.Remove(source);
        audioSourcePool.Enqueue(source);
    }

    #endregion

    #region Fade Effects

    public void FadeInMusic(float duration)
    {
        if (musicSource != null)
        {
            StartCoroutine(FadeAudioSource(musicSource, 0f, musicVolume, duration));
        }
    }

    public void FadeOutMusic(float duration)
    {
        if (musicSource != null)
        {
            StartCoroutine(FadeAudioSource(musicSource, musicSource.volume, 0f, duration));
        }
    }

    private IEnumerator FadeAudioSource(AudioSource audioSource, float startVolume, float endVolume, float duration)
    {
        float elapsed = 0f;
        audioSource.volume = startVolume;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, elapsed / duration);
            yield return null;
        }

        audioSource.volume = endVolume;

        if (endVolume == 0f)
        {
            audioSource.Stop();
        }
    }

    #endregion

    #region Save/Load Settings

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SoundVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SoundVolume", 1f);

        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }

    #endregion

    #region Utility

    public bool IsSoundPlaying(string soundName)
    {
        if (soundDictionary.ContainsKey(soundName))
        {
            return soundDictionary[soundName].source.isPlaying;
        }
        return false;
    }

    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    #endregion
}