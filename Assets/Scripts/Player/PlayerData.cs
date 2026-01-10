using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using GASHAPWN.Audio;

// TO DO: Move flash-related effects to new script


namespace GASHAPWN
{
    /// <summary>
    /// Handles data relating to player actions and routes events
    /// </summary>
    public class PlayerData : MonoBehaviour
    {
        [HideInInspector] public Rigidbody rb;

        [Header("Health & Stamina")]
            public int maxHealth = 5;
            public int currentHealth;
            public float maxStamina = 6f;
            public float currentStamina;
            public float staminaRegenRate = 0.5f;

        [Header("I-Frame & Defense Settings")]
            public bool isInvincible = false;
            public float invincibilityDuration = 1.0f; // Default 1 second of i-frames
            public float damageReduction = 0f; // Percentage of damage reduction (0-1)
            public float defenseDuration = 0f; // Duration of active defense state
            private float defenseTimer = 0f; // Timer for tracking defense state

            private float damageMultiplier = 1.0f; // For offensive moves
            private float attackBonusTimer = 0f; // Timer for tracking attack bonus

        [Header("Events")]
            public UnityEvent<int> OnDamage = new UnityEvent<int>();
            public UnityEvent<int> SetMaxHealth = new UnityEvent<int>();
            public UnityEvent<int> SetHealth = new UnityEvent<int>();
            public UnityEvent<float> OnStaminaChanged = new UnityEvent<float>();
            public UnityEvent<float> SetMaxStamina = new UnityEvent<float>();
            public UnityEvent<float> OnStaminaHardDecrease = new UnityEvent<float>();
            public UnityEvent<float> OnStaminaHardIncrease = new UnityEvent<float>();
            public UnityEvent<float> OnLowStamina = new UnityEvent<float>();
            public UnityEvent<GameObject> OnDeath = new UnityEvent<GameObject>();

            // New events
            public UnityEvent OnDefenseActivated = new UnityEvent();
            public UnityEvent OnDefenseDeactivated = new UnityEvent();
            public UnityEvent OnAttackBonusActivated = new UnityEvent();
            public UnityEvent OnAttackBonusDeactivated = new UnityEvent();
            public UnityEvent<bool> OnChargeRoll = new UnityEvent<bool>();
            public UnityEvent OnSlam = new UnityEvent();

        [Header("Player State Flags")]
            [HideInInspector] public bool isGrounded = false;
            [HideInInspector] public bool controlsEnabled = false;
            [HideInInspector] public bool hasSlammed = false;
            [HideInInspector] public bool isCharging = false;
            [HideInInspector] public bool hasCharged = false;
            [HideInInspector] public bool isBursting = false; // Track burst state for invincibility
            [HideInInspector] public bool isDefending = false;
            [HideInInspector] public bool isDead = false;

        [Header("Movement Settings")]
            public float moveSpeed = 5f;
            public float minHitSpeed = 3f;
            public float deflectKnockbackMultiplier = 1.5f;
            public float slamAirborneTime = 1f;
            public float generalImpactSpeedThreshold = 2f;

        [Header("Physics Floatiness")]
            public float drag = 0f;
            public float angularDrag = 0.05f;

        [Header("Physic Material (optional)")]
            public PhysicsMaterial sphereMaterial;

        [Header("Air Control Settings")]
            public float airTorque = 5f;

            public Vector2 rotationInput;
            [HideInInspector] public ChargeRollIndicator chargeRollIndicator;

        [Header("Burst Settings")]
            public float burstInvincibilityDuration = 1.2f; // Invincibility duration during burst

        private ParticleEffects particleEffects;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            chargeRollIndicator = GetComponent<ChargeRollIndicator>();
        }

        private void Update()
        {
            // Update defense timer
            if (defenseTimer > 0)
            {
                defenseTimer -= Time.deltaTime;
                if (defenseTimer <= 0)
                {
                    // Defense bonus expired
                    damageReduction = 0f;
                    OnDefenseDeactivated.Invoke();
                }
            }

            // Update attack timer
            if (attackBonusTimer > 0)
            {
                attackBonusTimer -= Time.deltaTime;
                if (attackBonusTimer <= 0)
                {
                    // Attack bonus expired
                    damageMultiplier = 1.0f;
                    OnAttackBonusDeactivated.Invoke();
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
            {
                Rigidbody otherRb = collision.rigidbody;
                PlayerData otherPlayerData = collision.gameObject.GetComponent<PlayerData>();
                if (otherRb == null || otherPlayerData == null) return;

                // Calculate relative velocity between the players
                float relativeSpeed = (rb.linearVelocity - otherRb.linearVelocity).magnitude;

                // Check if either player is using an offensive ability
                bool selfOffensive = hasSlammed || hasCharged || isBursting;
                bool otherOffensive = otherPlayerData.hasSlammed || otherPlayerData.hasCharged || otherPlayerData.isBursting;

                // Calculate speed difference - lower value means more similar speeds
                float speedDifference = Mathf.Abs(rb.linearVelocity.magnitude - otherRb.linearVelocity.magnitude);

                // Deflection case - generous criteria but ONLY when:
                // 1. Both players using offensive abilities (always deflect)
                // 2. OR neither player is offensive BUT they have similar speeds
                bool shouldDeflect =
                    (selfOffensive && otherOffensive) || // Both offensive abilities always deflect
                    (!selfOffensive && !otherOffensive && speedDifference < 3f); // Similar speeds (generous threshold)

                if (relativeSpeed >= minHitSpeed)
                {
                    Vector3 contactPoint = collision.GetContact(0).point;

                    if (shouldDeflect)
                    {
                        // Both deflect each other with enhanced knockback
                        Vector3 deflectionDir = (transform.position - collision.transform.position).normalized;
                        float knockbackForce = deflectKnockbackMultiplier * relativeSpeed;

                        // Add some upward component to make deflections more visible and dramatic
                        Vector3 enhancedDeflection = (deflectionDir + Vector3.up * 0.3f).normalized;

                        rb.AddForce(enhancedDeflection * knockbackForce, ForceMode.Impulse);
                        otherRb.AddForce(-enhancedDeflection * knockbackForce, ForceMode.Impulse);

                        particleEffects?.PlayDeflectEffect(contactPoint);
                        GAME_SFXManager.Instance.Play_ImpactDeflect(transform);
                    }
                    // If I'm using an offensive ability and the other player is not, I win
                    else if (selfOffensive && !otherOffensive)
                    {
                        // Calculate damage based on offensive ability
                        int damageAmount = CalculateDamageAmount(relativeSpeed, true); // Using true for offensive ability

                        otherPlayerData.TakeDamage(damageAmount);
                        particleEffects?.PlayHitEffect(contactPoint);
                    }
                    // If the other player is using an offensive ability and I'm not, they win
                    else if (!selfOffensive && otherOffensive)
                    {
                        // No need to calculate damage here, as the other player's OnCollisionEnter will handle it
                        // But we need to handle knockback
                        Vector3 knockbackDir = (transform.position - collision.transform.position).normalized;
                        rb.AddForce(knockbackDir * deflectKnockbackMultiplier * relativeSpeed, ForceMode.Impulse);
                    }
                    // Neither player is using offensive abilities, and they don't have similar speeds
                    else
                    {
                        // Determine who has higher momentum (mass × velocity)
                        bool hasHigherMomentum = rb.linearVelocity.magnitude > otherRb.linearVelocity.magnitude;

                        if (hasHigherMomentum)
                        {
                            // Calculate damage based on speed difference
                            int damageAmount = CalculateDamageAmount(relativeSpeed, false);

                            otherPlayerData.TakeDamage(damageAmount);
                            particleEffects?.PlayHitEffect(contactPoint);

                        }
                    }
                }
            }
            else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Wall"))
            {
                if (rb.linearVelocity.magnitude > generalImpactSpeedThreshold) GAME_SFXManager.Instance.Play_ImpactGeneral(transform);
            }
        }

        // Calculate damage based on speed and offensive abilities
        private int CalculateDamageAmount(float relativeSpeed, bool isOffensiveAbility)
        {
            // For offensive abilities, deal flat damage
            if (isOffensiveAbility)
            {
                return Mathf.RoundToInt(1.5f);  // Flat 1.5 damage for offensive abilities
            }

            // Speed-based damage from 0.5 to 1.25 based on speed
            float speedDamage = Mathf.Lerp(0.5f, 1.25f, Mathf.Clamp01((relativeSpeed - minHitSpeed) / 10f));

            // Apply attacker's damage multiplier
            float totalDamage = speedDamage * damageMultiplier;

            return Mathf.Max(1, Mathf.RoundToInt(totalDamage));
        }

        private bool IsMetalEnd(Vector3 hitPoint)
        {
            return hitPoint.y < transform.position.y;
        }

        private bool IsDeflecting(Vector3 hitNormal)
        {
            return Vector3.Dot(hitNormal, transform.forward) > 0.5f;
        }

        private void ApplyKnockback(Rigidbody otherRb, float multiplier)
        {
            Vector3 knockbackDirection = (otherRb.position - transform.position).normalized;
            otherRb.AddForce(knockbackDirection * moveSpeed * multiplier, ForceMode.Impulse);
        }

        public void TakeDamage(int damageAmt)
        {
            if (isInvincible)
            {
                Debug.Log(gameObject.name + " is invincible! No damage taken.");
                return;
            }

            currentHealth -= damageAmt;
            // make sure to set health to 0 if it somehow dips into the negative
            if (currentHealth < 0) currentHealth = 0;

            Debug.Log(gameObject.name + " took " + damageAmt + " damage! Current HP: " + currentHealth);
            OnDamage.Invoke(damageAmt);

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
            Debug.Log($"Attack boost activated on {gameObject.name} for {duration} seconds with {multiplier}x multiplier");

            damageMultiplier = multiplier;
            attackBonusTimer = duration;
            OnAttackBonusActivated.Invoke();
            //StartCoroutine(AttackVisualEffect(duration));
        }

        // Used when performing burst move
        public void ActivateBurstInvincibility()
        {
            Debug.Log($"Burst invincibility activated on {gameObject.name}");

            isBursting = true;
            //StartCoroutine(BurstInvincibility());
        }

        private IEnumerator BurstInvincibility()
        {
            isInvincible = true;
            Debug.Log(gameObject.name + " entered Burst Invincibility!");

            // Store reference to the coroutine
            //Coroutine visualEffect = StartCoroutine(BurstVisualEffect());

            yield return new WaitForSeconds(burstInvincibilityDuration);

            isInvincible = false;
            isBursting = false;
            Debug.Log(gameObject.name + " exited Burst Invincibility!");

            // Stop the visual effect if it's still running
            //if (visualEffect != null)
            //    StopCoroutine(visualEffect);

            // Reset all renderers to original colors
            //ResetAllRenderersToOriginalColors();
        }

        private IEnumerator ActivateIFrames()
        {
            isInvincible = true;
            Debug.Log(gameObject.name + " entered I-Frames! No damage can be taken.");
            yield return new WaitForSeconds(invincibilityDuration);
            isInvincible = false;
            Debug.Log(gameObject.name + " exited I-Frames! Can take damage again.");
        }

        //private IEnumerator AttackVisualEffect(float duration)
        //{
        //    if (playerRenderers.Length == 0)
        //        yield break;

        //    // Apply attack color to all renderers
        //    for (int i = 0; i < playerRenderers.Length; i++)
        //    {
        //        if (playerRenderers[i] != null)
        //        {
        //            Color lerpedColor = Color.Lerp(originalColors[i], attackColor, 0.7f);
        //            playerRenderers[i].material.color = lerpedColor;
        //        }
        //    }

        //    yield return new WaitForSeconds(duration);

        //    // Only reset colors if not in another state (like invincibility)
        //    if (!isInvincible && defenseTimer <= 0 && attackBonusTimer <= 0)
        //    {
        //        //ResetAllRenderersToOriginalColors();
        //    }
        //}

        //private IEnumerator BurstVisualEffect()
        //{
        //    if (playerRenderers.Length == 0)
        //        yield break;

        //    float elapsedTime = 0f;

        //    while (elapsedTime < burstInvincibilityDuration)
        //    {
        //        // Pulse between white and yellow for burst
        //        float pulseValue = Mathf.PingPong(elapsedTime * 8f, 1f);

        //        for (int i = 0; i < playerRenderers.Length; i++)
        //        {
        //            if (playerRenderers[i] != null)
        //            {
        //                playerRenderers[i].material.color = Color.Lerp(Color.white, defenseColor, pulseValue);
        //            }
        //        }

        //        elapsedTime += Time.deltaTime;
        //        yield return null;
        //    }
        //}


        public void SetHP(int value)
        {
            if (value > 0 && value <= maxHealth) { currentHealth = value; }
            else Debug.LogError("Value must be set between 0 and maxHealth.");
            SetHealth.Invoke(currentHealth);
        }

        private void Die()
        {
            if (isDead) return;

            Debug.Log(gameObject.name + " has been eliminated!");
            isDead = true;
            OnDeath.Invoke(this.gameObject);
            BattleManager.Instance.OnPlayerDeath(this.gameObject);
        }

        public void InitializePlayerData()
        {
            isDead = false;
            controlsEnabled = true;

            // Set Health and Stamina, GUI as well
            currentHealth = maxHealth;
            SetMaxHealth.Invoke(maxHealth);
            currentStamina = maxStamina;
            SetMaxStamina.Invoke(maxStamina);
        }


        /// PUBLIC SPECIAL MOVE COROUTINES ///        


        public IEnumerator SlamCoroutine(float slamForce, float slamAirborneTime)
        {
            controlsEnabled = false;
            OnSlam.Invoke();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;

            yield return new WaitForSeconds(slamAirborneTime);

            GAME_SFXManager.Instance.Play_Drop(this.transform);
            rb.useGravity = true;
            rb.AddForce(Vector3.down * slamForce, ForceMode.Impulse);

            hasSlammed = true;
        }

        public IEnumerator QuickBreakCoroutine(float quickBreakDuration, float quickBreakDefenseDuration)
        {
            // Make player temporarily invincible during quick break
            isInvincible = true;

            Vector3 initialVelocity = rb.linearVelocity;
            Vector3 initialAngularVelocity = rb.angularVelocity;

            Quaternion initialRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.FromToRotation(-transform.up, Vector3.up) * transform.rotation;

            // Break
            float elapsed = 0f;
            while (elapsed < quickBreakDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / quickBreakDuration);

                rb.linearVelocity = Vector3.Lerp(initialVelocity, Vector3.zero, t);
                rb.angularVelocity = Vector3.Lerp(initialAngularVelocity, Vector3.zero, t);

                transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

                yield return null;
            }

            // Activate defense
            isDefending = true;
            OnDefenseActivated.Invoke();

            elapsed = 0f;
            while (elapsed < quickBreakDefenseDuration)
            {
                elapsed += Time.deltaTime;
                damageReduction = Mathf.Clamp01(0.5f); // 50% damage reduction
                yield return null;
            }

            if (isDefending) QuickBreak_Cancel();
        }

        public void QuickBreak_Cancel()
        {
            //// Restore invincibility state
            isInvincible = false;
            isDefending = false;
            OnDefenseDeactivated?.Invoke();
        }

    }
}
