using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;

public class OptionsScreenAudio : MonoBehaviour
{
    // Get reference to the mixer the sliders should edit
    public AudioMixer theMixer;

    public Slider masterSlider, musicSlider, soundSlider;
    public TMP_Text masterLabel, musicLabel, soundLabel;

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
