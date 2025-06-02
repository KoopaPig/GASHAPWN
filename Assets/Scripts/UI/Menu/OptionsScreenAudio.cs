using UnityEngine;
using UnityEngine.Audio;
using TMPro;
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

        void Start()
        {
            float vol;

            theMixer.GetFloat("MasterVol", out vol);
            masterSlider.value = vol;
            theMixer.GetFloat("MusicVol", out vol);
            musicSlider.value = vol;
            theMixer.GetFloat("SoundVol", out vol);
            soundSlider.value = vol;

            masterLabel.text = Mathf.RoundToInt(masterSlider.value + 80).ToString();
            musicLabel.text = Mathf.RoundToInt(musicSlider.value + 80).ToString();
            soundLabel.text = Mathf.RoundToInt(soundSlider.value + 80).ToString();

        }

        public void SetMasterVol()
        {
            masterLabel.text = Mathf.RoundToInt(masterSlider.value + 80).ToString();
            theMixer.SetFloat("MasterVol", masterSlider.value);
            PlayerPrefs.SetFloat("MasterVol", masterSlider.value);
        }

        public void SetMusicVol()
        {
            musicLabel.text = Mathf.RoundToInt(musicSlider.value + 80).ToString();
            theMixer.SetFloat("MusicVol", musicSlider.value);
            PlayerPrefs.SetFloat("MusicVol", musicSlider.value);
        }

        public void SetSoundVol()
        {
            soundLabel.text = Mathf.RoundToInt(soundSlider.value + 80).ToString();
            theMixer.SetFloat("SoundVol", soundSlider.value);
            PlayerPrefs.SetFloat("SoundVol", soundSlider.value);
        }
    }
}