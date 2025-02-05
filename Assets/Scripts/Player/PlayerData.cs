using System;
using UnityEngine;
using UnityEngine.Events;
using static GASHAPWN.GameManager;

public class PlayerData : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;

    public UnityEvent<int> OnDamage; // Broadcasts current health after taking damage

    public UnityEvent<int> SetMaxHealth; // Broadcasts max health

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Physics Floatiness")]
    public float drag = 0f;
    public float angularDrag = 0.05f;

    [Header("Physic Material (optional)")]
    public PhysicsMaterial sphereMaterial; 

    [Header("Air Control Settings")]
    public float airTorque = 5f;

    private void Start()
    {
        currentHealth = maxHealth;
        SetMaxHealth.Invoke(maxHealth);
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
}
