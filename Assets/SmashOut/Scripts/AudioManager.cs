using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager S_Instance = null;

    [Header ("Audio Sources")]
    public AudioSource EfxAudioSource;
    public AudioSource MusicAudioSource;

    [Header ("Background Music")]
    public AudioClip MenuMusicAudio;
    public AudioClip GameMusicAudio;

    [Header("Sound Effects")]
    public AudioClip ButtonClickAudio;
    public AudioClip GameOverAudio;
    public AudioClip SlideAudio;
    public AudioClip CounterAudio;
    public AudioClip SmashAudio;

    private bool _isMuteMusic;
    private bool _isMuteEfx;
    private AudioClip _lastRequestedMusicAudio;

    void Awake()
    {
        if (S_Instance == null)
            S_Instance = this;
        else if (S_Instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    // Use this for initialization
    void Start()
    {
        _isMuteMusic = PlayerPrefs.GetInt("MuteMusic") == 1;
        _isMuteEfx = PlayerPrefs.GetInt("MuteEfx") == 1;

        ApplyMuteStatesToAudioSources();
        PlayMusicClip(MenuMusicAudio);
    }

    public void PlayMusicClip(AudioClip clip)
    {
        // Don't restart the same clip if it's already playing
        if (MusicAudioSource.clip == clip && MusicAudioSource.isPlaying)
            return;

        _lastRequestedMusicAudio = clip;
        if (_isMuteMusic)
        {
            MusicAudioSource.clip = clip;
            MusicAudioSource.Pause();
            return;
        }

        MusicAudioSource.clip = clip;
        MusicAudioSource.Play();
    }

    private void StopMusic()
    {
        MusicAudioSource.Stop();
    }

    public void PlayEffectsAudio(AudioClip clip)
    {
        if (_isMuteEfx)
            return;

        EfxAudioSource.PlayOneShot(clip);
    }

    public void MuteMusicEffect()
    {
        if (_isMuteMusic)
        {
            _isMuteMusic = false;
            ApplyMuteStatesToAudioSources();
            // resume last requested track or fallback to menu
            PlayMusicClip(_lastRequestedMusicAudio != null ? _lastRequestedMusicAudio : MenuMusicAudio);
            PlayerPrefs.SetInt("MuteMusic", 0);
        }
        else
        {
            _isMuteMusic = true;
            ApplyMuteStatesToAudioSources();
            MusicAudioSource.Pause();
            PlayerPrefs.SetInt("MuteMusic", 1);
        }
    }

    public void MuteEfx()
    {
        if (_isMuteEfx)
            PlayerPrefs.SetInt("MuteEfx", 0);
        else
            PlayerPrefs.SetInt("MuteEfx", 1);

        _isMuteEfx = !_isMuteEfx;
        ApplyMuteStatesToAudioSources();
    }

    public bool IsMusicMuted()
    {
        return _isMuteMusic;
    }

    public bool IsEfxMuted()
    {
        return _isMuteEfx;
    }

    private void ApplyMuteStatesToAudioSources()
    {
        if (MusicAudioSource != null)
            MusicAudioSource.mute = _isMuteMusic;
        if (EfxAudioSource != null)
            EfxAudioSource.mute = _isMuteEfx;
    }
}
