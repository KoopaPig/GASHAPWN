using GASHAPWN.Audio;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GASHAPWN {
    /// <summary>
    /// Defines methods for player control
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerData))]
    [RequireComponent(typeof(PlayerCollisionHandler))]
    [RequireComponent(typeof(SpecialMoveHandler))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
            public float MoveSpeed = 0.27f;

            [Tooltip("Distance from center to check for ground.\nShould be slightly more than sphere radius")]
            [SerializeField] private float groundCheckDistance = 0.05f;

        [Header("Physics Settings")]
            [Range(0.2f, 5f)] public float Drag = 1.75f;
            [Range(0.2f, 5f)] public float AngularDrag = 0.2f;
            [Range(0f, 5f)] public float BrakingForce = 1f;

            public PhysicsMaterial CapsulePhysicsMaterial;

            [Range(0, 1)] public float upGravityMultiplier = 1f;
            [Range(0, 1)] public float fallGravityMultiplier = 0.5f;

        [Header("Air Control Settings")]
            [Range(0.2f, 5f)] public float AirDrag = 2f;

        // Reference to playerData
        private PlayerData _pData;

        // Reference to specialMoveHandler
        private SpecialMoveHandler _pSpecialMoveHandler;

        // Reference to PlayerCollisionHandler
        private PlayerCollisionHandler _pCollisionHandler;

        // Public getter for main player rigidbody
        public Rigidbody rb { get; private set; }

        // Public getter for main player collider
        public SphereCollider sphereCollider { get; private set; }

        // Vector for movement input
        public Vector2 MoveInput { get; private set; }

        // "Forward" direction of the player, changes based on move input
        public Vector3 MovementForward { get; private set; } = Vector3.forward;

        #region PLAYER CONTROLLER FLAGS
        [HideInInspector] public bool IsGrounded { get; private set; } = false;
            [HideInInspector] public bool ControlsEnabled = false;
        #endregion


        /// PRIVATE METHODS ///

        private void OnEnable()
        {
            Initialize();
        }

        private void FixedUpdate()
        {
            if (_pData == null)
                return;

            IsGrounded = CheckGrounded();

            #region CAPTURE VARIABLES
            bool isCharging = _pSpecialMoveHandler.IsCharging;
            bool hasCharged = _pSpecialMoveHandler.HasCharged;
            bool isDefending = _pSpecialMoveHandler.IsDefending;
            bool isStunned = _pSpecialMoveHandler.IsStunned;
            bool hasSlammed = _pSpecialMoveHandler.HasSlammed;

            bool canMoveGrounded = IsGrounded && !isCharging && !hasCharged && !isDefending && !isStunned;
            bool isAirborne = !IsGrounded && !hasSlammed;
            #endregion

            // Update MovementForward
            if (MoveInput.sqrMagnitude > 0.01f)
                MovementForward = new Vector3(MoveInput.x, 0f, MoveInput.y).normalized;

            // Play slam sound if isGrounded and hasSlammed
            if (IsGrounded && hasSlammed)
                GAME_SFXManager.Instance.Play_SlamImpact(this.transform);

            if (canMoveGrounded)
            {
                HandleGroundedMovement();
                return;
            }

            if (isAirborne)
            {
                HandleAirborneMovement();
                return;
            }

            if (!isCharging)
                MoveInput = Vector2.zero;      
        }

        // Returns true if grounded
        private bool CheckGrounded()
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.red);
            return Physics.Raycast(ray, groundCheckDistance, LayerMask.GetMask("Ground"));
        }

        // Apply ground movement, handle stamina, handle braking
        private void HandleGroundedMovement()
        {
            rb.linearDamping = Drag;
            rb.angularDamping = AngularDrag;
            ControlsEnabled = true;

            _pSpecialMoveHandler.HasSlammed = false;
            _pSpecialMoveHandler.HasCharged = false;
            _pSpecialMoveHandler.HasJumped = false;

            if (MoveInput.sqrMagnitude > 0.01f)
            {
                // Regen stamina while rolling on ground
                RegenStamina();

                // Add movement forces
                Vector3 force = new Vector3(MoveInput.x, 0f, MoveInput.y) * MoveSpeed;
                rb.AddForce(force);
            }
            // Apply braking forces
            else
            {
                Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(-horizontalVel * BrakingForce, ForceMode.Acceleration);
            }
        }

        // Apply airborne movement + gravity mods
        private void HandleAirborneMovement()
        {
            rb.linearDamping = AirDrag;
            rb.angularDamping = 0f;

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Physics.gravity * (upGravityMultiplier - 1f), ForceMode.Acceleration);
            else
                rb.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
        }

        // Handle stamina regeneration
        private void RegenStamina()
        {
            if (!IsGrounded) return;
            _pData.currentStamina += _pData.staminaRegenRate * Time.fixedDeltaTime;
            _pData.currentStamina = Mathf.Clamp(_pData.currentStamina, 0f, _pData.maxStamina);
            _pData.staminaEvents.OnStaminaChanged.Invoke(_pData.currentStamina);
        }


        /// PUBLIC METHODS ///

        public void Initialize()
        {
            ControlsEnabled = true;

            rb = GetComponent<Rigidbody>();
            sphereCollider = GetComponent<SphereCollider>();
            _pData = GetComponent<PlayerData>();
            _pSpecialMoveHandler = GetComponent<SpecialMoveHandler>();
            _pCollisionHandler = GetComponent<PlayerCollisionHandler>();

            if (CapsulePhysicsMaterial != null)
            {
                var coll = GetComponent<Collider>();
                coll.material = CapsulePhysicsMaterial;
            }

            _pData.Initialize();
            _pSpecialMoveHandler.Initialize(this);
            _pCollisionHandler.Initialize(this);
        }

        #region INPUT MANAGEMENT

        // Actions for "Movement" input
        public void OnMovement(InputAction.CallbackContext context)
        {
            if (_pData == null || _pSpecialMoveHandler == null) return;
            else MoveInput = context.ReadValue<Vector2>();
        }

        // Actions for "Jump" input
        public void OnJump(InputAction.CallbackContext context)
        {
            if (_pData == null || _pSpecialMoveHandler == null) return;

            // Getting the context does not matter, because it does not get the changed context
            if (context.performed) _pSpecialMoveHandler.TryUseSpecialMove("Jump");
            if (context.canceled) _pSpecialMoveHandler.TryCancelSpecialMove("Jump");
        }

        // Actions for "Slam" input
        public void OnSlam(InputAction.CallbackContext context)
        {
            if (_pData == null || _pSpecialMoveHandler == null) return;

            if (context.performed) _pSpecialMoveHandler.TryUseSpecialMove("Slam");
        }

        // Actions for "QuickBrake" input
        public void OnQuickBrake(InputAction.CallbackContext context)
        {
            if (_pData == null || _pSpecialMoveHandler == null) return;

            if (context.performed) _pSpecialMoveHandler.TryUseSpecialMove("QuickBrake");
            else if (context.canceled) _pSpecialMoveHandler.TryCancelSpecialMove("QuickBrake");
        }

        // Actions for "ChargeRoll" input
        public void OnChargeRoll(InputAction.CallbackContext context)
        {
            if (_pData == null || _pSpecialMoveHandler == null)
                return;

            if (context.started) _pSpecialMoveHandler.TryUseSpecialMove("ChargeRoll");
            else if (context.canceled) _pSpecialMoveHandler.TryCancelSpecialMove("ChargeRoll");
        }

        // Actions for "Pause" input
        public void OnPause(InputAction.CallbackContext context)
        {
            if (BattleManager.Instance != null)
                BattleManager.Instance.PauseGame(GetComponent<PlayerInput>());
        }

        // Actions for "Unpause" input
        public void OnUnpause(InputAction.CallbackContext context)
        {
            if (BattleManager.Instance != null)
                BattleManager.Instance.UnpauseGame();
        }

        #endregion
    }
}