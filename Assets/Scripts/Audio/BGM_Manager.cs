using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI.Table;
using GASHAPWN;
using System.Collections;

namespace GASHAPWN.Audio {
    public class BGM_Manager : MonoBehaviour
    {
        public static BGM_Manager Instance { get; private set; }


        [SerializeField] private AudioMixerGroup musicMixer;

        [Header("Addressable Keys for Music")]
        [SerializeField] private string battleMusicKey;
        [SerializeField] private string menuMusicKey;
        [SerializeField] private string levelSelectMusicKey;
        [SerializeField] private string collectionMusicKey;

        private AudioSource mainAudioSource;

        private AsyncOperationHandle<AudioClip> currentHandle;

        private void Awake()
        {
            // Check for other instances
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            // Persistance
            DontDestroyOnLoad(this);

            FindOrCreateAudioSource(); // Ensure a valid audio source
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            FindOrCreateAudioSource();
            // Can only set music based on GameState/BattleState when scene is loaded
            SetMusicState();
        }

        private void OnDisable()
        {
           SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        private void SetMusicState()
        {
            GameState currentGameState = GameManager.Instance.State;
            BattleState currentBattleState;

            string audioKey = "";

            // Set the addressable key based on the game state
            switch (currentGameState)
            {
                case GameState.Battle:
                    audioKey = battleMusicKey;
                    break;
                case GameState.Title:
                    audioKey = menuMusicKey;
                    break;
                case GameState.LevelSelect:
                    audioKey = levelSelectMusicKey;
                    break;
                case GameState.Collection:
                    audioKey = collectionMusicKey;
                    break;
                default:
                    Debug.LogWarning("BGM_Manager: No music key set for current GameState.");
                    return;
            }

            // Set the addressable key based on the battle state
            if (BattleManager.Instance != null)
            {
                currentBattleState = BattleManager.Instance.State;
                switch (currentBattleState)
                {
                    case BattleState.Sleep:
                        // no music
                        break;
                    case BattleState.Battle:
                        break;
                    case BattleState.CountDown:
                        // battle music should start playing during countdown
                        break;
                    case BattleState.VictoryScreen:
                        // switch to victory music
                        break;
                    case BattleState.NewFigureScreen:
                        // no music
                        break;
                    default:
                        Debug.LogWarning("BGM_Manager: No music key set for current BattleState.");
                        return;
                }
            }

                LoadAndPlayMusic(audioKey);
        }

        private void SetMusicBattleState()
        {
            BattleState currentState = BattleManager.Instance.State;

        }

        private void LoadAndPlayMusic(string addressableKey)
        {
            FindOrCreateAudioSource(); // Ensure AudioSource exists

            if (!string.IsNullOrEmpty(addressableKey))
            {
                // Stop the current music
                if (mainAudioSource.isPlaying)
                {
                    mainAudioSource.Stop();
                }
                // Release from addressables
                if (currentHandle.IsValid())
                {
                    Addressables.Release(currentHandle);
                }
                // Load in new music
                Addressables.LoadAssetAsync<AudioClip>(addressableKey).Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        AudioClip clip = handle.Result;
                        if (clip == null)
                        {
                            Debug.LogError("BGM_Manager: Loaded AudioClip is NULL!");
                            return;
                        }

                        currentHandle = handle;
                        mainAudioSource.clip = handle.Result;
                        mainAudioSource.loop = true;
                        mainAudioSource.Play();
                    }
                    else
                    {
                        Debug.LogError($"BGM_Manager: Failed to load music: {addressableKey}");
                    }
                };
            }
        }

        private void StopCurrentMusic()
        {
            if (mainAudioSource.isPlaying)
            {
                mainAudioSource.Stop();
            } else
            {
                Debug.LogWarning("BGM_Manager: There is no music to stop");
            }
        }

        // FadeOutMusic given duration
        private IEnumerator FadeOutMusic(float fadeDuration)
        {
            if (mainAudioSource == null)
            {
                Debug.LogWarning("BGM_Manager: FadeOutMusic: AudioSource is null.");
                yield break;
            }

            if (!mainAudioSource.isPlaying)
            {
                Debug.LogWarning("BGM_Manager: FadeOutMusic: AudioSource is not playing.");
                yield break;
            }

            float startVolume = mainAudioSource.volume;
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                mainAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
                yield return null;
            }

            mainAudioSource.volume = 0f;
            mainAudioSource.Stop(); // Stop the music after fade-out

        }

        // TODO: Consider support for stings, and then afterwards the normal music continues.

        // This function tries to find an audioSource to use in the scene (attached to the main camera)
        // If not, it creates a new one.
        private void FindOrCreateAudioSource()
        {
            if (mainAudioSource != null) return;

            var cameraAudioSource = FindFirstObjectByType<Camera>()?.GetComponent<AudioSource>();

            if (cameraAudioSource != null)
            {
                mainAudioSource = cameraAudioSource;
            }
            else
            {
                // If no AudioSource found, create one
                GameObject newAudioObject = new GameObject("BGM_AudioSource");
                mainAudioSource = newAudioObject.AddComponent<AudioSource>();
                mainAudioSource.volume = 0.6f;
                mainAudioSource.outputAudioMixerGroup = musicMixer;
                DontDestroyOnLoad(newAudioObject);
                Debug.Log("BGM_Manager: Created new persistent AudioSource.");
            }
        }
    }
}

        
