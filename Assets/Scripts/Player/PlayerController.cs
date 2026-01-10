using System.Collections;
using GASHAPWN.Audio;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GASHAPWN {
    /// <summary>
    /// Defines methods for player control
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("Distance from center to check for ground")]
        [SerializeField] private float groundCheckDistance = 0.6f; // Slightly more than sphere radius

        [Tooltip("LayerMask for \"Ground\" layer")]
        [SerializeField] private LayerMask groundLayer; // Set this in inspector to your ground layer

        // Reference to playerData
        private PlayerData _pPlayerData;

        // Reference to specialMoveHandler
        private SpecialMoveHandler _pSpecialMoveHandler;

        // Reference to rigidbody
        private Rigidbody _rb;

        // Vector for movement input
        private Vector2 moveInput;


        /// PRIVATE METHODS ///

        private void Awake()
        {
            Physics.gravity = new Vector3(0, -9.81f * 3f, 0);

            _rb = GetComponent<Rigidbody>();
            _pPlayerData = GetComponent<PlayerData>();
            _pSpecialMoveHandler = GetComponent<SpecialMoveHandler>();

            if (_pPlayerData != null)
            {
                _rb.linearDamping = _pPlayerData.drag;
                _rb.angularDamping = _pPlayerData.angularDrag;

                if (_pPlayerData.sphereMaterial != null)
                {
                    var coll = GetComponent<Collider>();
                    coll.material = _pPlayerData.sphereMaterial;
                }
            }
        }

        private void FixedUpdate()
        {
            if (_pPlayerData == null)
                return;

            _pPlayerData.isGrounded = IsGrounded();

            // Play slam sound if isGrounded and hasSlammed
            // Consider moving this elsewhere?
            if (_pPlayerData.isGrounded && _pPlayerData.hasSlammed)
            {
                GAME_SFXManager.Instance.Play_SlamImpact(this.transform);
            }

            if (_pPlayerData.isGrounded && !_pPlayerData.isCharging && !_pPlayerData.isDefending)
            {
                _rb.linearDamping = _pPlayerData.drag;
                _rb.angularDamping = _pPlayerData.angularDrag;
                _pPlayerData.controlsEnabled = true;
                _pPlayerData.hasSlammed = false;
                _pPlayerData.hasCharged = false;
                Vector3 force = new Vector3(moveInput.x, 0f, moveInput.y) * _pPlayerData.moveSpeed;
                _rb.AddForce(force);

                if (moveInput.sqrMagnitude > 0.01f)
                {
                    _pPlayerData.currentStamina += _pPlayerData.staminaRegenRate * Time.fixedDeltaTime;
                    _pPlayerData.currentStamina = Mathf.Clamp(_pPlayerData.currentStamina, 0f, _pPlayerData.maxStamina);
                    _pPlayerData.OnStaminaChanged.Invoke(_pPlayerData.currentStamina);
                }
            }
            else
            {
                _rb.linearDamping = 0f;
                _rb.angularDamping = 0f;
            }

            Vector3 torque = new Vector3(moveInput.y, 0f, -moveInput.x) * _pPlayerData.airTorque;
            _rb.AddTorque(torque);
        }

        // Returns true if grounded
        private bool IsGrounded()
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.red);
            return Physics.Raycast(ray, groundCheckDistance, groundLayer);
        }


        /// PUBLIC METHODS ///

        // Actions for "Movement" input
        public void OnMovement(InputAction.CallbackContext context)
        {
            if (_pPlayerData == null || _pPlayerData.isCharging || _pPlayerData.isDefending)
            {
                return;
            } 
            else
            {
                moveInput = context.ReadValue<Vector2>();
            }
        }

        // Actions for "Jump" input
        public void OnJump(InputAction.CallbackContext context)
        {
            if (_pPlayerData == null || !_pPlayerData.controlsEnabled || !_pPlayerData.isGrounded || _pPlayerData.isCharging)
                return;

            if (context.performed) _pSpecialMoveHandler.TryUseSpecialMove("Jump");
        }

        // Actions for "Slam" input
        public void OnSlam(InputAction.CallbackContext context)
        {
            if (_pPlayerData == null) return;

            if (context.performed) _pSpecialMoveHandler.TryUseSpecialMove("Slam");
        }

        // Actions for "QuickBreak" input
        public void OnQuickBreak(InputAction.CallbackContext context)
        {
            if (_pPlayerData == null) return;

            if (context.performed) _pSpecialMoveHandler.TryUseSpecialMove("QuickBreak");
            if (context.canceled) _pSpecialMoveHandler.TryCancelSpecialMove("QuickBreak");
        }

        // Actions for "RotateChargeDirection" input
        // Might need to edit this given new way I want charge rotation to work
        public void OnRotateChargeDirection(InputAction.CallbackContext context)
        {
            if (_pPlayerData.isCharging)
                _pPlayerData.rotationInput = context.ReadValue<Vector2>();
        }

        // Actions for "ChargeRoll" input
        //public void OnChargeRoll(InputAction.CallbackContext context)
        //{
        //    if (_pPlayerData == null || !_pPlayerData.controlsEnabled || _pPlayerData.isCharging || _pPlayerData.hasCharged)
        //        return;

        //    if (context.started)
        //    {
        //        _pSpecialMoveHandler.TryUseSpecialMove("ChargeRoll");
        //    }
        //    else if (context.canceled && _pPlayerData.isCharging)
        //    {
        //        _pSpecialMoveHandler.TryCancelSpecialMove("ChargeRoll");
        //    }
        //}

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


        /*
        public void OnBurst(InputAction.CallbackContext context)
        {
            if (playerData == null || !playerData.controlsEnabled || playerData.isCharging)
                return;

            if (context.performed)
            {
                if (playerData.currentStamina >= 6f)
                {
                    StartCoroutine(BurstCoroutine());
                    playerData.currentStamina -= 6f;
                    playerData.OnStaminaChanged.Invoke(playerData.currentStamina);
                }
                else
                {
                    playerData.OnLowStamina.Invoke(playerData.currentStamina);
                    Debug.Log("Not enough stamina for burst");
                }
            }
        }
        */

        /*
        private IEnumerator BurstCoroutine()
        {
            playerData.controlsEnabled = false;

            // Activate invincibility for burst
            playerData.ActivateBurstInvincibility();

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.AddForce(Vector3.up * 20f, ForceMode.Impulse);

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
            float rotationDuration = 0.5f;
            float elapsed = 0f;
            while (elapsed < rotationDuration)
            {
                elapsed += Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsed / rotationDuration);
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            float shockwaveRadius = 20f;
            float knockbackForce = 80f;
            Collider[] colliders = Physics.OverlapSphere(transform.position, shockwaveRadius);
            foreach (Collider col in colliders)
            {
                if ((col.CompareTag("Player1") || col.CompareTag("Player2")) && col.gameObject != gameObject)
                {
                    Rigidbody otherRb = col.GetComponent<Rigidbody>();
                    if (otherRb != null)
                    {
                        Vector3 direction = (otherRb.position - transform.position).normalized;
                        otherRb.AddForce(direction * knockbackForce, ForceMode.Impulse);
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
            playerData.controlsEnabled = true;
        }
        */
    }
}