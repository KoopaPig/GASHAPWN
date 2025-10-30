using GASHAPWN.Audio;
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


        /// SPECIALIZED VARIABLES ///

        public float jumpForce = 30f;


        /// METHODS ///

        public Jump_SpecialMove() { }

        public Jump_SpecialMove(float jumpForce)
        {
            this.jumpForce = jumpForce;
        }

        public bool CanExecute(PlayerData playerData)
        {
            return playerData.controlsEnabled &&
               playerData.isGrounded &&
               playerData.currentStamina >= StaminaCost &&
               !playerData.isCharging;
        }

        public void Execute(PlayerData playerData, MonoBehaviour host)
        {
            Rigidbody rb = playerData.rb;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            playerData.currentStamina -= StaminaCost;
            playerData.OnStaminaChanged?.Invoke(playerData.currentStamina);

            GAME_SFXManager.Instance.Play_Jump(playerData.transform);
        }

        public void Cancel() { }
    }
}