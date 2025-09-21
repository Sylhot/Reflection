using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip backgroundMusic;
    
    [Header("SFX Clips")]
    [SerializeField] private AudioClip bulletSound;
    [SerializeField] private AudioClip playerHitSound;
    [SerializeField] private AudioClip gameOverSound;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;
    
    // Singleton pattern
    public static SoundManager Instance;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // AudioSource'ları kontrol et
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
            
        // AudioSource ayarları
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }
    
    void Start()
    {
        // Background müziği başlat
        PlayBackgroundMusic();
    }
    
    void Update()
    {
        // Volume ayarlarını güncelle
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }
    
    #region Music Methods
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && !musicSource.isPlaying)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }
    
    public void StopBackgroundMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
    
    public void PauseBackgroundMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }
    
    public void ResumeBackgroundMusic()
    {
        if (!musicSource.isPlaying && musicSource.clip != null)
        {
            musicSource.UnPause();
        }
    }
    #endregion
    
    #region SFX Methods
    public void PlayBulletSound()
    {
        PlaySFX(bulletSound);
    }
    
    public void PlayPlayerHitSound()
    {
        PlaySFX(playerHitSound);
    }
    
    public void PlayGameOverSound()
    {
        PlaySFX(gameOverSound);
    }
    
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    
    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
    #endregion
    
    #region Volume Controls
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }
    
    public void MuteMusic(bool mute)
    {
        musicSource.mute = mute;
    }
    
    public void MuteSFX(bool mute)
    {
        sfxSource.mute = mute;
    }
    #endregion
}