using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Audio;

namespace GASHAPWN.Audio
{
    /// <summary>
    /// PlayerSFXProfile: For player-related sound effects, need to get reference to 
    /// player object, playerTag, and playerData.
    /// </summary>
    public class PlayerSFXProfile
    {
        public PlayerSFXProfile(string tag)
        {
            playerTag = tag;
            playerObject = GameObject.FindGameObjectWithTag(playerTag);
            playerData = playerObject.GetComponent<PlayerData>();
            Initialize();
        }

        public GameObject playerObject { get; set; }
        protected string playerTag { get; set; }
        protected PlayerData playerData { get; set; }
        // Then probably get playerEffects in here eventually

        public void Initialize()
        {
            if (playerData != null)
            {
                playerData.OnDamage.AddListener(HandleDamageSFX);
                // Add more listeners here
            }
        }

        public void HandleDamageSFX(int val)
        {
            int i = playerData.maxHealth - playerData.currentHealth - 1;
            GAME_SFXManager.Instance.Play_OrcHitDamage(playerObject.transform, i);

        }

        private void OnDisable()
        {
            playerData.OnDamage.RemoveListener(HandleDamageSFX);
        }
    }

    public class GAME_SFXManager : MonoBehaviour
    {
        [SerializeField] private AudioMixerGroup soundMixer;

        public static GAME_SFXManager Instance { get; private set; }

        private PlayerSFXProfile player1SFXProfile, player2SFXProfile;

        [SerializeField] private SFXGroup orcHitDamageGroup;

        private void Awake()
        {
            // Check for other instances
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (BattleManager.Instance != null)
            {
                player1SFXProfile = new PlayerSFXProfile("Player1");
                player2SFXProfile = new PlayerSFXProfile("Player2");
                Debug.Log("GAME_SFXManager: PlayerSFXProfiles constructed");
            }
            else Debug.Log("GAME_SFXManager: PlayerSFXProfiles not constructed. Not in Battle Scene.");

        }

        public void Play_OrcHitDamage(Transform transform, int index)
        {
            AudioManager.Instance.PlaySoundGivenIndex(orcHitDamageGroup, transform, index);
        }

        // Interfacing with playerData.hasCharged and isCharging is the interface I need
        // isCharging initiated when holding button, released when release charge
        // hasCharged is set at end of coroutine, not really sure how useful hasCharged will be
    }
}