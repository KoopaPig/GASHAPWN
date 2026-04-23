using GASHAPWN;
using GASHAPWN.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Stores Dictionary of Special Moves and executes special moves (needs reference to PlayerData)
/// </summary>

[RequireComponent(typeof(PlayerData))]
public class SpecialMoveHandler : MonoBehaviour
{
    // Currently active special move
    public ISpecialMove activeSpecialMove { private set; get; } = null;

    // Dictionary of Special Moves
    public Dictionary<string, ISpecialMove> moveSet;

    // Reference to Player Data
    public PlayerData pData { get; private set; }

    // Reference to Player Controller
    public PlayerController pController { get; private set; }

    // Single corotoutine ensures only one special move can execute at a time
    private Coroutine _specialMoveCoroutine = null;

    #region LOCAL DEFENSE VARIABLES
        public float ShieldTimer { get; private set; } = 0f;
        public float MaxShieldDuration { get; private set; }
        private float _shieldRechargeRate;
    #endregion

    #region SPECIAL MOVE FLAGS
        [HideInInspector] public bool HasJumped = false;
        [HideInInspector] public bool HasSlammed = false;
        [HideInInspector] public bool IsDefending = false;
        [HideInInspector] public bool IsCharging = false;
        [HideInInspector] public bool HasCharged = false;
        [HideInInspector] public bool IsBursting = false;
        [HideInInspector] public bool IsStunned = false;
        private bool _isCooldown = false;
        private bool _wasStunned = false;
    #endregion

    [Tooltip("Cooldown timer between special moves so you cannot spam them")]
    [Range(0.1f, 1)] public float SpecialMoveCooldown = 0.25f;

    [Tooltip("Reference to charge roll indicator effect")]
    public ChargeRollIndicator chargeRollIndicator;
        private float _specialMoveCooldownTimer = 0f;

        private float stunDuration;
        private float stunTimer;

    public SpecialMoveEvents Events;

    private void Awake()
    {
        // Register Special Moves here
        // If attributes need to be adjusted, construct with new attributes
        moveSet = new Dictionary<string, ISpecialMove>
        {
            { "Jump", new Jump_SpecialMove() },
            { "Slam", new Slam_SpecialMove() },
            { "QuickBrake", new QuickBrake_SpecialMove() },
            { "ChargeRoll", new ChargeRoll_SpecialMove() }
        };

        // Set local _maxShieldDuration variable based on data from QuickBrake_SpecialMove
        if (moveSet.TryGetValue("QuickBrake", out ISpecialMove move))
        {
            QuickBrake_SpecialMove quickBrakeMove = move as QuickBrake_SpecialMove;
            MaxShieldDuration = quickBrakeMove.maxShieldDuration;
            _shieldRechargeRate = quickBrakeMove.shieldRechargeRate;
            ShieldTimer = MaxShieldDuration;
        }
    }


    private void Update()
    {
        // Handle defense shield duration and recharge
        if (IsDefending)
        {
            if (ShieldTimer > 0)
            {
                ShieldTimer -= Time.deltaTime;
                if (ShieldTimer <= 0)
                {
                    TryCancelSpecialMove("QuickBrake");
                    ApplyStun(3f);
                }
            }
        }
        else
        {
            if (ShieldTimer <= MaxShieldDuration)
                ShieldTimer += _shieldRechargeRate * Time.deltaTime;
            else ShieldTimer = MaxShieldDuration;
        }

        // Handle special move cooldown
        _isCooldown = (_specialMoveCooldownTimer < SpecialMoveCooldown);

        if (_specialMoveCooldownTimer <= SpecialMoveCooldown)
        {
            _specialMoveCooldownTimer += Time.deltaTime;
        }

        // Handle stunned state
        if (IsStunned)
        {
            // Fire event only on enter
            if (!_wasStunned)
            {
                pController.ControlsEnabled = false;
                Events.OnStunned.Invoke(stunDuration);
                _wasStunned = true;
            }

            stunTimer += Time.deltaTime;

            if (stunTimer >= stunDuration)
            {
                IsStunned = false;
                stunTimer = 0;
            }
        }
        else
        {
            // Fire event only on exit
            if (_wasStunned)
            {
                pController.ControlsEnabled = true;
                _wasStunned = false;
            }
        }
    }

    public void Initialize(PlayerController playerController)
    {
        pData = GetComponent<PlayerData>();
        pController = playerController;
        _specialMoveCooldownTimer = SpecialMoveCooldown;
    }

    /// <summary>
    /// Given string corresponding to special move, try to execute it
    /// </summary>
    public void TryUseSpecialMove(string moveName)
    {
        if (moveSet.TryGetValue(moveName, out ISpecialMove move))
        {
            // If in cooldown, cannot execute
            if (_isCooldown)
            {
                Debug.Log($"Cannot execute {move.Name}. Special move cooldown has not finished.");
                return;
            }
            // If execution conditions are met, execute
            if (move.CanExecute(this))
            {
                if (activeSpecialMove != move && activeSpecialMove != null) activeSpecialMove.Cancel(this);
                if (_specialMoveCoroutine != null) StopCoroutine(_specialMoveCoroutine); 

                _specialMoveCoroutine = StartCoroutine(move.Execute(this));
                activeSpecialMove = move;
                _specialMoveCooldownTimer = 0f;
                Debug.Log($"Executed: {move.Name}");
            }
            // If cannot execute, stamina must be low
            else
            {
                if (pData.currentStamina < move.StaminaCost)
                {
                    pData.staminaEvents.OnLowStamina.Invoke(pData.currentStamina);
                    Debug.Log($"Not enough stamina to execute move: {move.Name}");
                }
                else
                {
                    Debug.Log($"Cannot execute move: {move.Name} (other conditions failed)");
                }
            }
        }
        else
        {
            Debug.LogWarning($"Move not found: {moveName}");
        }
    }

    /// <summary>
    /// Given string corresponding to special move, try to call its Cancel method
    /// </summary>
    public void TryCancelSpecialMove(string moveName)
    {
        if (moveSet.TryGetValue(moveName, out ISpecialMove move))
        {
            if (move.CanCancel(this))
            {
                move.Cancel(this);
                activeSpecialMove = null;
            } 
        }
    }

    public void ApplyStun(float duration)
    {
        stunDuration = duration;
        stunTimer = 0;
        IsStunned = true;
    }


    [System.Serializable]
    public class SpecialMoveEvents
    {
        public UnityEvent OnDefenseActivated = new UnityEvent();
        public UnityEvent OnDefenseDeactivated = new UnityEvent();
        public UnityEvent OnAttackBonusActivated = new UnityEvent();
        public UnityEvent OnAttackBonusDeactivated = new UnityEvent();
        public UnityEvent<ChargeRoll_SpecialMove.ChargeRollState> OnChargeRoll = new UnityEvent<ChargeRoll_SpecialMove.ChargeRollState>();
        public UnityEvent OnSlam = new UnityEvent();
        public UnityEvent<float> OnStunned = new UnityEvent<float>();
    }
}