using System;
using UnityEngine;
using UnityEngine.Events;
using static GASHAPWN.GameManager;

public class PlayerData : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;

    private Rigidbody rb;

    public UnityEvent<int> OnDamage; // Broadcasts current health after taking damage

    public UnityEvent<int> SetMaxHealth; // Broadcasts max health

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float minHitSpeed = 3.0f; // Speed threshold for valid hits
    public float deflectKnockbackMultiplier = 1.5f; // Knockback applied during deflection

    [Header("Physics Floatiness")]
    public float drag = 0f;
    public float angularDrag = 0.05f;

    [Header("Physic Material (optional)")]
    public PhysicsMaterial sphereMaterial; 

    private void Start()
    {
        currentHealth = maxHealth;
        SetMaxHealth.Invoke(maxHealth);
        rb = GetComponent<Rigidbody>(); // Glass vs Metal
    }

    public void TakeDamage(int damageAmt)
    {
        currentHealth -= damageAmt;

        OnDamage.Invoke(damageAmt);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has been eliminated!");
        // Handle player defeat logic here (disable player, trigger animations, etc.)
    }

    private void OnTriggerEnter(Collider other)
    {
        // Get the opponent's Rigidbody
        Rigidbody opponentRb = other.GetComponentInParent<Rigidbody>();
        if (opponentRb == null) return;

        // Calculate the impact speed
        float impactSpeed = opponentRb.linearVelocity.magnitude;
        if (impactSpeed < minHitSpeed)
        {
            Debug.Log("Too slow, no effect.");
            return; // Ignore hit if speed is below threshold
        }

        // Determine hitbox types (Glass or Metal) based on tags
        bool thisIsMetal = gameObject.CompareTag("Metal");
        bool thisIsGlass = gameObject.CompareTag("Glass");
        bool otherIsMetal = other.CompareTag("Metal");
        bool otherIsGlass = other.CompareTag("Glass");

        // Handle different hit types
        if (thisIsGlass && otherIsGlass)
        {
            Deflect(opponentRb);
            Debug.Log("Glass vs. Glass - Deflect!");
        }
        else if (thisIsMetal && otherIsMetal)
        {
            Deflect(opponentRb);
            Debug.Log("Metal vs. Metal - Deflect!");
        }
        else if (thisIsGlass && otherIsMetal) // Glass vs Metal: Damage is applied
        {
            TakeDamage(1);
            Knockback(opponentRb);
            Debug.Log(gameObject.name + " was hit by Metal!");
        }
    }

    // Method to handle deflection between two players
    private void Deflect(Rigidbody opponentRb)
    {
        // Calculate deflection direction and apply forces to both players
        Vector3 deflectDirection = (opponentRb.position - rb.position).normalized;
        rb.AddForce(deflectDirection * deflectKnockbackMultiplier, ForceMode.Impulse);
        opponentRb.AddForce(-deflectDirection * deflectKnockbackMultiplier, ForceMode.Impulse);
    }

    // Method to apply knockback when the player gets hit
    private void Knockback(Rigidbody opponentRb)
    {
        // Calculate knockback direction based on positions
        Vector3 knockbackDirection = (rb.position - opponentRb.position).normalized;
        rb.AddForce(knockbackDirection * 5f, ForceMode.Impulse);
    }
}
