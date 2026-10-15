
using UnityEngine;
using UnityEngine.Audio;

public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Sonidos")]
    [SerializeField] private AudioClip[] sfxClips;
    [SerializeField] private AudioClip jumpClip;

    private float musicVolume = 0.7f;
    private float sfxVolume = 0.7f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);

        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);

        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);

        if (mainMixer == null)
            return;

        float volumeDB = Mathf.Log10(Mathf.Max(musicVolume, 0.0001f)) * 20f;
        mainMixer.SetFloat("MusicVolume", volumeDB);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);

        if (mainMixer == null)
            return;

        float volumeDB = Mathf.Log10(Mathf.Max(sfxVolume, 0.0001f)) * 20f;
        mainMixer.SetFloat("SFXVolume", volumeDB);
    }

    public void PlayRandomSFX()
    {
        if (sfxSource == null || sfxClips == null || sfxClips.Length == 0)
            return;

        AudioClip clip = sfxClips[0];

        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayJumpSound()
    {
        if (sfxSource != null && jumpClip != null)
            sfxSource.PlayOneShot(jumpClip);
    }

    public void PlayButtonSound()
    {
        if (sfxSource == null || sfxClips == null || sfxClips.Length == 0)
            return;

        AudioClip clip = sfxClips[sfxClips.Length - 1];

        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }
}
