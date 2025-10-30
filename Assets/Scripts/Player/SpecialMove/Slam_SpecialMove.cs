using GASHAPWN.Audio;
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
        public float DamageMultiplier => 1.5f;

        /// SPECIALIZED VARIABLES ///

        public float slamForce = 80f;
        public float slamDelay = 0.4f;


        /// METHODS ///
        
        public Slam_SpecialMove() { }

        public Slam_SpecialMove(float slamForce, float slamDelay)
        {
            this.slamForce = slamForce;
            this.slamDelay = slamDelay;
        }

        public bool CanExecute(PlayerData playerData)
        {
            return playerData.controlsEnabled &&
                   !playerData.isCharging &&
                   !playerData.isGrounded &&
                   !playerData.hasSlammed &&
                   playerData.currentStamina >= StaminaCost;
        }

        public void Execute(PlayerData playerData, MonoBehaviour host)
        {
            playerData.currentStamina -= StaminaCost;
            playerData.OnStaminaChanged?.Invoke(playerData.currentStamina);

            playerData.ActivateAttackBoost(1.0f, DamageMultiplier);

            // Delegate coroutine to Player Data (Monobehaviour)
            playerData.StartCoroutine(playerData.SlamCoroutine(slamForce, slamDelay));
        }

        public void Cancel() { }
    }
}