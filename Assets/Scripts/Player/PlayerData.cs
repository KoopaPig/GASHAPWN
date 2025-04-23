using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using GASHAPWN;

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
    public float invincibilityDuration = 1.0f; // Default 1 second of i-frames
    public float damageReduction = 0f; // Percentage of damage reduction (0-1)
    public float defenseDuration = 0f; // Duration of active defense state
    public float defenseTimer = 0f; // Timer for tracking defense state

    [Header("Attack Properties")]
    public float damageMultiplier = 1.0f; // For offensive moves
    public float attackBonusDuration = 0f; // Duration of attack bonus
    private float attackBonusTimer = 0f; // Timer for tracking attack bonus

    [Header("Move-Specific Damage Values")]
    public int slamDamage = 2;
    public int chargeRollDamage = 3;
    // Burst doesn't deal damage, only knockback
    public int quickBreakDamage = 1;
    public int normalCollisionDamage = 1;

    [Header("Momentum-Based Damage System")]
    [Tooltip("Minimum relative speed needed to consider damage")]
    public float minDamageSpeed = 3f;
    
    [Tooltip("Maximum damage from a normal collision (non-ability)")]
    public int maxMomentumDamage = 2;
    
    [Tooltip("Speed difference percentage threshold for deflection")]
    [Range(0f, 1f)]
    public float deflectionThreshold = 0.3f; // Increased to 30% for more common deflections
    
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
    public UnityEvent<GameObject> OnDeath = new UnityEvent<GameObject>();
    
    // New events
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
    public bool isBursting = false; // Track burst state for invincibility

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float minHitSpeed = 3f;
    public float deflectKnockbackMultiplier = 1.5f;

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
    public float quickBreakDefenseDuration = 1.5f; // Duration of defense boost after quick break

    [Header("Charge Roll Settings")]
    public float chargeRollMinForce = 15f;
    public float chargeRollMaxForce = 45f;
    public float chargeRollMaxDuration = 2f;
    public float chargeRollSpinSpeed = 1000f;
    public float chargeRollAttackBoostDuration = 2.0f; // Duration of attack boost after charge

    [Header("Burst Settings")]
    public float burstInvincibilityDuration = 1.2f; // Invincibility duration during burst

    [Header("Visual Feedback")]
    public Color flashColor = Color.red; // Color to flash during i-frames
    public Color defenseColor = Color.blue; // Color to show during defense boost
    public Color attackColor = Color.yellow; // Color to show during attack boost
    public float flashSpeed = 0.1f; // How fast to flash

    // Multiple renderer support
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

        // Find all renderers in the player hierarchy
        playerRenderers = GetComponentsInChildren<Renderer>();
        
        Debug.Log($"Found {playerRenderers.Length} renderers in {gameObject.name}");
        
        if (playerRenderers.Length == 0)
        {
            Debug.LogError($"No renderers found in {gameObject.name} or its children! Visual effects won't work.");
        }
        
        // Store all original colors
        originalColors = new Color[playerRenderers.Length];
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            // Create unique material instances to avoid shared material issues
            if (playerRenderers[i].sharedMaterial != null)
            {
                playerRenderers[i].material = new Material(playerRenderers[i].sharedMaterial);
                // Store the current color (which should be the correct original color)
                originalColors[i] = playerRenderers[i].material.color;
                
                // Debug the original color we're storing
                Debug.Log($"Original color for renderer {i}: {originalColors[i]}");
                
                // Make sure the initial color is not black
                if (originalColors[i] == Color.black)
                {
                    Debug.LogWarning($"Original color for renderer {i} is black, setting to white");
                    originalColors[i] = Color.white;
                    playerRenderers[i].material.color = Color.white;
                }
            }
            else
            {
                Debug.LogWarning($"Renderer {i} has no shared material!");
                originalColors[i] = Color.white;
            }
        }

        particleEffects = GetComponent<ParticleEffects>();
        if (particleEffects == null) {
            Debug.LogWarning("ParticleEffects component not found on " + gameObject.name);
        }
        
        // Initialize ability flags
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
        
        // Continuously update ability flags
        UpdateAbilityFlags();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player1") || collision.gameObject.CompareTag("Player2"))
        {
            Rigidbody otherRb = collision.rigidbody;
            PlayerData otherPlayerData = collision.gameObject.GetComponent<PlayerData>();
            if (otherRb == null || otherPlayerData == null) return;

            // Calculate relative velocity at point of impact
            Vector3 relativeVelocity = rb.linearVelocity - otherRb.linearVelocity;
            float relativeSpeed = relativeVelocity.magnitude;
            
            // Get contact point for visual effects
            Vector3 contactPoint = collision.GetContact(0).point;
            
            // Log collision for debugging
            Debug.Log($"Collision between {gameObject.name} (speed: {rb.linearVelocity.magnitude:F1}) and {collision.gameObject.name} (speed: {otherRb.linearVelocity.magnitude:F1}) - relative speed: {relativeSpeed:F1}");
            
            // Check if collision is fast enough to consider for damage
            if (relativeSpeed >= minDamageSpeed)
            {
                // Handle ability interactions first
                if (HandleAbilityInteractions(otherPlayerData, contactPoint, relativeVelocity))
                {
                    // If ability interaction was handled, exit
                    return;
                }
                
                // Check if either player is invincible or using defensive ability
                if (isInvincible || otherPlayerData.isInvincible || 
                    isUsingDefensiveAbility || otherPlayerData.isUsingDefensiveAbility)
                {
                    // No damage for protected players
                    Debug.Log($"Player {(isInvincible || isUsingDefensiveAbility ? gameObject.name : otherPlayerData.gameObject.name)} is protected, no damage applied");
                    return;
                }
                
                // Calculate speeds for comparisons
                float mySpeed = rb.linearVelocity.magnitude;
                float otherSpeed = otherRb.linearVelocity.magnitude;
                
                // Check if either player is using an offensive ability
                bool myOffensiveAbility = isUsingOffensiveAbility;
                bool otherOffensiveAbility = otherPlayerData.isUsingOffensiveAbility;
                
                // Log abilities for debugging
                if (myOffensiveAbility)
                    Debug.Log($"{gameObject.name} is using offensive ability: HasCharged={hasCharged}, HasSlammed={hasSlammed}");
                if (otherOffensiveAbility)
                    Debug.Log($"{collision.gameObject.name} is using offensive ability: HasCharged={otherPlayerData.hasCharged}, HasSlammed={otherPlayerData.hasSlammed}");
                
                // If I'm using an offensive ability and the other player isn't
                if (myOffensiveAbility && !otherOffensiveAbility)
                {
                    // Offensive ability always deals damage regardless of speed
                    int damageAmount = GetOffensiveAbilityDamage();
                    otherPlayerData.TakeDamage(damageAmount);
                    particleEffects?.PlayHitEffect(contactPoint);
                    Debug.Log($"Offensive ability hit! {gameObject.name} deals {damageAmount} damage to {collision.gameObject.name}");
                    return;
                }
                // If other player is using an offensive ability and I'm not
                else if (!myOffensiveAbility && otherOffensiveAbility)
                {
                    // Other player's offensive ability always deals damage regardless of speed
                    int damageAmount = otherPlayerData.GetOffensiveAbilityDamage();
                    TakeDamage(damageAmount);
                    particleEffects?.PlayHitEffect(contactPoint);
                    Debug.Log($"Offensive ability hit! {collision.gameObject.name} deals {damageAmount} damage to {gameObject.name}");
                    return;
                }
                
                // Calculate speed thresholds for deflection
                // A player will deflect if their speed is within the threshold percentage of the other player
                float myDeflectThreshold = otherSpeed * (1f + deflectionThreshold);
                float otherDeflectThreshold = mySpeed * (1f + deflectionThreshold);
                
                // Determine if this is a deflection (speeds are similar)
                bool isDeflection = mySpeed <= myDeflectThreshold && otherSpeed <= otherDeflectThreshold;
                
                Debug.Log($"Deflection check: My speed {mySpeed:F1} <= {myDeflectThreshold:F1}? {mySpeed <= myDeflectThreshold}");
                Debug.Log($"Deflection check: Other speed {otherSpeed:F1} <= {otherDeflectThreshold:F1}? {otherSpeed <= otherDeflectThreshold}");
                Debug.Log($"Is deflection? {isDeflection}");
                
                if (isDeflection)
                {
                    // Both players deflect
                    ApplyDeflection(otherRb, contactPoint);
                    particleEffects?.PlayDeflectEffect(contactPoint);
                    Debug.Log("Deflection! Both players knocked back.");
                }
                else
                {
                    // Determine which player has higher speed
                    bool isFasterThanOpponent = mySpeed > otherSpeed;
                    
                    if (isFasterThanOpponent)
                    {
                        // I'm faster, I deal damage to the other player
                        int damageAmount = CalculateMomentumDamage(mySpeed, otherSpeed);
                        otherPlayerData.TakeDamage(damageAmount);
                        particleEffects?.PlayHitEffect(contactPoint);
                        Debug.Log($"Hit! {gameObject.name} deals {damageAmount} damage to {collision.gameObject.name}");
                    }
                    else
                    {
                        // Other player is faster, I take damage
                        int damageAmount = CalculateMomentumDamage(otherSpeed, mySpeed);
                        TakeDamage(damageAmount);
                        particleEffects?.PlayHitEffect(contactPoint);
                        Debug.Log($"Hit! {collision.gameObject.name} deals {damageAmount} damage to {gameObject.name}");
                    }
                }
            }
        }
    }
        
    // Get damage value for current offensive ability
    private int GetOffensiveAbilityDamage()
    {
        if (hasCharged)
        {
            // Always ensure charge roll does at least 2 damage, but can do more with multiplier
            return Mathf.Max(2, Mathf.RoundToInt(chargeRollDamage * damageMultiplier));
        }
        else if (hasSlammed)
        {
            // Use the enhanced slamDamage value
            return Mathf.RoundToInt(slamDamage * damageMultiplier);
        }
        
        // Default offensive ability damage
        return Mathf.RoundToInt(normalCollisionDamage * damageMultiplier);
    }
        
    // Calculate damage based on speed difference - modified to scale better with speed
    private int CalculateMomentumDamage(float fasterSpeed, float slowerSpeed)
    {
        // Calculate speed difference as a percentage of the faster speed
        float speedDifference = fasterSpeed - slowerSpeed;
        
        // More dramatic scaling with speed difference
        float speedRatio = Mathf.Clamp01(speedDifference / fasterSpeed);
        
        // Map the speedRatio to damage, with clearer impact from higher speeds
        // Scale damage exponentially based on speed ratio (more speed = significantly more damage)
        float scaledRatio = Mathf.Pow(speedRatio, 0.75f); // Less severe scaling than square 
        
        // Convert to damage value (scaling up to maxMomentumDamage)
        int baseDamage = Mathf.RoundToInt(scaledRatio * maxMomentumDamage);
        
        // Add a speed bonus for very high speeds (over 20)
        float speedBonus = 0;
        if (fasterSpeed > 20f)
        {
            speedBonus = Mathf.Floor((fasterSpeed - 20f) / 10f);
        }
        
        // Combine base damage with speed bonus
        int damage = Mathf.RoundToInt(baseDamage + speedBonus);
        
        // For normal collisions, ensure at least normalCollisionDamage
        damage = Mathf.Max(damage, normalCollisionDamage);
        
        // Ensure minimum of 1 damage if any damage is to be dealt
        return Mathf.Max(1, damage);
    }
        
    // Apply deflection physics
    private void ApplyDeflection(Rigidbody otherRb, Vector3 contactPoint)
    {
        // Calculate direction away from contact point for each player
        Vector3 myDeflectionDirection = (transform.position - contactPoint).normalized;
        Vector3 otherDeflectionDirection = (otherRb.position - contactPoint).normalized;
        
        // Apply knockback force to both players
        rb.AddForce(myDeflectionDirection * deflectKnockbackMultiplier * rb.linearVelocity.magnitude, ForceMode.Impulse);
        otherRb.AddForce(otherDeflectionDirection * deflectKnockbackMultiplier * otherRb.linearVelocity.magnitude, ForceMode.Impulse);
    }
        
    // Handle special ability interactions
    private bool HandleAbilityInteractions(PlayerData otherPlayerData, Vector3 contactPoint, Vector3 relativeVelocity)
    {
        // Handle defensive abilities first
        if (isUsingDefensiveAbility || otherPlayerData.isUsingDefensiveAbility)
        {
            // Defensive abilities prevent damage
            Debug.Log("Defensive ability active - no damage applied");
            return true;
        }
        
        // Handle offensive vs offensive ability clash
        if (isUsingOffensiveAbility && otherPlayerData.isUsingOffensiveAbility)
        {
            // Both players using offensive abilities - cancel out and knock back
            // Calculate increased knockback based on combined momentum
            float knockbackForce = relativeVelocity.magnitude * 1.5f;
            Vector3 myKnockbackDir = (transform.position - contactPoint).normalized;
            Vector3 otherKnockbackDir = (otherPlayerData.transform.position - contactPoint).normalized;
            
            rb.AddForce(myKnockbackDir * knockbackForce, ForceMode.Impulse);
            otherPlayerData.GetComponent<Rigidbody>().AddForce(otherKnockbackDir * knockbackForce, ForceMode.Impulse);
            
            particleEffects?.PlayDeflectEffect(contactPoint);
            Debug.Log("Offensive abilities collided! Increased knockback applied.");
            return true;
        }
        
        // No special interaction
        return false;
    }
        
    // Update ability flags based on player state
    public void UpdateAbilityFlags()
    {
        // Offensive abilities
        isUsingOffensiveAbility = hasSlammed || hasCharged;
        
        // Defensive abilities (burst or high damage reduction from quick break)
        isUsingDefensiveAbility = isBursting || (defenseTimer > 0f && damageReduction >= 0.9f);
    }

    public void TakeDamage(int damageAmt)
    {
        if (isInvincible || isUsingDefensiveAbility) 
        {
            Debug.Log(gameObject.name + " is protected! No damage taken.");
            return;
        }

        // Apply damage reduction if active
        if (damageReduction > 0)
        {
            damageAmt = Mathf.Max(1, Mathf.RoundToInt(damageAmt * (1f - damageReduction)));
        }

        currentHealth -= damageAmt;
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

    // Activate defense boost
    public void ActivateDefense(float duration, float reduction)
    {
        Debug.Log($"Defense activated on {gameObject.name} for {duration} seconds with {reduction*100}% reduction");
        
        damageReduction = Mathf.Clamp01(reduction); // Clamp between 0-1
        defenseTimer = duration;
        OnDefenseActivated.Invoke();
        
        // Update ability flags immediately
        UpdateAbilityFlags();
        
        StartCoroutine(DefenseVisualEffect(duration));
    }

    // Activate attack boost
    public void ActivateAttackBoost(float duration, float multiplier)
    {
        Debug.Log($"Attack boost activated on {gameObject.name} for {duration} seconds with {multiplier}x multiplier");
        
        damageMultiplier = multiplier;
        attackBonusTimer = duration;
        OnAttackBonusActivated.Invoke();
        
        // Update ability flags immediately
        UpdateAbilityFlags();
        
        StartCoroutine(AttackVisualEffect(duration));
    }

    // Used when performing burst move
    public void ActivateBurstInvincibility()
    {
        Debug.Log($"Burst invincibility activated on {gameObject.name}");
        
        isBursting = true;
        isInvincible = true;
        UpdateAbilityFlags();
        
        StartCoroutine(BurstInvincibility());
    }

    private IEnumerator BurstInvincibility()
    {
        Debug.Log(gameObject.name + " entered Burst Invincibility!");

        // Store reference to the coroutine
        Coroutine visualEffect = StartCoroutine(BurstVisualEffect());

        yield return new WaitForSeconds(burstInvincibilityDuration);

        isInvincible = false;
        isBursting = false;
        UpdateAbilityFlags();
        Debug.Log(gameObject.name + " exited Burst Invincibility!");

        // Stop the visual effect if it's still running
        if (visualEffect != null)
            StopCoroutine(visualEffect);

        // Reset all renderers to original colors
        ResetAllRenderersToOriginalColors();
    }

    private IEnumerator ActivateIFrames()
    {
        isInvincible = true;
        Debug.Log(gameObject.name + " entered I-Frames! No damage can be taken.");

        StartCoroutine(FlashEffect());

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
        Debug.Log(gameObject.name + " exited I-Frames! Can take damage again.");

        // Reset all renderers to original colors
        ResetAllRenderersToOriginalColors();
    }

    private IEnumerator FlashEffect()
    {
        if (playerRenderers.Length == 0)
            yield break;

        Debug.Log($"Starting flash effect on {gameObject.name}");
        
        // Track how many flashes we've done
        int flashCount = 0;
        
        while (isInvincible && flashCount < 20) // Limit to 20 flashes as a safety
        {
            // Change all renderers to flash color
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                if (playerRenderers[i] != null)
                {
                    playerRenderers[i].material.color = flashColor;
                }
            }
            
            yield return new WaitForSeconds(flashSpeed);
            
            // Change all renderers back to original color
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
        
        Debug.Log($"Flash effect ended on {gameObject.name} after {flashCount} flashes");
        
        // Ensure colors are reset at the end
        ResetAllRenderersToOriginalColors();
    }

    private IEnumerator DefenseVisualEffect(float duration)
    {
        if (playerRenderers.Length == 0)
            yield break;

        // Apply defense color to all renderers
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
            {
                Color lerpedColor = Color.Lerp(originalColors[i], defenseColor, 0.7f);
                playerRenderers[i].material.color = lerpedColor;
            }
        }
        
        yield return new WaitForSeconds(duration);
        
        // Only reset colors if not in another state (like invincibility)
        if (!isInvincible && defenseTimer <= 0 && attackBonusTimer <= 0)
        {
            ResetAllRenderersToOriginalColors();
        }
    }

    private IEnumerator AttackVisualEffect(float duration)
    {
        if (playerRenderers.Length == 0)
            yield break;

        // Apply attack color to all renderers
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
            {
                Color lerpedColor = Color.Lerp(originalColors[i], attackColor, 0.7f);
                playerRenderers[i].material.color = lerpedColor;
            }
        }
        
        yield return new WaitForSeconds(duration);
        
        // Only reset colors if not in another state (like invincibility)
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
            // Pulse between white and yellow for burst
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
    }

    // Helper method to reset all renderers to original colors
    private void ResetAllRenderersToOriginalColors()
    {
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
            {
                playerRenderers[i].material.color = originalColors[i];
                
                // Force update material
                Material currentMat = playerRenderers[i].material;
                playerRenderers[i].material = currentMat;
            }
        }
    }
    
    // Add OnDisable to ensure colors get reset when the object is disabled
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
        Debug.Log(gameObject.name + " has been eliminated!");
        OnDeath.Invoke(this.gameObject);
        BattleManager.Instance.OnPlayerDeath(this.gameObject);
    }
}