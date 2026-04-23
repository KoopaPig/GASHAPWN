using GASHAPWN.Audio;
using System;
using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;

namespace GASHAPWN
{
    /// <summary>
    /// Concrete "Slam" Special Move
    /// </summary>
    [System.Serializable]
    public class Slam_SpecialMove : ISpecialMove
    {
        /// GENERAL VARIABLES ///

        // Name of Special Move
        public string Name => "Slam";
        // Stamina Cost of Special Move
        public float StaminaCost => 1f;

        // Damage Multiplier
        public float DamageMultiplier => 1f;

        // No substates for Slam
        public Enum GetSubState() => null;

        /// SPECIALIZED VARIABLES ///

        public float slamForce = 0.45f;
        public float slamDelay = 0.45f;


        /// METHODS ///
        
        public Slam_SpecialMove() { }

        public Slam_SpecialMove(float slamForce, float slamDelay)
        {
            this.slamForce = slamForce;
            this.slamDelay = slamDelay;
        }

        public bool CanExecute(SpecialMoveHandler spMoveHandler)
        {
            return spMoveHandler.pController.ControlsEnabled &&
                   !spMoveHandler.IsCharging &&
                   !spMoveHandler.HasCharged &&
                   !spMoveHandler.IsBursting &&
                   !spMoveHandler.pController.IsGrounded &&
                   !spMoveHandler.HasSlammed &&
                   spMoveHandler.pData.currentStamina >= StaminaCost;
        }

        public bool CanCancel(SpecialMoveHandler specialMoveHandler) { return true; }

        public IEnumerator Execute(SpecialMoveHandler spMoveHandler)
        {
            // Handle stamina
            spMoveHandler.pData.currentStamina -= StaminaCost;
            spMoveHandler.pData.staminaEvents.OnStaminaChanged?.Invoke(spMoveHandler.pData.currentStamina);

            // Activate attack boost
            spMoveHandler.pData.ActivateAttackBoost(1.1f, DamageMultiplier);

            // Delegate coroutine to Special Move Handler
            return SlamCoroutine(spMoveHandler);
        }

        // Can't cancel slam
        public void Cancel(SpecialMoveHandler spMoveHandler) { }

        #region SPECIAL MOVE COROUTINES
        public IEnumerator SlamCoroutine(SpecialMoveHandler spMoveHandler)
        {
            if (spMoveHandler.HasSlammed) yield break;

            spMoveHandler.pController.ControlsEnabled = false;
            spMoveHandler.Events.OnSlam.Invoke();
            Rigidbody _rb = spMoveHandler.pController.rb;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.useGravity = false;

            spMoveHandler.HasSlammed = true;

            // Rotate player so metal faces down
            Quaternion initialRotation = _rb.transform.rotation;
            Quaternion targetRotation = Quaternion.FromToRotation(-_rb.transform.up, Vector3.down) * _rb.transform.rotation;

            float elapsed = 0f;
            while (elapsed < slamDelay)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / slamDelay);
                _rb.transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);
                yield return null;
            }
            GAME_SFXManager.Instance.Play_Drop(spMoveHandler.transform);
            _rb.useGravity = true;
            _rb.AddForce(Vector3.down * slamForce, ForceMode.Impulse);
        }
        #endregion
    }
}