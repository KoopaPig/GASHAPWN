using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GASHAPWN.Audio {
    /// <summary>
    /// SFXGroup is used for returning a random sound from a group of sounds
    /// </summary>
    [System.Serializable]
    public class SFXGroup
    {
        public string groupTag = "Unnamed";
        public List<string> addressableKeys = new List<string>(); // List of addressableKeys
        private int previousIndex = -1;

        // Return a random Addressable in group
        public string GetRandomAudioAddress()
        {
            if (addressableKeys.Count < 2)
            {
                Debug.LogWarning($"SFXGroup '{groupTag}' needs at least 2 addressable keys assigned.");
                return null;
            }

            // Make sure newly selected clip is not the same clip
            int i = UnityEngine.Random.Range(0, addressableKeys.Count);
            while (i == previousIndex && addressableKeys.Count > 1)
            {
                i = UnityEngine.Random.Range(0, addressableKeys.Count);
            }
            previousIndex = i;

            return addressableKeys[i];
        }

        // Given AudioClip callback, return a AudioClip at given index
        public string GetAudioAddressAtIndex(int index)
        {
            if (index < 0 || index >= addressableKeys.Count)
            {
                Debug.LogWarning($"Invalid index {index} for SFXGroup '{groupTag}'.");
                return null;
            }

            return addressableKeys[index];
        }

        public int Size() { return addressableKeys.Count; }
    }


    /// <summary>
    /// SFXGroup_DynamicTriad is a specialized offshoot of SFXGroup
    /// Consists of 3 sounds, a "start", "hold", and "finish" sound
    /// Note: PlayerSFXProfile holds data for functionality of DynamicTriad
    /// </summary>
    [System.Serializable]
    public class  SFXGroup_DynamicTriad : SFXGroup
    {
        public enum TriadState { NONE, START, HOLD, FINISH }

        public void SetTriadState(TriadState state) { _currState = state; }

        public TriadState GetTriadState() { return _currState; }

        private void OnValidate() { Debug.LogAssertion(addressableKeys.Count == 3); }

        private SFXGroup_DynamicTriad() { _currState = TriadState.NONE; }

        private TriadState _currState;
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Mixers")]
        // Get reference to the mixer
        public AudioMixer theMixer;
        public AudioMixerGroup SFXMixer;
        public AudioMixerGroup MusicMixer;

        private void Awake()
        {
            // Check for other instances
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            // Subscribe to GameManager stuff here
        }

        private void Start()
        {
            if (PlayerPrefs.HasKey("MasterVol"))
            {
                theMixer.SetFloat("MasterVol", PlayerPrefs.GetFloat("MasterVol"));
            }
            if (PlayerPrefs.HasKey("MusicVol"))
            {
                theMixer.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVol"));
            }
            if (PlayerPrefs.HasKey("SoundVol"))
            {
                theMixer.SetFloat("SoundVol", PlayerPrefs.GetFloat("SoundVol"));
            }
        }

        private void OnDisable()
        {
            // unsub from GameManager stuff here
        }

        /// <summary>
        /// Plays sound given key, pitch, and loop
        /// </summary>
        /// <param name="addressableKey"></param>
        /// <param name="pitch"></param>
        /// <param name="loop"></param>
        public AudioSource PlaySound(string addressableKey, float pitch = 1.0f)
        {
            var audioSource = AudioSourcePool.Instance.GetAudioSource().GetComponent<AudioSource>();
            audioSource.transform.position = transform.position;
            audioSource.outputAudioMixerGroup = SFXMixer;
            audioSource.transform.position = transform.position;
            audioSource.pitch = pitch;

            Addressables.LoadAssetAsync<AudioClip>(addressableKey).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    audioSource.clip = handle.Result;
                    audioSource.Play();

                    StartCoroutine(ReturnAfterPlay(audioSource, handle.Result));
                }
            };
            return audioSource;
        }

        /// <summary>
        /// Plays sound given key, transform, pitch, and loop
        /// </summary>
        /// <param name="addressableKey"></param>
        /// <param name="transform"></param>
        /// <param name="pitch"></param>
        /// <param name="loop"></param>
        public AudioSource PlaySound(string addressableKey, Transform transform, float pitch = 1.0f, bool loop = false)
        {
            var audioSource = AudioSourcePool.Instance.GetAudioSource().GetComponent<AudioSource>();
            audioSource.transform.position = transform.position;
            audioSource.outputAudioMixerGroup = SFXMixer;
            audioSource.transform.position = transform.position;
            audioSource.loop = loop;
            audioSource.pitch = pitch;

            Addressables.LoadAssetAsync<AudioClip>(addressableKey).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    audioSource.clip = handle.Result;
                    audioSource.Play();

                    if (!loop)
                    {
                        StartCoroutine(ReturnAfterPlay(audioSource, handle.Result));
                    }
                }
            };
            return audioSource;
        }

        /// <summary>
        /// Plays random sound from SFXGroup
        /// </summary>
        /// <param name="sfxGroup"></param>
        public void PlayRandomSound(SFXGroup sfxGroup)
        {
            PlaySound(sfxGroup.GetRandomAudioAddress());
        }

        /// <summary>
        /// Plays random sound from SFXGroup given transform
        /// </summary>
        /// <param name="sfxGroup"></param>
        /// <param name="transform"></param>
        public void PlayRandomSound(SFXGroup sfxGroup, Transform transform)
        {
            PlaySound(sfxGroup.GetRandomAudioAddress(), transform);
        }

        /// <summary>
        /// Plays sound from SFXGroup given index
        /// </summary>
        /// <param name="sfxGroup"></param>
        /// <param name="transform"></param>
        /// <param name="index"></param>
        public void PlaySoundGivenIndex(SFXGroup sfxGroup, Transform transform, int index)
        {
            PlaySound(sfxGroup.GetAudioAddressAtIndex(index), transform);
        }

        /// <summary>
        /// Given a Dynamic Triad, PlayerSFXProfile, and TriadState, set states and handle sounds
        /// </summary>
        /// <param name="dynTriad"></param>
        /// <param name="profile"></param>
        /// <param name="state">Current state of the DynamicTriad</param>
        public void HandleSoundDynamicTriad(SFXGroup_DynamicTriad dynTriad, PlayerSFXProfile profile, SFXGroup_DynamicTriad.TriadState state)
        {
            profile.currChargeRollState = state;

            // Stop previous sound if any
            if (profile.currentChargeRollSource != null)
            {
                profile.currentChargeRollSource.Stop();
                AudioSourcePool.Instance.ReturnToPool(profile.currentChargeRollSource);
                profile.currentChargeRollSource = null;
            }

            switch (state)
            {
                case SFXGroup_DynamicTriad.TriadState.START:
                    profile.currentChargeRollSource = PlaySound(dynTriad.addressableKeys[0], profile.playerObject.transform);
                    break;
                case SFXGroup_DynamicTriad.TriadState.HOLD:
                    profile.currentChargeRollSource = PlaySound(dynTriad.addressableKeys[1], profile.playerObject.transform, loop: true);
                    break;
                case SFXGroup_DynamicTriad.TriadState.FINISH:
                    profile.currentChargeRollSource = PlaySound(dynTriad.addressableKeys[2], profile.playerObject.transform);
                    break;
            }
        }

        // Safely release clip from addressables after it 
        private IEnumerator ReturnAfterPlay(AudioSource audioSource, AudioClip clip)
        {
            yield return new WaitUntil(() => !audioSource.isPlaying);
            AudioSourcePool.Instance.ReturnToPool(audioSource);
            Addressables.Release(clip);
            yield return null;
        }
    }
}


