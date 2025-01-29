using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;

    public UnityEvent<int> OnDamage; // Broadcasts current health after taking damage

    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Physics Floatiness")]
    public float drag = 0f;
    public float angularDrag = 0.05f;

    [Header("Physic Material (optional)")]
    public PhysicsMaterial sphereMaterial; 

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage()
    {
        currentHealth--;

        OnDamage.Invoke(currentHealth);

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
