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

        public float minForce = 0.02f;
        public float maxForce = 0.5f;
        public float maxChargeDuration = 2f;
        public float maxHoldDuration = 4f;
        public float maxBurstDuration = 1.1f;



        private Rigidbody _rb;
        private Quaternion chargeRotation;
        private float _chargeStartTime;

        // Need to figure out a way for this special move to get access to rotation input per frame
        private Vector2 _chargeDirection;


        /// METHODS ///

        public ChargeRoll_SpecialMove() {}

        public ChargeRoll_SpecialMove(float minForce, float maxForce, float maxChargeDuration, float maxBurstDuration)
        {
            this.minForce = minForce;
            this.maxForce = maxForce;
            this.maxChargeDuration = maxChargeDuration;
            this.maxBurstDuration = maxChargeDuration;
        }

        public bool CanExecute(SpecialMoveHandler spMoveHandler)
        {
            return spMoveHandler.pController.ControlsEnabled &&
                   !spMoveHandler.IsCharging &&
                   !spMoveHandler.HasCharged &&
                   !spMoveHandler.IsBursting &&
                   !spMoveHandler.IsDefending;
        }

        // Can only cancel if charging or holding
        public bool CanCancel(SpecialMoveHandler spMoveHandler) {
            return spMoveHandler.IsCharging || spMoveHandler.HasCharged; 
        }

        public IEnumerator Execute(SpecialMoveHandler spMoveHandler)
        {
            if (_rb == null) _rb = spMoveHandler.pController.rb;

            // Handle stamina
            spMoveHandler.pData.currentStamina -= StaminaCost;
            spMoveHandler.pData.staminaEvents.OnStaminaChanged?.Invoke(spMoveHandler.pData.currentStamina);

            return ChargeRollCoroutine(spMoveHandler);
        }

        // Cancel in this case handles Charge Roll burst if charging
        public ISpecialMove Cancel(SpecialMoveHandler spMoveHandler)
        {
            spMoveHandler.TrySpecialMoveSubCoroutine(this, BurstCoroutine(spMoveHandler));
            return this;
        }

        // Handles what occurs during the burst after a charge roll
        private IEnumerator BurstCoroutine(SpecialMoveHandler spMoveHandler)
        {
            if (_chargeStartTime > 0f)
            {
                spMoveHandler.IsCharging = false;
                spMoveHandler.HasCharged = false;
                spMoveHandler.IsBursting = true;
                _state = ChargeRollState.Burst;
                spMoveHandler.Events.OnChargeRoll.Invoke(_state);

                float chargeDuration = Time.time - _chargeStartTime;
                float chargePercent = Mathf.Clamp01(chargeDuration / maxChargeDuration);

                // Apply force
                float forceMagnitude = Mathf.Lerp(minForce, maxForce, chargePercent);
                Vector3 dir = new Vector3(_chargeDirection.x, 0f, _chargeDirection.y).normalized;
                _rb.AddForce(dir * forceMagnitude, ForceMode.Impulse);

                // Burst duration and attack boost scales with charge
                float burstDuration = Mathf.Lerp(0.3f, maxBurstDuration, chargePercent);

                spMoveHandler.pData.ActivateAttackBoost(burstDuration, DamageMultiplier);

                spMoveHandler.chargeRollIndicator?.HideIndicator();

                float elapsed = 0f;

                while (elapsed < burstDuration)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
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

            Vector3 baseDirection = new Vector3(_rb.transform.forward.x, 0, _rb.transform.forward.z).normalized;
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

                _chargeStartTime = Time.time;

                elapsed = 0f;
                while (elapsed <= maxChargeDuration)
                {
                    _chargeDirection = new Vector2(spMoveHandler.pController.MovementForward.x, spMoveHandler.pController.MovementForward.z);

                    elapsed += Time.deltaTime;
                    float chargePercent = Mathf.Clamp01((Time.time - _chargeStartTime) / maxChargeDuration);

                    spMoveHandler.chargeRollIndicator?.UpdateIndicator(chargePercent, _chargeDirection);
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
            _chargeStartTime = 0f;
            spMoveHandler.chargeRollIndicator?.HideIndicator();

            if (_rb != null)
                _rb.useGravity = true;

            _state = ChargeRollState.None;
            spMoveHandler.Events.OnChargeRoll.Invoke(_state);

            spMoveHandler.activeSpecialMove = null;
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