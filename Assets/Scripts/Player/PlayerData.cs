using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GASHAPWN
{
    /// <summary>
    /// Handles data relating to player actions and routes events
    /// </summary>
    public class PlayerData : MonoBehaviour
    {
        [Header("Health & Stamina")]
            public int maxHealth = 5;
            public int currentHealth { get; set; }
            public float maxStamina = 6f;
            public float currentStamina { get; set; }
            public float staminaRegenRate = 0.5f;

            public const int BaseDamage = 1;

        [Header("I-Frame & Defense Settings")]
            public bool IsInvincible { get; set; } = false;
            public float invincibilityDuration = 1.0f; // Default: 1 second of i-frames

            private float damageMultiplier = 1.0f; // For offensive moves
            private float attackBonusTimer = 0f; // Timer for tracking attack bonus

        [Header("Events")]
            public HealthEvents healthEvents;
            public StaminaEvents staminaEvents;

        #region PLAYER STATE FLAGS
            [HideInInspector] public bool IsDead = false;
        #endregion

        private void Update()
        {
            // Update attack timer
            if (attackBonusTimer > 0)
            {
                attackBonusTimer -= Time.deltaTime;
                if (attackBonusTimer <= 0)
                {
                    // Attack bonus expired
                    damageMultiplier = 1.0f;
                }
            }
        }

        // Calculate damage based on speed and offensive abilities
        public int CalculateDamageAmount(float relativeSpeed, bool isOffensiveAbility, PlayerCollisionHandler collisionHandler)
        {
            // Deal damage based on special move's damage multipler
            if (isOffensiveAbility && attackBonusTimer > 0)
                return Mathf.RoundToInt(BaseDamage * damageMultiplier);
            return BaseDamage;
        }

        public void TakeDamage(int damageAmt)
        {
            if (IsInvincible) return;

            currentHealth -= damageAmt;
            // make sure to set health to 0 if it somehow dips into the negative
            if (currentHealth < 0) currentHealth = 0;

            Debug.Log($"{nameof(PlayerData)}: {gameObject.name} took {damageAmt} damage! Current HP: {currentHealth}");
            healthEvents.OnDamage.Invoke(damageAmt);

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(ActivateIFrames());
            }
        }

        /// <summary>
        /// Activate attack boost given duration and multiplier
        /// </summary>
        public void ActivateAttackBoost(float duration, float multiplier)
        {
            Debug.Log($"{nameof(PlayerData)}: Attack boost activated on {gameObject.name} for {duration} seconds with {multiplier}x multiplier");
            damageMultiplier = multiplier;
            attackBonusTimer = duration;
        }

        private IEnumerator ActivateIFrames()
        {
            IsInvincible = true;
            Debug.Log($"{nameof(PlayerData)}: {gameObject.name} entered I-Frames! No damage can be taken.");

            // Activate potential visual effect here

            yield return new WaitForSeconds(invincibilityDuration);
            IsInvincible = false;
            Debug.Log($"{nameof(PlayerData)}: {gameObject.name} exited I-Frames! Can take damage again.");
        }

        public void SetHP(int value)
        {
            if (value > 0 && value <= maxHealth) { currentHealth = value; }
            else Debug.LogError($"{nameof(PlayerData)}: Value must be set between 0 and maxHealth.");
            healthEvents.SetHealth.Invoke(currentHealth);
        }

        private void Die()
        {
            if (IsDead) return;
            Debug.Log($"{nameof(PlayerData)}: {gameObject.name} has been eliminated!");
            IsDead = true;
            healthEvents.OnDeath.Invoke(this.gameObject);
            BattleManager.Instance.OnPlayerDeath(this.gameObject);
        }

        public void Initialize()
        {
            IsDead = false;
            IsInvincible = false;
            // Set Health and Stamina, trigger events so GUI is in sync
            currentHealth = maxHealth;
            healthEvents.SetMaxHealth.Invoke(maxHealth);
            currentStamina = maxStamina;
            staminaEvents.SetMaxStamina.Invoke(maxStamina);
        }

        [System.Serializable]
        public class HealthEvents
        {
            public UnityEvent<int> OnDamage = new UnityEvent<int>();
            public UnityEvent<ContactPoint, Transform> OnDeflect = new UnityEvent<ContactPoint, Transform>();
            public UnityEvent<ContactPoint, Transform> OnHit = new UnityEvent<ContactPoint, Transform>();
            public UnityEvent<int> SetMaxHealth = new UnityEvent<int>();
            public UnityEvent<int> SetHealth = new UnityEvent<int>();
            public UnityEvent<GameObject> OnDeath = new UnityEvent<GameObject>();
        }

        [System.Serializable]
        public class StaminaEvents
        {
            public UnityEvent<float> OnStaminaChanged = new UnityEvent<float>();
            public UnityEvent<float> SetMaxStamina = new UnityEvent<float>();
            public UnityEvent<float> OnStaminaHardDecrease = new UnityEvent<float>();
            public UnityEvent<float> OnStaminaHardIncrease = new UnityEvent<float>();
            public UnityEvent<float> OnLowStamina = new UnityEvent<float>();
        }

    }
}
