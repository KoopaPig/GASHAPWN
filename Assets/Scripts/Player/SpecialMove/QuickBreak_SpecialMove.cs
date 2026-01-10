using GASHAPWN.Audio;
using System.Collections;
using UnityEngine;

namespace GASHAPWN
{
    /// <summary>
    /// Concrete "QuickBreak" Special Move
    /// </summary>
    [System.Serializable]
    public class QuickBreak_SpecialMove : ISpecialMove
    {
        /// GENERAL VARIABLES ///

        // Name of Special Move
        public string Name => "QuickBreak";
        // Stamina Cost of Special Move
        public float StaminaCost => 2f;
        // Damage Multipler (does not apply for QuickBreak)
        public float DamageMultiplier => 1f;


        /// SPECIALIZED VARIABLES ///
        // Duration of break
        public float quickBreakDuration = 0.2f;
        // Duration of defense boost after quick break
        public float quickBreakDefenseDuration = 1.5f;


        /// METHODS ///

        public QuickBreak_SpecialMove() { }

        public QuickBreak_SpecialMove(float breakDuration, float defenseDuration)
        {
            this.quickBreakDuration = breakDuration;
            this.quickBreakDefenseDuration = defenseDuration;
        }

        public bool CanExecute(PlayerData playerData)
        {
            return playerData.controlsEnabled &&
                   !playerData.isCharging &&
                   playerData.isGrounded &&
                   !playerData.isDefending &&
                   playerData.currentStamina >= StaminaCost;
        }

        public bool CanCancel(PlayerData playerData)
        {
            return playerData.controlsEnabled &&
                    playerData.isDefending;
        }

        public IEnumerator Execute(PlayerData playerData, MonoBehaviour host)
        {
            playerData.currentStamina -= StaminaCost;
            playerData.OnStaminaChanged?.Invoke(playerData.currentStamina);

            return playerData.QuickBreakCoroutine(quickBreakDuration, quickBreakDefenseDuration);
        }

        public void Cancel(PlayerData playerData, MonoBehaviour host) {
            playerData.QuickBreak_Cancel();
        }
    }
}