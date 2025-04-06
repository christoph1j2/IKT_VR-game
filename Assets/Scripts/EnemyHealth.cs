using UnityEngine;
using UnityEngine.Events; // Optional: if you want events later

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 50; // Example starting health
    [SerializeField] private bool destroyOnDeath = true; // Option to destroy object on death
    private int currentHealth;

    // Optional: Event if needed later
    // public UnityEvent OnEnemyDied;

    void Start()
    {
        currentHealth = maxHealth;
        // Ensure the object has the "Enemy" tag if this script is present
        if (!gameObject.CompareTag("Enemy"))
        {
            Debug.LogWarning($"Object '{gameObject.name}' has EnemyHealth script but is not tagged 'Enemy'. Please add the tag.", gameObject);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return; // Already dead

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Prevent negative health

        Debug.Log($"Enemy '{gameObject.name}' took {damageAmount} damage. Current health: {currentHealth}/{maxHealth}", gameObject);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"Enemy '{gameObject.name}' has died!", gameObject);

        // Optional: Trigger death event
        // OnEnemyDied?.Invoke();

        if (destroyOnDeath)
        {
            // Simple death: destroy the enemy GameObject
            Destroy(gameObject);
        }
        else
        {
            // Alternative: disable components, play animation, etc.
            // For example:
            // GetComponent<Collider>().enabled = false;
            // GetComponent<Rigidbody>()?.Sleep(); // Stop physics
            // GetComponent<YourEnemyAI>()?.enabled = false; // Disable AI script
            gameObject.SetActive(false); // Simple disable
        }
    }

    // Allow checking health if needed
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}
