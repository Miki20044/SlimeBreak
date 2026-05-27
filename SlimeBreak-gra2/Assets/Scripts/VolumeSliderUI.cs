using UnityEngine;
using UnityEngine.UI;

// Wrzuc ten skrypt na panel ustawien (CzyngsPanel) i podepnij 3 slidery.
// Skrypt automatycznie ustawi ich wartosci na te z PlayerPrefs przy kazdym otwarciu.
public class VolumeSliderUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    void OnEnable()
    {
        if (AudioSettings.instance == null) return;

        if (masterSlider != null)
            masterSlider.SetValueWithoutNotify(AudioSettings.instance.GetMasterVolume());

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(AudioSettings.instance.GetMusicVolume());

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(AudioSettings.instance.GetSFXVolume());
    }
}
