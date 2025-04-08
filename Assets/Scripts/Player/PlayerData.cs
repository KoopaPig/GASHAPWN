using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerData : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Health & Stamina")]
    public int maxHealth = 5;
    private int currentHealth;
    public float maxStamina = 300;
    private float currentStamina;

    [Header("Events")]

    public UnityEvent<int> OnDamage = new UnityEvent<int>(); // Broadcasts current health after taking damage
    public UnityEvent<int> SetMaxHealth = new UnityEvent<int>(); // Broadcasts max health
    public UnityEvent<int> SetHealth = new UnityEvent<int>(); // Set health

    public UnityEvent<float> OnStaminaChanged = new UnityEvent<float>(); // Broadcast stamina value when changed
    public UnityEvent<float> SetMaxStamina = new UnityEvent<float>(); // Broadcasts maximum stamina
    public UnityEvent<float> OnStaminaHardDecrease = new UnityEvent<float>(); // Broadcast when some action instantly depletes stamina
    public UnityEvent<float> OnStaminaHardIncrease = new UnityEvent<float>(); // Broadcast when some action instantly refills stamina
    public UnityEvent<GameObject> OnDeath = new UnityEvent<GameObject>();

    [Header("Player State Flags")]
    public bool isGrounded = false;
    public bool controlsEnabled = true;
    public bool hasSlammed = false;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float minHitSpeed = 3f; // Speed threshold for valid hits
    public float deflectKnockbackMultiplier = 1.5f; // Knockback applied during deflection

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
<<<<<<< Updated upstream
=======
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
    private ImpactEffects impactEffects;


    // Multiple renderer support
    private Renderer[] playerRenderers;
    private Color[] originalColors;
>>>>>>> Stashed changes

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        SetMaxHealth.Invoke(maxHealth);
        SetMaxStamina.Invoke(maxStamina);
        rb = GetComponent<Rigidbody>();
<<<<<<< Updated upstream
=======
        impactEffects = GetComponent<ImpactEffects>();

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
>>>>>>> Stashed changes
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) // Assuming both players have the "Player" tag
        {
            Rigidbody otherRb = collision.rigidbody;
            if (otherRb == null) return;

            float relativeSpeed = (rb.linearVelocity - otherRb.linearVelocity).magnitude;

            if (relativeSpeed >= minHitSpeed)
            {
                Vector3 contactPoint = collision.GetContact(0).point;
                bool isMetalEnd = IsMetalEnd(contactPoint);
                bool isDeflecting = IsDeflecting(collision.contacts[0].normal);

                if (isMetalEnd)
                {
                    if (isDeflecting)
                    {
                        ApplyKnockback(otherRb, deflectKnockbackMultiplier);
                        Debug.Log("Deflect! Knockback applied.");
                        impactEffects.PlayDeflectEffect();
                    }
                    else
                    {
<<<<<<< Updated upstream
                        TakeDamage(1);
                        Debug.Log("Hit! Damage taken.");
=======
                        // Determine damage amount based on move type
                        int damageAmount = CalculateDamageAmount(otherPlayerData);
                        TakeDamage(damageAmount);
                        Debug.Log("Hit! Damage taken: " + damageAmount);
                        impactEffects.PlayDamageEffect();
>>>>>>> Stashed changes
                    }
                }
                else
                {
                    Debug.Log("No damage: Glass side hit or invalid contact.");
                }
            }
        }
    }

    private bool IsMetalEnd(Vector3 hitPoint)
    {
        return hitPoint.y < transform.position.y; // Example: Metal end is lower
    }

    private bool IsDeflecting(Vector3 hitNormal)
    {
        return Vector3.Dot(hitNormal, transform.forward) > 0.5f; // Example deflection logic
    }

    private void ApplyKnockback(Rigidbody otherRb, float multiplier)
    {
        Vector3 knockbackDirection = (otherRb.position - transform.position).normalized;
        otherRb.AddForce(knockbackDirection * moveSpeed * multiplier, ForceMode.Impulse);
    }

    public void TakeDamage(int damageAmt)
    {
        currentHealth -= damageAmt;
        OnDamage.Invoke(currentHealth);


        if (currentHealth <= 0)
        {
            Die();
        }
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
        gameObject.SetActive(false); // Disables the player
    }

}
