using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace GASHAPWN.UI
{
    /// <summary>
    /// Controller for Audio-related options
    /// </summary>
    public class OptionsScreenAudio : MonoBehaviour
    {
        [Tooltip("The mixer the sliders should edit")]
        public AudioMixer theMixer;

        // References to sliders
        public Slider masterSlider, musicSlider, soundSlider;

        // References to slider labels
        public TextMeshProUGUI masterLabel, musicLabel, soundLabel;

        private void Start()
        {
            LoadVolume("MasterVol", masterSlider, masterLabel);
            LoadVolume("MusicVol", musicSlider, musicLabel);
            LoadVolume("SoundVol", soundSlider, soundLabel);
        }

        private void LoadVolume(string param, Slider slider, TMP_Text label)
        {
            float linear = PlayerPrefs.GetFloat(param, 1f); // default 100%
            slider.value = linear;
            label.text = Mathf.RoundToInt(linear * 100f).ToString();
            SetMixerVolume(param, linear);
        }

        private void LoadVolume(string param)
        {
            float linear = PlayerPrefs.GetFloat(param, 1f); // default 100%
            SetMixerVolume(param, linear);
        }

        private void UpdateVolume(string param, Slider slider, TMP_Text label)
        {
            float linear = slider.value;
            label.text = Mathf.RoundToInt(linear * 100f).ToString();

            SetMixerVolume(param, linear);
            PlayerPrefs.SetFloat(param, linear);
        }

        private void SetMixerVolume(string param, float linear)
        {
            if (linear <= 0f)
                theMixer.SetFloat(param, -80f);
            else
                theMixer.SetFloat(param, Mathf.Log10(linear) * 20f);
        }

        public void SetMasterVol() => UpdateVolume("MasterVol", masterSlider, masterLabel);

        public void SetMusicVol() => UpdateVolume("MusicVol", musicSlider, musicLabel);

        public void SetSoundVol() => UpdateVolume("SoundVol", soundSlider, soundLabel);

        public void LoadAllVolume() 
        {
            LoadVolume("MasterVol");
            LoadVolume("MusicVol");
            LoadVolume("SoundVol");
        }
    }
}