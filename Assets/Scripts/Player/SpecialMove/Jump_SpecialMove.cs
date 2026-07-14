using GASHAPWN.Audio;
using System;
using System.Collections;
using UnityEngine;

namespace GASHAPWN
{
    /// <summary>
    /// Concrete "Jump" Special Move
    /// </summary>
    [System.Serializable]
    public class Jump_SpecialMove : ISpecialMove
    {
        /// GENERAL VARIABLES ///

        // Name of Special Move
        public string Name => "Jump";
        // Stamina Cost of Special Move
        public float StaminaCost => 1f;
        // Damage Multiplier (does not apply for Jump)
        public float DamageMultiplier => 1f;

        // No substates for Jump
        public Enum GetSubState() => null;

        /// SPECIALIZED VARIABLES ///

        public float JumpForce = 0.5f;


        /// METHODS ///

        public Jump_SpecialMove() { }

        public Jump_SpecialMove(float jumpForce)
        {
            JumpForce = jumpForce;
        }

        public bool CanExecute(SpecialMoveHandler spMoveHandler)
        {
            return spMoveHandler.pController.ControlsEnabled &&
               spMoveHandler.pController.IsGrounded &&
               !spMoveHandler.HasJumped && 
               !spMoveHandler.IsCharging &&
               !spMoveHandler.HasCharged &&
               !spMoveHandler.IsBursting;
        }

        public bool CanCancel(SpecialMoveHandler spMoveHandler) { return true; }

        public IEnumerator Execute(SpecialMoveHandler spMoveHandler)
        {
            Rigidbody rb = spMoveHandler.pController.rb;
            Vector3 vel = rb.linearVelocity;

            rb.linearVelocity = new Vector3(vel.x, 0f, vel.z);
            
            rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse); // Apply jump

            // Handle stamina
            spMoveHandler.pData.currentStamina -= StaminaCost;
            spMoveHandler.pData.staminaEvents.OnStaminaChanged?.Invoke(spMoveHandler.pData.currentStamina);

            GAME_SFXManager.Instance.Play_Jump(spMoveHandler.pData.transform);

            // Set flags
            spMoveHandler.HasJumped = true;

            yield return null;
        }

        public ISpecialMove Cancel(SpecialMoveHandler spMoveHandler) 
        {
            Rigidbody rb = spMoveHandler.pController.rb;

            // Only cut jump if still going up
            if (rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    rb.linearVelocity.y * 0.4f,
                    rb.linearVelocity.z
                );
            }
            // if nothing happens after cancel, return null
            return null;
        }
    }
}