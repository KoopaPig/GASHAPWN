using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace GASHAPWN.Audio
{
    /// <summary>
    /// PlayerSFXProfile: For player-related sound effects, need to get reference to 
    /// player object, playerTag, and playerData.
    /// This object holds sound-effect related data, but it allocates actual playback to GAME_SFXManager.
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

        // Get reference to various player components
        public GameObject playerObject { get; set; }
        protected string playerTag { get; set; }
        protected PlayerData playerData { get; set; }
        
        // Get playerEffects in here too?

        // For handling the Charge Roll Dynamic Triad SFX
        public SFXGroup_DynamicTriad.TriadState currChargeRollState = SFXGroup_DynamicTriad.TriadState.NONE;
        public Coroutine chargeRollRoutine;
        public AudioSource currentChargeRollSource;

        public void Initialize()
        {
            if (playerData != null)
            {
                playerData.OnDamage.AddListener(HandleDamageSFX);
                playerData.OnChargeRoll.AddListener(HandleChargeRollSFX);
                // Add more listeners here
            }
        }

        public void HandleDamageSFX(int val)
        {
            
            int i = Mathf.FloorToInt(playerData.maxHealth - playerData.currentHealth - 1);
            GAME_SFXManager.Instance.Play_OrcHitDamage(playerObject.transform, i);
        }


        public void HandleChargeRollSFX(bool isCharging)
        {
            GAME_SFXManager.Instance.HandleChargeRollSFX(playerObject.transform, isCharging, this);
        }

        ~PlayerSFXProfile()
        {
            playerData.OnDamage.RemoveListener(HandleDamageSFX);
            playerData.OnChargeRoll.RemoveListener(HandleChargeRollSFX);
        }
    }

    public class GAME_SFXManager : MonoBehaviour
    {
        public static GAME_SFXManager Instance { get; private set; }

        [Tooltip("Reference to Sound Mixer")]
        [SerializeField] private AudioMixerGroup soundMixer;

        #region SFX Groups
        [Header("SFX Groups")]
            [SerializeField] private SFXGroup_DynamicTriad chargeRollGroup;
            [SerializeField] private SFXGroup orcHitDamageGroup;
            [SerializeField] private SFXGroup impactGroupGeneral;
            [SerializeField] private SFXGroup impactGroupDeflect;
            [SerializeField] private SFXGroup impactGroupSlam;
        #endregion

        // PlayerSFXProfiles corresponding to each player (fully dynamic in future?)
        private PlayerSFXProfile _player1SFXProfile, _player2SFXProfile;


        /// PRIVATE METHODS ///

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
                _player1SFXProfile = new PlayerSFXProfile("Player1");
                _player2SFXProfile = new PlayerSFXProfile("Player2");
                Debug.Log("GAME_SFXManager: PlayerSFXProfiles constructed");
            }
            else Debug.Log("GAME_SFXManager: PlayerSFXProfiles not constructed. Not in Battle Scene.");

        }

        // Wait to transition DynamicTriad state from START to HOLD
        private IEnumerator WaitForStartToFinishThenHold(SFXGroup_DynamicTriad group, PlayerSFXProfile profile)
        {
            // This wait time is hard-coded for simplicity
            yield return new WaitForSeconds(0.7f);

            // Transition to HOLD only if still charging
            if (profile.currChargeRollState == SFXGroup_DynamicTriad.TriadState.START)
            {
                AudioManager.Instance.HandleSoundDynamicTriad(group, profile, SFXGroup_DynamicTriad.TriadState.HOLD);
            }
        }


        /// PUBLIC METHODS ///

        // Handle TriadState changes for Charge Roll given transform, isCharging bool, and PlayerSFXProfile
        public void HandleChargeRollSFX(Transform transform, bool isCharging, PlayerSFXProfile profile)
        {
            if (isCharging)
            {
                if (profile.currChargeRollState == SFXGroup_DynamicTriad.TriadState.NONE ||
                    profile.currChargeRollState == SFXGroup_DynamicTriad.TriadState.FINISH)
                {
                    AudioManager.Instance.HandleSoundDynamicTriad(chargeRollGroup, profile, SFXGroup_DynamicTriad.TriadState.START);

                    if (profile.chargeRollRoutine != null)
                        StopCoroutine(profile.chargeRollRoutine);
                    profile.chargeRollRoutine = StartCoroutine(WaitForStartToFinishThenHold(chargeRollGroup, profile));
                }
            }
            else
            {
                if (profile.currChargeRollState == SFXGroup_DynamicTriad.TriadState.START ||
                    profile.currChargeRollState == SFXGroup_DynamicTriad.TriadState.HOLD)
                {
                    if (profile.chargeRollRoutine != null)
                        StopCoroutine(profile.chargeRollRoutine);

                    AudioManager.Instance.HandleSoundDynamicTriad(chargeRollGroup, profile, SFXGroup_DynamicTriad.TriadState.FINISH);
                }
            }
        }

        public void Play_OrcHitDamage(Transform transform, int index)
        {
            AudioManager.Instance.PlaySoundGivenIndex(orcHitDamageGroup, transform, index);
        }

        public void Play_Drop(Transform transform)
        {
            AudioManager.Instance.PlaySound("SFX_Drop_3", transform);
        }

        public void Play_SlamImpact(Transform transform)
        {
            AudioManager.Instance.PlayRandomSound(impactGroupSlam, transform);
        }

        public void Play_DefenseActivate(Transform transform)
        {
            AudioManager.Instance.PlaySound("SFX_Deflect_6", transform);
        }
        
        public void Play_DefenseDeactivate(Transform transform)
        {
            AudioManager.Instance.PlaySound("SFX_Deflect_1", transform);
        }

        public void Play_Jump(Transform transform)
        {
            AudioManager.Instance.PlaySound("SFX_Jump_1", transform, Random.Range(0.8f, 1.2f));
        }

        public void Play_Boing(Transform transform)
        {
            AudioManager.Instance.PlaySound("SFX_SMB2_Boing", transform);
        }

        public void Play_BouncePad(Transform transform)
        {
            AudioManager.Instance.PlaySound("SFX_Dash_1", transform);
        }

        public void Play_GlassBreak(Transform transform)
        {
            AudioManager.Instance.PlaySound("SFX_Pot_Smash_1", transform);
        }

        public void Play_ImpactGeneral(Transform transform)
        {
            AudioManager.Instance.PlayRandomSound(impactGroupGeneral);
        }

        public void Play_ImpactDeflect(Transform transform)
        {
            AudioManager.Instance.PlayRandomSound(impactGroupDeflect);
        }

    }
}