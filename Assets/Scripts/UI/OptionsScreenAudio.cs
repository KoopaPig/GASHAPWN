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

        vol = PlayerPrefs.GetFloat("MasterVol");
        masterSlider.value = vol;
        vol = PlayerPrefs.GetFloat("MusicVol");
        musicSlider.value = vol;
        vol = PlayerPrefs.GetFloat("NoteSoundVol");
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

    public void SetNoteVol()
    {
        soundLabel.text = Mathf.RoundToInt(soundSlider.value + 80).ToString();
        theMixer.SetFloat("SoundVol", soundSlider.value);
        PlayerPrefs.SetFloat("SoundVol", soundSlider.value);
    }
}
