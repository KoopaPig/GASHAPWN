using GASHAPWN.Audio;
using System;
using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GASHAPWN
{
    /// <summary>
    /// Concrete "QuickBrake" Special Move
    /// </summary>
    [System.Serializable]
    public class QuickBrake_SpecialMove : ISpecialMove
    {
        /// GENERAL VARIABLES ///

        // Name of Special Move
        public string Name => "QuickBrake";
        // Stamina Cost of Special Move
        public float StaminaCost => 2f;
        // Damage Multipler (does not apply for QuickBrake)
        public float DamageMultiplier => 1f;

        // No substates for QuickBrake
        public Enum GetSubState() => null;

        /// SPECIALIZED VARIABLES ///
        // Duration of break
        public float quickBrakeDuration = 0.2f;

        // Maximum duration of defensive shield after quick break
        public float maxShieldDuration = 3f;

        // Recharge rate of shield when not defending
        // This is multiplied by deltaTime, so a value below 1 means a recharge rate slower than realtime.
        // 0.5 means 4 seconds to fully recharge from 0, for instance
        public float shieldRechargeRate = 0.35f;

        /// METHODS ///

        public QuickBrake_SpecialMove() { }

        public QuickBrake_SpecialMove(float breakDuration, float maxShieldDuration_)
        {
            this.quickBrakeDuration = breakDuration;
            this.maxShieldDuration = maxShieldDuration_;
        }

        public bool CanExecute(SpecialMoveHandler spMoveHandler)
        {
            return spMoveHandler.pController.ControlsEnabled &&
                   !spMoveHandler.IsCharging &&
                   !spMoveHandler.HasCharged &&
                   !spMoveHandler.IsBursting &&
                   spMoveHandler.pController.IsGrounded &&
                   !spMoveHandler.IsDefending &&
                   spMoveHandler.pData.currentStamina >= StaminaCost;
        }

        public bool CanCancel(SpecialMoveHandler spMoveHandler)
        {
            return spMoveHandler.pController.ControlsEnabled &&
                    spMoveHandler.IsDefending;
        }

        public IEnumerator Execute(SpecialMoveHandler spMoveHandler)
        {
            // Handle stamina
            spMoveHandler.pData.currentStamina -= StaminaCost;
            spMoveHandler.pData.staminaEvents.OnStaminaChanged?.Invoke(spMoveHandler.pData.currentStamina);

            return QuickBrakeCoroutine(spMoveHandler);
        }

        public void Cancel(SpecialMoveHandler spMoveHandler) {
            spMoveHandler.pData.IsInvincible = false;
            spMoveHandler.IsDefending = false;
            spMoveHandler.Events.OnDefenseDeactivated?.Invoke();
        }

        private IEnumerator QuickBrakeCoroutine(SpecialMoveHandler spMoveHandler)
        {
            // Make player temporarily invincible during quick break
            spMoveHandler.pData.IsInvincible = true;

            Rigidbody _rb = spMoveHandler.pController.rb;
            Vector3 initialVelocity = _rb.linearVelocity;
            Vector3 initialAngularVelocity = _rb.angularVelocity;

            Quaternion initialRotation = _rb.transform.rotation;
            Quaternion targetRotation = Quaternion.FromToRotation(-_rb.transform.up, Vector3.up) * _rb.transform.rotation;

            // Activate defense
            spMoveHandler.IsDefending = true;
            spMoveHandler.Events.OnDefenseActivated.Invoke();

            // Break
            float elapsed = 0f;
            while (elapsed < quickBrakeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / quickBrakeDuration);

                _rb.linearVelocity = Vector3.Lerp(initialVelocity, Vector3.zero, t);
                _rb.angularVelocity = Vector3.Lerp(initialAngularVelocity, Vector3.zero, t);

                _rb.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

                yield return null;
            }

            elapsed = 0f;
            while (elapsed < maxShieldDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // cancelling is managed in SpecialMoveHandler
        }
    }
}