using System;
using System.Collections;
using UnityEngine;
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
    public float invincibilityDuration = 1.0f; // Default 1 second of i-frames
    public float damageReduction = 0f; // Percentage of damage reduction (0-1)
    public float defenseDuration = 0f; // Duration of active defense state
    private float defenseTimer = 0f; // Timer for tracking defense state

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
            
            // Deflection case: both players using offensive abilities or similar speeds
            bool shouldDeflect = (selfOffensive && otherOffensive) || 
                                (Mathf.Abs(rb.linearVelocity.magnitude - otherRb.linearVelocity.magnitude) < 2f);
            
            // Determine who has higher momentum (mass × velocity)
            bool hasHigherMomentum = rb.linearVelocity.magnitude > otherRb.linearVelocity.magnitude;
            
            if (relativeSpeed >= minHitSpeed)
            {
                Vector3 contactPoint = collision.GetContact(0).point;
                
                if (shouldDeflect)
                {
                    // Both deflect each other
                    Vector3 deflectionDir = (transform.position - collision.transform.position).normalized;
                    rb.AddForce(deflectionDir * deflectKnockbackMultiplier * relativeSpeed, ForceMode.Impulse);
                    otherRb.AddForce(-deflectionDir * deflectKnockbackMultiplier * relativeSpeed, ForceMode.Impulse);
                    
                    particleEffects?.PlayDeflectEffect(contactPoint);
                    GAME_SFXManager.Instance.Play_ImpactDeflect(transform);
                }
                else if (hasHigherMomentum)
                {
                    // Calculate damage based on speed and abilities
                    int damageAmount = CalculateDamageAmount(relativeSpeed, selfOffensive);
                    
                    otherPlayerData.TakeDamage(damageAmount);
                    particleEffects?.PlayHitEffect(contactPoint);
                    GAME_SFXManager.Instance.Play_ImpactGeneral(transform);
                }
            }
        }
    }

    // Calculate damage based on speed and offensive abilities
    private int CalculateDamageAmount(float relativeSpeed, bool isOffensiveAbility)
    {
        // Base damage from 0.5 to 1.25 based on speed
        float speedDamage = Mathf.Lerp(0.5f, 1.25f, Mathf.Clamp01((relativeSpeed - minHitSpeed) / 10f));
        
        // Offensive abilities deal fixed damage
        if (isOffensiveAbility)
        {
            if (hasSlammed)
                return slamDamage;
            else if (hasCharged)
                return chargeRollDamage;
            else if (isBursting)
                return 2; // Default offensive ability damage
        }
        
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
        StartCoroutine(DefenseVisualEffect(duration));
    }

    // Activate attack boost
    public void ActivateAttackBoost(float duration, float multiplier)
    {
        Debug.Log($"Attack boost activated on {gameObject.name} for {duration} seconds with {multiplier}x multiplier");
        
        damageMultiplier = multiplier;
        attackBonusTimer = duration;
        OnAttackBonusActivated.Invoke();
        StartCoroutine(AttackVisualEffect(duration));
    }

    // Used when performing burst move
    public void ActivateBurstInvincibility()
    {
        Debug.Log($"Burst invincibility activated on {gameObject.name}");
        
        isBursting = true;
        StartCoroutine(BurstInvincibility());
    }

    private IEnumerator BurstInvincibility()
    {
        isInvincible = true;
        Debug.Log(gameObject.name + " entered Burst Invincibility!");

        // Store reference to the coroutine
        Coroutine visualEffect = StartCoroutine(BurstVisualEffect());

        yield return new WaitForSeconds(burstInvincibilityDuration);

        isInvincible = false;
        isBursting = false;
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
        //Debug.Log($"Resetting all renderers to original colors on {gameObject.name}");
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
            {
                playerRenderers[i].material.color = originalColors[i];
                //Debug.Log($"Reset renderer {i} to color: {originalColors[i]}");
                
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