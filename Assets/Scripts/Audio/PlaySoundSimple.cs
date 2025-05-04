using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

namespace GASHAPWN.Audio
{
    public class PlaySoundSimple : MonoBehaviour
    {
        // Uses AudioSourcePool to play given sound clip at position of object this script is attached to
        public AudioMixerGroup audioMixer;
        public void PlaySound(AudioClip clip)
        {
            // Create a new game object to play the audio clip, and position it at the game object's location
            var audioObj = AudioSourcePool.Instance.GetAudioSource();
            audioObj.transform.position = this.transform.position;
            var audioSource = audioObj.GetComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = audioMixer; // set audio mixer
            audioSource.clip = clip; // Play the audio clip
            audioSource.Play();
            // Return audio source to pool when done playing
            StartCoroutine(ReturnAfterPlay(audioSource, clip));
        }

        private IEnumerator ReturnAfterPlay(AudioSource audioSource, AudioClip clip)
        {
            yield return new WaitUntil(() => !audioSource.isPlaying);
            AudioSourcePool.Instance.ReturnToPool(audioSource);
            Addressables.Release(clip);
            yield return null;
        }
    }
}

