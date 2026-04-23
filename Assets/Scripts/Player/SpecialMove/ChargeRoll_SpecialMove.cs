using GASHAPWN.Audio;
using System;
using System.Collections;
using UnityEngine;

namespace GASHAPWN
{
    /// <summary>
    /// Concrete "ChargeRoll" Special Move
    /// </summary>
    [System.Serializable]
    public class ChargeRoll_SpecialMove : ISpecialMove
    {
        /// GENERAL VARIABLES ///

        // Name of Special Move
        public string Name => "ChargeRoll";
        // Stamina Cost of Special Move
        public float StaminaCost => 3f;

        public float DamageMultiplier => 2f;

        public Enum GetSubState() => _state;


        /// SPECIALIZED VARIABLES ///

        private ChargeRollState _state;

        public float minForce = 0.1f;
        public float maxForce = 1f;
        public float maxChargeDuration = 2f;
        public float maxHoldDuration = 4f;

        public float spinSpeed = 1000f;
        // Duration of attack boost after charge
        public float attackBoostDuration = 1.5f;

        private Transform playerTransform;
        private Rigidbody _rb;
        private Quaternion chargeRotation;
        private float chargeStartTime;

        // Need to figure out a way for this special move to get access to rotation input per frame
        private Vector2 chargeDirection;


        /// METHODS ///

        public ChargeRoll_SpecialMove() {}

        public ChargeRoll_SpecialMove(float minForce, float maxForce, float maxDuration, float spinSpeed, 
            float attackBoostDuration)
        {
            this.minForce = minForce;
            this.maxForce = maxForce;
            this.maxChargeDuration = maxDuration;
            this.spinSpeed = spinSpeed;
            this.attackBoostDuration = attackBoostDuration;
        }

        public bool CanExecute(SpecialMoveHandler spMoveHandler)
        {
            return spMoveHandler.pController.ControlsEnabled &&
                   !spMoveHandler.IsCharging &&
                   !spMoveHandler.HasCharged &&
                   !spMoveHandler.IsBursting &&
                   !spMoveHandler.IsDefending &&
                   spMoveHandler.pData.currentStamina >= StaminaCost;
        }

        // Can only cancel if charging or holding
        public bool CanCancel(SpecialMoveHandler spMoveHandler) {
            return spMoveHandler.IsCharging || spMoveHandler.HasCharged; 
        }

        public IEnumerator Execute(SpecialMoveHandler spMoveHandler)
        {
            _rb = spMoveHandler.pController.rb;
            playerTransform = _rb.transform;

            // Handle stamina
            spMoveHandler.pData.currentStamina -= StaminaCost;
            spMoveHandler.pData.staminaEvents.OnStaminaChanged?.Invoke(spMoveHandler.pData.currentStamina);

            return ChargeRollCoroutine(spMoveHandler);
        }

        // Cancel in this case handles Charge Roll burst
        public void Cancel(SpecialMoveHandler spMoveHandler)
        {
            spMoveHandler.StartCoroutine(BurstCoroutine(spMoveHandler));
        }

        // BIG ISSUE: Other moves are possible to perform during charge and burst, completely breaking states.


        // Handles what occurs during the burst after a charge roll
        private IEnumerator BurstCoroutine(SpecialMoveHandler spMoveHandler)
        {
            Debug.Log("here in Burst coroutine");
            spMoveHandler.IsCharging = false;
            spMoveHandler.HasCharged = false;
            spMoveHandler.IsBursting = true;
            _state = ChargeRollState.Burst;
            spMoveHandler.Events.OnChargeRoll.Invoke(_state);
            
            // Technically might have to be IsDefending as the appropriate collision handling would then happen

            float chargeDuration = Time.time - chargeStartTime;
            float chargePercent = Mathf.Clamp01(chargeDuration / maxChargeDuration);

            //playerTransform.rotation = chargeRotation;

            float forceMagnitude = Mathf.Lerp(minForce, maxForce, chargePercent);
            _rb.AddForce(playerTransform.forward * forceMagnitude, ForceMode.Impulse);
            spMoveHandler.chargeRollIndicator?.HideIndicator();
            spMoveHandler.pData.ActivateAttackBoost(1.1f, DamageMultiplier);

            // Burst duration scales with charge
            //float burstDuration = Mathf.Lerp(0.1f, 0.6f, chargePercent);
            float burstDuration = 1f;
            float elapsed = 0f;

            while (elapsed < burstDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Exit burst
            Reset(spMoveHandler);
        }

        // Handles what occurs before and during a Charge Roll
        private IEnumerator ChargeRollCoroutine(SpecialMoveHandler spMoveHandler)
        {
            spMoveHandler.IsCharging = true;
            spMoveHandler.pController.ControlsEnabled = false;
            _state = ChargeRollState.Charge;
            spMoveHandler.Events.OnChargeRoll.Invoke(_state);

            Vector3 baseDirection = new Vector3(playerTransform.forward.x, 0, playerTransform.forward.z).normalized;
            chargeRotation = Quaternion.LookRotation(baseDirection);

            bool originalGravity = _rb.useGravity;
            float originalDrag = _rb.linearDamping;
            float originalAngularDrag = _rb.angularDamping;

            #region STOP PHASE
                float stopDuration = 0.3f;
                float elapsed = 0f;
                Vector3 initialVelocity = _rb.linearVelocity;
                Vector3 initialAngularVelocity = _rb.angularVelocity;

                while (elapsed < stopDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / stopDuration;
                    _rb.linearVelocity = Vector3.Lerp(initialVelocity, Vector3.zero, t);
                    _rb.angularVelocity = Vector3.Lerp(initialAngularVelocity, Vector3.zero, t);
                    yield return null;
                }

                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.useGravity = false;
            #endregion
            #region CHARGE PHASE
                
                // Need to rotate the player to the charge direction

                chargeStartTime = Time.time;

                elapsed = 0f;
                while (elapsed <= maxChargeDuration)
                {
                    chargeDirection = new Vector2(spMoveHandler.pController.MovementForward.x, spMoveHandler.pController.MovementForward.z);

                    elapsed += Time.deltaTime;
                    float chargePercent = Mathf.Clamp01((Time.time - chargeStartTime) / maxChargeDuration);

                    //playerTransform.rotation = chargeRotation;

                    spMoveHandler.chargeRollIndicator?.UpdateIndicator(chargePercent, chargeDirection);
                    yield return null;
                }
            #endregion

            #region HOLD PHASE
                spMoveHandler.IsCharging = false;
                spMoveHandler.HasCharged = true;
                _state = ChargeRollState.Hold;
                spMoveHandler.Events.OnChargeRoll.Invoke(_state);

                elapsed = 0f;
                bool timedOut = true;

                while (elapsed < maxHoldDuration)
                {
                    if (spMoveHandler.IsBursting || !spMoveHandler.HasCharged)
                    {
                        timedOut = false;
                        break;
                    }

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (timedOut)
                {
                    Debug.Log("in here");
                    spMoveHandler.ApplyStun(3f);
                    Reset(spMoveHandler);
                }
            #endregion
        }

        // Resets booleans and state
        private void Reset(SpecialMoveHandler spMoveHandler)
        {
            spMoveHandler.pController.ControlsEnabled = true;

            spMoveHandler.IsCharging = false;
            spMoveHandler.HasCharged = false;
            spMoveHandler.IsBursting = false;
            spMoveHandler.chargeRollIndicator?.HideIndicator();

            if (_rb != null)
                _rb.useGravity = true;

            _state = ChargeRollState.None;
            spMoveHandler.Events.OnChargeRoll.Invoke(_state);
        }

        public enum ChargeRollState
        {
            None,
            Charge,
            Hold,
            Burst
        }
    }
}