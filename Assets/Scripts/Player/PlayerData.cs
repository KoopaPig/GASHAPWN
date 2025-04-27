using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using GASHAPWN;
using GASHAPWN.Audio;

public class PlayerData : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Health & Stamina")]
    public int maxHealth = 5;
    public int currentHealth;
    public float maxStamina = 6f;
    public float currentStamina;
    public float staminaRegenRate = .5f;

    [Header("I-Frame & Defense Settings")]
    public bool isInvincible = false;
    public float invincibilityDuration = 1.0f;
    public float damageReduction = 0f;
    public float defenseDuration = 0f;
    public float defenseTimer = 0f;

    [Header("Attack Properties")]
    public float damageMultiplier = 1.0f;
    public float attackBonusDuration = 0f;
    private float attackBonusTimer = 0f;

    [Header("Move-Specific Damage Values")]
    // Changed to 2 for both slam and charge
    public int slamDamage = 2;
    public int chargeRollDamage = 2;
    public int quickBreakDamage = 1;
    public int normalCollisionDamage = 1;

    [Header("Momentum-Based Damage System")]
    [Tooltip("Minimum relative speed needed to consider damage")]
    public float minDamageSpeed = 3f;
    
    [Tooltip("Maximum damage from a normal collision (non-ability)")]
    public int maxMomentumDamage = 2;
    
    [Tooltip("Speed difference percentage threshold for deflection")]
    [Range(0f, 1f)]
    public float deflectionThreshold = 0.4f;
    
    [Tooltip("Coefficient used to convert speed difference to damage")]
    public float speedToDamageCoefficient = 0.1f;
    
    [Header("Ability State Tracking")]
    public bool isUsingOffensiveAbility = false;
    public bool isUsingDefensiveAbility = false;

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
    
    public UnityEvent OnDefenseActivated = new UnityEvent();
    public UnityEvent OnDefenseDeactivated = new UnityEvent();
    public UnityEvent OnAttackBonusActivated = new UnityEvent();
    public UnityEvent OnAttackBonusDeactivated = new UnityEvent();
    public UnityEvent<bool> OnChargeRoll = new UnityEvent<bool>();

    [Header("Player State Flags")]
    public bool isGrounded = false;
    public bool controlsEnabled = true;
    public bool hasSlammed = false;
    public bool isCharging = false;
    public bool hasCharged = false;
    public bool isBursting = false;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float minHitSpeed = 3f;
    public float deflectKnockbackMultiplier = 1.5f;
    public float slamAirborneTime = 1f;

    [Header("Physics Floatiness")]
    public float drag = 0f;
    public float angularDrag = 0.05f;

    [Header("Physic Material (optional)")]
    public PhysicsMaterial sphereMaterial;

    [Header("Air Control Settings")]
    public float airTorque = 5f;

    [Header("Slam Settings")]
    public float slamForce = 80f;
    public float slamDelay = .4f;

    [Header("Quick Break Settings")]
    public float quickBreakDuration = 0.2f;
    public float quickBreakDefenseDuration = 1.5f;

    [Header("Charge Roll Settings")]
    public float chargeRollMinForce = 15f;
    public float chargeRollMaxForce = 45f;
    public float chargeRollMaxDuration = 2f;
    public float chargeRollSpinSpeed = 1000f;
    public float chargeRollAttackBoostDuration = 2.0f;

    [Header("Burst Settings")]
    public float burstInvincibilityDuration = 1.2f;

    [Header("Visual Feedback")]
    public Color flashColor = Color.red;
    public Color defenseColor = Color.blue;
    public Color attackColor = Color.yellow;
    public float flashSpeed = 0.1f;

    private Renderer[] playerRenderers;
    private Color[] originalColors;

    private ParticleEffects particleEffects;

    private void Start()
    {
        currentHealth = maxHealth;
        SetMaxHealth.Invoke(maxHealth);
        currentStamina = maxStamina;
        SetMaxStamina.Invoke(maxStamina);
        rb = GetComponent<Rigidbody>();

        playerRenderers = GetComponentsInChildren<Renderer>();
        
        if (playerRenderers.Length == 0)
        {
            Debug.LogError($"No renderers found in {gameObject.name} or its children! Visual effects won't work.");
        }
        
        originalColors = new Color[playerRenderers.Length];
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i].sharedMaterial != null)
            {
                playerRenderers[i].material = new Material(playerRenderers[i].sharedMaterial);
                originalColors[i] = playerRenderers[i].material.color;
                
                if (originalColors[i] == Color.black)
                {
                    originalColors[i] = Color.white;
                    playerRenderers[i].material.color = Color.white;
                }
            }
            else
            {
                originalColors[i] = Color.white;
            }
        }

        particleEffects = GetComponent<ParticleEffects>();
        if (particleEffects == null) {
            Debug.LogWarning("ParticleEffects component not found on " + gameObject.name);
        }
        
        UpdateAbilityFlags();
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
                isUsingDefensiveAbility = false;
                ResetAllRenderersToOriginalColors(); // Added: Reset colors when defense expires
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
                ResetAllRenderersToOriginalColors(); // Added: Reset colors when attack bonus expires
            }
        }
        
        UpdateAbilityFlags();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            Rigidbody otherRb = collision.rigidbody;
            PlayerData otherPlayerData = collision.gameObject.GetComponent<PlayerData>();
            if (otherRb == null || otherPlayerData == null) return;

            Vector3 relativeVelocity = rb.linearVelocity - otherRb.linearVelocity;
            float relativeSpeed = relativeVelocity.magnitude;
            
            Vector3 contactPoint = collision.GetContact(0).point;
            
            if (relativeSpeed >= minDamageSpeed)
            {
                if (HandleAbilityInteractions(otherPlayerData, contactPoint, relativeVelocity))
                {
                    return;
                }
                
                if (isInvincible || otherPlayerData.isInvincible || 
                    isUsingDefensiveAbility || otherPlayerData.isUsingDefensiveAbility)
                {
                    return;
                }
                
                float mySpeed = rb.linearVelocity.magnitude;
                float otherSpeed = otherRb.linearVelocity.magnitude;
                
                bool myOffensiveAbility = isUsingOffensiveAbility;
                bool otherOffensiveAbility = otherPlayerData.isUsingOffensiveAbility;
                
                if (myOffensiveAbility && !otherOffensiveAbility)
                {
                    int damageAmount = GetOffensiveAbilityDamage();
                    otherPlayerData.TakeDamage(damageAmount);
                    particleEffects?.PlayHitEffect(contactPoint);
                    return;
                }
                else if (!myOffensiveAbility && otherOffensiveAbility)
                {
                    int damageAmount = otherPlayerData.GetOffensiveAbilityDamage();
                    TakeDamage(damageAmount);
                    particleEffects?.PlayHitEffect(contactPoint);
                    return;
                }
                
                float myDeflectThreshold = otherSpeed * (1f + deflectionThreshold);
                float otherDeflectThreshold = mySpeed * (1f + deflectionThreshold);
                
                bool isDeflection = mySpeed <= myDeflectThreshold && otherSpeed <= otherDeflectThreshold;
                
                if (isDeflection)
                {
                    ApplyDeflection(otherRb, contactPoint);
                    particleEffects?.PlayDeflectEffect(contactPoint);
                }
                else
                {
                    bool isFasterThanOpponent = mySpeed > otherSpeed;
                    
                    if (isFasterThanOpponent)
                    {
                        int damageAmount = CalculateMomentumDamage(mySpeed, otherSpeed);
                        otherPlayerData.TakeDamage(damageAmount);
                        particleEffects?.PlayHitEffect(contactPoint);
                    }
                    else
                    {
                        int damageAmount = CalculateMomentumDamage(otherSpeed, mySpeed);
                        TakeDamage(damageAmount);
                        particleEffects?.PlayHitEffect(contactPoint);
                    }
                }
            }
        }
    }
        
    // Updated to return flat 2 damage for slam and charge
    private int GetOffensiveAbilityDamage()
    {
        if (hasCharged)
        {
            return 2; // Flat damage for charge roll
        }
        else if (hasSlammed)
        {
            return 2; // Flat damage for slam
        }
        
        return Mathf.RoundToInt(normalCollisionDamage * damageMultiplier);
    }
        
    // Modified to return damage between 0.5 and 1.5 based on speed
    private int CalculateMomentumDamage(float fasterSpeed, float slowerSpeed)
    {
        // Calculate speed difference
        float speedDifference = fasterSpeed - slowerSpeed;
        
        // Calculate raw damage value between 0.5 and 1.5 based on speed difference
        // Full range reached at 10 speed difference
        float rawDamage = Mathf.Lerp(0.5f, 1.5f, Mathf.Clamp01(speedDifference / 10f));
        
        // Round to nearest 0.5
        float roundedDamage = Mathf.Round(rawDamage * 2) / 2;
        
        // Convert to int - will be either 0, 1, or 1 for values in our range
        // Note: using ceiling to ensure 0.5 becomes 1
        return Mathf.CeilToInt(roundedDamage);
    }
        
    private void ApplyDeflection(Rigidbody otherRb, Vector3 contactPoint)
    {
        Vector3 myDeflectionDirection = (transform.position - contactPoint).normalized;
        Vector3 otherDeflectionDirection = (otherRb.position - contactPoint).normalized;
        
        rb.AddForce(myDeflectionDirection * deflectKnockbackMultiplier * rb.linearVelocity.magnitude, ForceMode.Impulse);
        otherRb.AddForce(otherDeflectionDirection * deflectKnockbackMultiplier * otherRb.linearVelocity.magnitude, ForceMode.Impulse);
    }
        
    private bool HandleAbilityInteractions(PlayerData otherPlayerData, Vector3 contactPoint, Vector3 relativeVelocity)
    {
        if (isUsingDefensiveAbility || otherPlayerData.isUsingDefensiveAbility)
        {
            return true;
        }
        
        if (isUsingOffensiveAbility && otherPlayerData.isUsingOffensiveAbility)
        {
            float knockbackForce = relativeVelocity.magnitude * 1.5f;
            Vector3 myKnockbackDir = (transform.position - contactPoint).normalized;
            Vector3 otherKnockbackDir = (otherPlayerData.transform.position - contactPoint).normalized;
            
            rb.AddForce(myKnockbackDir * knockbackForce, ForceMode.Impulse);
            otherPlayerData.GetComponent<Rigidbody>().AddForce(otherKnockbackDir * knockbackForce, ForceMode.Impulse);
            
            particleEffects?.PlayDeflectEffect(contactPoint);
            return true;
        }
        
        return false;
    }
        
    public void UpdateAbilityFlags()
    {
        isUsingOffensiveAbility = hasSlammed || hasCharged;
        isUsingDefensiveAbility = isBursting || (defenseTimer > 0f && damageReduction >= 0.9f);
    }

    public void TakeDamage(int damageAmt)
    {
        if (isInvincible || isUsingDefensiveAbility) 
        {
            return;
        }

        if (damageReduction > 0)
        {
            damageAmt = Mathf.Max(1, Mathf.RoundToInt(damageAmt * (1f - damageReduction)));
        }

        currentHealth -= damageAmt;
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

    public void ActivateDefense(float duration, float reduction)
    {
        damageReduction = Mathf.Clamp01(reduction);
        defenseTimer = duration;
        OnDefenseActivated.Invoke();
        
        UpdateAbilityFlags();
        
        StartCoroutine(DefenseVisualEffect(duration));
    }

    public void ActivateAttackBoost(float duration, float multiplier)
    {
        damageMultiplier = multiplier;
        attackBonusTimer = duration;
        OnAttackBonusActivated.Invoke();
        
        UpdateAbilityFlags();
        
        StartCoroutine(AttackVisualEffect(duration));
    }

    public void ActivateBurstInvincibility()
    {
        isBursting = true;
        isInvincible = true;
        UpdateAbilityFlags();
        
        StartCoroutine(BurstInvincibility());
    }

    private IEnumerator BurstInvincibility()
    {
        Coroutine visualEffect = StartCoroutine(BurstVisualEffect());

        yield return new WaitForSeconds(burstInvincibilityDuration);

        isInvincible = false;
        isBursting = false;
        UpdateAbilityFlags();

        if (visualEffect != null)
            StopCoroutine(visualEffect);

        ResetAllRenderersToOriginalColors();
    }

    private IEnumerator ActivateIFrames()
    {
        isInvincible = true;

        StartCoroutine(FlashEffect());

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;

        ResetAllRenderersToOriginalColors();
    }

    private IEnumerator FlashEffect()
    {
        if (playerRenderers.Length == 0)
            yield break;

        int flashCount = 0;
        
        while (isInvincible && flashCount < 20)
        {
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                if (playerRenderers[i] != null)
                {
                    playerRenderers[i].material.color = flashColor;
                }
            }
            
            yield return new WaitForSeconds(flashSpeed);
            
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                if (playerRenderers[i] != null)
                {
                    playerRenderers[i].material.color = originalColors[i];
                }
            }
            
            yield return new WaitForSeconds(flashSpeed);
            flashCount++;
        }
        
        ResetAllRenderersToOriginalColors();
    }

    // Modified to ensure colors are reset when defense effect ends
    private IEnumerator DefenseVisualEffect(float duration)
    {
        if (playerRenderers.Length == 0)
            yield break;

        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
            {
                Color lerpedColor = Color.Lerp(originalColors[i], defenseColor, 0.7f);
                playerRenderers[i].material.color = lerpedColor;
            }
        }
        
        yield return new WaitForSeconds(duration);
        
        // Reset colors when the effect ends, but only if not in another state
        if (!isInvincible && defenseTimer <= 0 && attackBonusTimer <= 0)
        {
            ResetAllRenderersToOriginalColors();
        }
    }

    // Modified to ensure colors are reset when attack effect ends
    private IEnumerator AttackVisualEffect(float duration)
    {
        if (playerRenderers.Length == 0)
            yield break;

        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
            {
                Color lerpedColor = Color.Lerp(originalColors[i], attackColor, 0.7f);
                playerRenderers[i].material.color = lerpedColor;
            }
        }
        
        yield return new WaitForSeconds(duration);
        
        // Reset colors when the effect ends, but only if not in another state
        if (!isInvincible && defenseTimer <= 0 && attackBonusTimer <= 0)
        {
            ResetAllRenderersToOriginalColors();
        }
    }

    private IEnumerator BurstVisualEffect()
    {
        if (playerRenderers.Length == 0)
            yield break;

        float elapsedTime = 0f;
        
        while (elapsedTime < burstInvincibilityDuration)
        {
            float pulseValue = Mathf.PingPong(elapsedTime * 8f, 1f);
            
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                if (playerRenderers[i] != null)
                {
                    playerRenderers[i].material.color = Color.Lerp(Color.white, defenseColor, pulseValue);
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Make sure to reset colors at the end
        ResetAllRenderersToOriginalColors();
    }

    // Helper method to reset all renderers to original colors
    private void ResetAllRenderersToOriginalColors()
    {
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
            {
                playerRenderers[i].material.color = originalColors[i];
            }
        }
    }
    
    // Called when player actions reset defense
    public void ClearDefenseState()
    {
        defenseTimer = 0f;
        damageReduction = 0f;
        OnDefenseDeactivated.Invoke();
        UpdateAbilityFlags();
        ResetAllRenderersToOriginalColors();
    }
    
    private void OnDisable()
    {
        ResetAllRenderersToOriginalColors();
    }

    public void SetHP(int value)
    {
        if (value > 0 && value <= maxHealth) { currentHealth = value; }
        else Debug.LogError("Value must be set between 0 and maxHealth.");
        SetHealth.Invoke(currentHealth);
    }

    private void Die()
    {
        OnDeath.Invoke(this.gameObject);
        BattleManager.Instance.OnPlayerDeath(this.gameObject);
    }
}