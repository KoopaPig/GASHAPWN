using GASHAPWN.Audio;
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


        /// SPECIALIZED VARIABLES ///

        public float minForce = 15f;
        public float maxForce = 45f;
        public float maxDuration = 2f;
        public float spinSpeed = 1000f;
        // Duration of attack boost after charge
        public float attackBoostDuration = 2f;


        private Coroutine activeCoroutine;
        private MonoBehaviour hostMono;
        private PlayerData playerData;
        private Transform playerTransform;
        private Rigidbody rb;
        private ChargeRollIndicator chargeIndicator;
        private Quaternion chargeRotation;
        private float chargeStartTime;

        private Vector2 rotationInput => playerData.rotationInput; // Assuming this is exposed


        /// METHODS ///

        public ChargeRoll_SpecialMove() {}

        public ChargeRoll_SpecialMove(float minForce, float maxForce, float maxDuration, float spinSpeed, 
            float attackBoostDuration)
        {
            this.minForce = minForce;
            this.maxForce = maxForce;
            this.maxDuration = maxDuration;
            this.spinSpeed = spinSpeed;
            this.attackBoostDuration = attackBoostDuration;
        }

        public bool CanExecute(PlayerData playerData)
        {
            return playerData.controlsEnabled &&
                   !playerData.isCharging &&
                   !playerData.hasCharged &&
                   playerData.currentStamina >= StaminaCost;
        }

        public void Execute(PlayerData data, MonoBehaviour host)
        {
            playerData = data;
            hostMono = host;
            rb = playerData.rb;
            playerTransform = playerData.transform;
            chargeIndicator = playerData.chargeRollIndicator;

            playerData.currentStamina -= StaminaCost;
            playerData.OnStaminaChanged?.Invoke(playerData.currentStamina);

            activeCoroutine = host.StartCoroutine(ChargeRollCoroutine());
        }

        // Cancel in this case handles Charge Roll release
        public void Cancel()
        {
            if (!playerData.isCharging) return;
            playerData.isCharging = false;
            playerData.OnChargeRoll.Invoke(false);

            float chargeDuration = Time.time - chargeStartTime;
            float chargePercent = Mathf.Clamp01(chargeDuration / maxDuration);

            playerTransform.rotation = chargeRotation;

            float forceMagnitude = Mathf.Lerp(minForce, maxForce, chargePercent);
            rb.AddForce(playerTransform.forward * forceMagnitude, ForceMode.Impulse);
        }

        // Handles what occurs before and during a Charge Roll
        private IEnumerator ChargeRollCoroutine()
        {
            playerData.isCharging = true;
            playerData.controlsEnabled = false;
            playerData.OnChargeRoll.Invoke(true);

            Vector3 baseDirection = new Vector3(playerTransform.forward.x, 0, playerTransform.forward.z).normalized;
            chargeRotation = Quaternion.LookRotation(baseDirection);

            bool originalGravity = rb.useGravity;
            float originalDrag = rb.linearDamping;
            float originalAngularDrag = rb.angularDamping;

            // Ease into stop
            float stopDuration = 0.3f;
            float elapsed = 0f;
            Vector3 initialVelocity = rb.linearVelocity;
            Vector3 initialAngularVelocity = rb.angularVelocity;

            while (elapsed < stopDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / stopDuration;
                rb.linearVelocity = Vector3.Lerp(initialVelocity, Vector3.zero, t);
                rb.angularVelocity = Vector3.Lerp(initialAngularVelocity, Vector3.zero, t);
                yield return null;
            }

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.linearDamping = 5f;
            rb.angularDamping = 5f;

            chargeStartTime = Time.time;

            while (playerData.isCharging)
            {
                float chargeTime = Time.time - chargeStartTime;
                float chargePercent = Mathf.Clamp01(chargeTime / maxDuration);

                float yRotation = rotationInput.x * 100f * Time.deltaTime;
                float xzRotation = rotationInput.y * 100f * Time.deltaTime;
                chargeRotation *= Quaternion.Euler(xzRotation, yRotation, 0f);

                Vector3 targetDirection = chargeRotation * Vector3.forward;
                Vector3 torqueDirection = Vector3.Cross(playerTransform.forward, targetDirection);
                rb.AddTorque(torqueDirection * spinSpeed * chargePercent);

                chargeIndicator?.UpdateIndicator(chargePercent, targetDirection);
                yield return null;
            }

            rb.useGravity = originalGravity;
            rb.linearDamping = originalDrag;
            rb.angularDamping = originalAngularDrag;
            chargeIndicator?.HideIndicator();

            playerData.controlsEnabled = true;
            playerData.hasCharged = true;

            float boost = Mathf.Lerp(1.2f, 2.0f, Mathf.Clamp01((Time.time - chargeStartTime) / maxDuration));
            playerData.ActivateAttackBoost(attackBoostDuration, boost);
        }
    }
}