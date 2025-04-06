using UnityEngine;
using UnityEngine.Events; // Needed for UnityEvent

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool isInvincible = false;
    private int currentHealth;

    // Event to notify other scripts (like UI) when health changes
    // Passes current health and max health
    public UnityEvent<int, int> OnHealthChanged;

    void Start()
    {
        currentHealth = maxHealth;
        // Trigger the event initially to show starting health
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"Player health initialized: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(int damageAmount)
    {
        // --- ADD THIS CHECK ---
        if (isInvincible)
        {
            Debug.Log("Player is invincible. Damage ignored."); // Optional debug message
            return; // Exit the method immediately, no damage taken
        }
        // --- END OF ADDED CHECK ---

        if (currentHealth <= 0) return; // Already dead

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player took {damageAmount} damage. Current health: {currentHealth}/{maxHealth}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        // Add death logic here later (e.g., game over screen, respawn)
        // For now, maybe just disable the player controller or the whole object
        // GetComponent<PlayerMovementScript>()?.enabled = false; // Replace with your actual movement script name
        gameObject.SetActive(false); // Simple way to stop interaction
    }

    // Optional: Add a way to heal
    public void Heal(int healAmount)
    {
        if (currentHealth <= 0) return; // Can't heal if dead

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Prevent health > maxHealth

        Debug.Log($"Player healed {healAmount}. Current health: {currentHealth}/{maxHealth}");

        // Trigger the event to update UI etc.
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Allow other scripts to check current health if needed
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
