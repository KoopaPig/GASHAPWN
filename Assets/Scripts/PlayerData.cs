using UnityEngine;
using UnityEngine.Events;

public class PlayerData : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;

    public UnityEvent<int> OnDamage; // Broadcasts current health after taking damage

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
