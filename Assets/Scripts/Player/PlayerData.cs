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

    private void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        SetMaxHealth.Invoke(maxHealth);
        SetMaxStamina.Invoke(maxStamina);
        rb = GetComponent<Rigidbody>();
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
                    }
                    else
                    {
                        TakeDamage(1);
                        Debug.Log("Hit! Damage taken.");
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
