using System;
using UnityEngine;
using UnityEngine.Events;
using static GASHAPWN.GameManager;

public class PlayerData : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Events")]

    public UnityEvent<int> OnDamage; // Broadcasts current health after taking damage

    public UnityEvent<int> SetMaxHealth; // Broadcasts max health

    public UnityEvent<GameObject> OnDeath;

    [Header("Player State Flags")]
    public bool isGrounded = false;
    public bool controlsEnabled = true;
    public bool hasSlammed = false;

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

    [Header("Slam Settings")]
    public float slamForce = 80f;
    public float slamDelay = .4f;

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
        OnDeath.Invoke(this.gameObject);
        // Handle player defeat logic here (disable player, trigger animations, etc.)
    }
}
