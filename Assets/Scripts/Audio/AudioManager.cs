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

        // Given AudioClip callback, return a random AudioClip
        public void GetRandomAudioClip(System.Action<AudioClip> callback)
        {
            if (addressableKeys.Count < 2)
            {
                Debug.LogWarning($"SFXGroup '{groupTag}' needs at least 2 addressable keys assigned.");
                callback?.Invoke(null);
                return;
            }

            // Make sure newly selected clip is not the same clip
            int i = UnityEngine.Random.Range(0, addressableKeys.Count);
            while (i == previousIndex && addressableKeys.Count > 1)
            {
                i = UnityEngine.Random.Range(0, addressableKeys.Count);
            }
            previousIndex = i;

            string selectedKey = addressableKeys[i];

            // Load the audio clip asynchronously
            Addressables.LoadAssetAsync<AudioClip>(selectedKey).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    callback?.Invoke(handle.Result);
                }
                else
                {
                    Debug.LogError($"Failed to load AudioClip with key: {selectedKey}");
                    callback?.Invoke(null);
                }
            };
        }
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Mixers")]
        // Get reference to the mixer
        public AudioMixer theMixer;
        public AudioMixerGroup SFXMixer;
        public AudioMixerGroup MusicMixer;

        [Header("SFXGroups")]
        [SerializeField] private SFXGroup testSFXGroup;

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

        /// <summary>
        /// Plays sound given key, pitch, and transform
        /// </summary>
        /// <param name="addressableKey"></param>
        /// <param name="transform"></param>
        /// <param name="pitch"></param>
        public void PlaySound(string addressableKey, Transform transform, float pitch = 1.0f)
        {
            Addressables.LoadAssetAsync<AudioClip>(addressableKey).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    // Create a new game object to play the audio clip, and position it at the game object's location
                    var audioObj = AudioSourcePool.Instance.GetAudioSource();
                    audioObj.transform.position = transform.position;
                    var audioSource = audioObj.GetComponent<AudioSource>();
                    audioSource.outputAudioMixerGroup = SFXMixer; // set audio mixer
                    audioSource.clip = handle.Result; // Play the audio clip
                    audioSource.pitch = pitch; // Apply pitch variation (none by default)
                    audioSource.Play();
                    // Return audio source to pool when done playing
                    StartCoroutine(AudioSourcePool.Instance.ReturnToPool(audioSource));
                    // Release memory after playing
                    Addressables.Release(handle.Result);

                }
            };
        }

        /// <summary>
        /// Plays sound given key and pitch
        /// </summary>
        /// <param name="addressableKey"></param>
        /// <param name="pitch"></param>
        public void PlaySound(string addressableKey, float pitch = 1.0f) {
            Addressables.LoadAssetAsync<AudioClip>(addressableKey).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    // Create a new game object to play the audio clip, and position it at the game object's location
                    var audioObj = AudioSourcePool.Instance.GetAudioSource();
                    audioObj.transform.position = this.transform.position;
                    var audioSource = audioObj.GetComponent<AudioSource>();
                    audioSource.outputAudioMixerGroup = SFXMixer; // set audio mixer
                    audioSource.clip = handle.Result; // Play the audio clip
                    audioSource.pitch = pitch; // Apply pitch variation (none by default)
                    audioSource.Play();
                    // Return audio source to pool when done playing
                    StartCoroutine(AudioSourcePool.Instance.ReturnToPool(audioSource));
                    // Release memory after playing
                    Addressables.Release(handle.Result);
                }
            };
        }

        /// <summary>
        /// Plays random sound from SFXGroup
        /// </summary>
        /// <param name="sfxGroup"></param>
        public void PlayRandomSound(SFXGroup sfxGroup)
        {
            sfxGroup.GetRandomAudioClip(clip =>
            {
                if (clip != null)
                {
                    var audioObj = AudioSourcePool.Instance.GetAudioSource();
                    var audioSource = audioObj.GetComponent<AudioSource>();
                    audioObj.transform.position = this.transform.position;
                    audioSource.clip = clip;
                    audioSource.Play();

                    StartCoroutine(AudioSourcePool.Instance.ReturnToPool(audioSource));
                    Addressables.Release(clip); // Release memory when done
                }
            });
        }

        /// <summary>
        /// Plays random sound from SFXGroup given transform
        /// </summary>
        /// <param name="sfxGroup"></param>
        /// <param name="transform"></param>
        public void PlayRandomSound(SFXGroup sfxGroup, Transform transform)
        {
            sfxGroup.GetRandomAudioClip(clip =>
            {
                if (clip != null)
                {
                    var audioObj = AudioSourcePool.Instance.GetAudioSource();
                    var audioSource = audioObj.GetComponent<AudioSource>();
                    audioObj.transform.position = transform.position;
                    audioSource.clip = clip;
                    audioSource.Play();

                    StartCoroutine(AudioSourcePool.Instance.ReturnToPool(audioSource));
                    Addressables.Release(clip); // Release memory when done
                }
            });
        }


        // TODO: Function to play SFXGroup in order with looping, for charge roll
        // This might need to be interfaced more directly with the charge roll, because it requires a conditional
        public void PlaySoundDynamicTriad(SFXGroup sfxGroup, Transform transform)
        {

        }

        private void Update()
        {
            // DEBUG FUNCTIONS
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                PlaySound("SFX_UI_Click_Generic_Cute");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                PlayRandomSound(testSFXGroup);
            }
        }

        private void OnDisable()
        {
            // unsub from GameManager stuff here
        }
    }
}


