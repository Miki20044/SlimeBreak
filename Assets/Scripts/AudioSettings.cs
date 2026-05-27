using UnityEngine;
using UnityEngine.Audio;

public class AudioSettings : MonoBehaviour
{
    public static AudioSettings instance;

    [Header("Audio Mixer")]
    public AudioMixer mixer;

    [Header("Exposed Mixer Parameters")]
    public string masterParam = "MasterVolume";
    public string musicParam = "MusicVolume";
    public string sfxParam = "SfxVolume";

    const string KEY_MASTER = "vol_master";
    const string KEY_MUSIC = "vol_music";
    const string KEY_SFX = "vol_sfx";

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ApplyAll();
    }

    public void ApplyAll()
    {
        SetMasterVolume(GetMasterVolume());
        SetMusicVolume(GetMusicVolume());
        SetSFXVolume(GetSFXVolume());
    }

    // ============ SETTERS (do podpiecia pod Slider.OnValueChanged) ============

    public void SetMasterVolume(float linearVolume)
    {
        PlayerPrefs.SetFloat(KEY_MASTER, linearVolume);
        ApplyMixerVolume(masterParam, linearVolume);

        if (mixer == null)
            AudioListener.volume = linearVolume; // fallback bez mixera
    }

    public void SetMusicVolume(float linearVolume)
    {
        PlayerPrefs.SetFloat(KEY_MUSIC, linearVolume);
        ApplyMixerVolume(musicParam, linearVolume);
    }

    public void SetSFXVolume(float linearVolume)
    {
        PlayerPrefs.SetFloat(KEY_SFX, linearVolume);
        ApplyMixerVolume(sfxParam, linearVolume);
    }

    // ============ GETTERS (do inicjalizacji sliderow) ============

    public float GetMasterVolume() => PlayerPrefs.GetFloat(KEY_MASTER, 1f);
    public float GetMusicVolume() => PlayerPrefs.GetFloat(KEY_MUSIC, 1f);
    public float GetSFXVolume() => PlayerPrefs.GetFloat(KEY_SFX, 1f);

    void ApplyMixerVolume(string param, float linearVolume)
    {
        if (mixer == null) return;

        // 0..1 -> dB (-80 do 0). log10 dla logarytmicznej skali ucha
        float db = linearVolume > 0.0001f
            ? Mathf.Log10(linearVolume) * 20f
            : -80f;

        mixer.SetFloat(param, db);
    }
}
