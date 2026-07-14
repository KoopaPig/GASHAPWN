using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace GASHAPWN.Audio {
    /// <summary>
    /// Manages background music based on game states
    /// </summary>
    public class BGM_Manager : MonoBehaviour
    {
        public static BGM_Manager Instance { get; private set; }

        [Tooltip("Reference to Music Mixer")]
        [SerializeField] private AudioMixerGroup musicMixer;

        [Header("Addressable Keys for Music")]
            [SerializeField] private string battleMusicKey;
            [SerializeField] private string menuMusicKey;
            [SerializeField] private string levelSelectMusicKey;
            [SerializeField] private string collectionMusicKey;
            [SerializeField] private string resultsScreenMusicKey;

        private AudioSource mainAudioSource;

        private AsyncOperationHandle<AudioClip> currentHandle;

        private BattleManager _previousBattleManager = null;
        private Tween _musicFadeTween;

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
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            // Wait until BattleManager is present to subscribe
        }
        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            FindOrCreateAudioSource();
            // Can only set music based on GameState/BattleState when scene is loaded
            SetMusicStates();
            if (BattleManager.Instance != _previousBattleManager)
            {
                Debug.Log($"{nameof(BGM_Manager)}: BattleManager has changed! Updating subscription.");
                UnsubscribeFromBattleManager();
                StartCoroutine(WaitForBattleManagerAndSubscribe());
            }
        }

        private void OnSceneUnloaded(Scene arg0)
        {
            UnsubscribeFromBattleManager();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            UnsubscribeFromBattleManager();
        }

        private void SubscribeToBattleManager(BattleManager battleManager)
        {
            // Keep track of previous BattleManager
            _previousBattleManager = battleManager;

            BattleManager.OnBattleStateChanged += SetMusicBattleState;
            BattleManager.Instance.OnGamePaused += HandleGamePaused;
            BattleManager.Instance.OnGameUnpaused += HandleGameUnpaused;
            SetMusicBattleState(battleManager.State);
        }
        
        private void UnsubscribeFromBattleManager()
        {
            // Unsubscribe from the previous BattleManager if it's not null
            if (_previousBattleManager != null)
            {
                BattleManager.OnBattleStateChanged -= SetMusicBattleState;
                BattleManager.Instance.OnGamePaused -= HandleGamePaused;
                BattleManager.Instance.OnGameUnpaused -= HandleGameUnpaused;
            }
        }

        private IEnumerator WaitForBattleManagerAndSubscribe()
        {
            yield return new WaitUntil(() => BattleManager.Instance != null);
            Debug.Log($"{nameof(BGM_Manager)}: BattleManager instance found, subscribing to events");
            SubscribeToBattleManager(BattleManager.Instance);
        }

        private void SetMusicStates()
        {
            SetMusicGameState(GameManager.Instance.State);
        }

        private void SetMusicGameState(GameState state)
        {
            //Debug.Log($"{nameof(BGM_Manager)}: Setting Game Music State to {state.ToString()}");
            string audioKey = "";

            // Set the addressable key based on the game state
            switch (state)
            {
                case GameState.Battle:
                    // Battle BGM is handled in SetMusicBattleState
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
                    Debug.LogWarning($"{nameof(BGM_Manager)}: No music key set for current GameState.");
                    return;
            }

            LoadAndPlayMusic(audioKey);
        }

        private void SetMusicBattleState(BattleState state)
        {
            //Debug.Log($"{nameof(BGM_Manager)}: Setting Battle Music State to {state.ToString()}");
            string audioKey = "";

            // Set the addressable key based on the battle state
            if (BattleManager.Instance != null)
            {
                switch (state)
                {
                    case BattleState.Sleep:
                        //StopCurrentMusic();
                        break;
                    case BattleState.Battle:
                        break;
                    case BattleState.CountDown:
                        // battle music should start playing during countdown
                        audioKey = battleMusicKey;
                        break;
                    case BattleState.ResultsScreen:
                        StopCurrentMusic();
                        audioKey = resultsScreenMusicKey;
                        break;
                    case BattleState.NewFigureScreen:
                        // fade out results screen music
                        FadeOutMusic(3f);
                        break;
                    case BattleState.SuddenDeath:
                        // nothing here
                        break;
                    default:
                        Debug.LogWarning($"{nameof(BGM_Manager)}: No music key set for current BattleState.");
                        return;
                }

                LoadAndPlayMusic(audioKey);

            }

        }

        // need to revise this
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
                            Debug.LogError($"{nameof(BGM_Manager)}: Loaded AudioClip is NULL!");
                            return;
                        }

                        currentHandle = handle;
                        mainAudioSource.clip = handle.Result;
                        mainAudioSource.loop = true;
                        mainAudioSource.Play();
                    }
                    else
                    {
                        Debug.LogError($"{nameof(BGM_Manager)}: Failed to load music: {addressableKey}");
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
                Debug.LogWarning($"{nameof(BGM_Manager)}: There is no music to stop");
            }
        }

        // FadeOutMusic given duration
        private void FadeOutMusic(float fadeDuration)
        {
            if (mainAudioSource == null)
            {
                Debug.LogWarning($"{nameof(BGM_Manager)}: AudioSource is null.");
                return;
            }

            if (!mainAudioSource.isPlaying)
            {
                Debug.LogWarning($"{nameof(BGM_Manager)}: AudioSource is not playing.");
                return;
            }

            _musicFadeTween?.Kill();

            _musicFadeTween = mainAudioSource
                .DOFade(0f, fadeDuration)
                .SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    mainAudioSource.Stop();
                    mainAudioSource.volume = 1f; // reset for next track
                });
        }

        // Handle music when game paused
        private void HandleGamePaused(PlayerInput input)
        {
            if (mainAudioSource == null) return;
            if (!mainAudioSource.isPlaying) return;
            mainAudioSource.Pause();
        }

        // Handle music when game unpaused
        private void HandleGameUnpaused()
        {
            if (mainAudioSource == null) return;
            if (mainAudioSource.isPlaying) return;
            mainAudioSource.UnPause();
        }

        // CONSIDER: support for stings, and then afterwards the normal music continues.

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
                Debug.Log($"{nameof(BGM_Manager)}: Created new persistent AudioSource.");
            }
        }
    }
}

        
