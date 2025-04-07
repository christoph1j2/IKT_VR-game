using UnityEngine;
using UnityEngine.Events; // Needed for UnityEvent
using UnityEngine.SceneManagement; // Needed for reloading scene
using System.Collections; // Needed for Coroutine

// Manages player health and triggers death sequence (fade/reload).
// Also triggers initial scene fade-in.
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField]
    [Tooltip("Maximum and starting health.")]
    private int maxHealth = 100;

    [SerializeField]
    [Tooltip("Check this box to prevent the player from taking damage.")]
    private bool isInvincible = false;

    // Tracks current health
    private int currentHealth;

    // Event to notify other scripts (like UI) when health changes
    // Passes current health and max health
    public UnityEvent<int, int> OnHealthChanged;

    // Optional: Add a reference to a movement script to disable on death
    // [SerializeField] private YourVRMovementScript vrMovement; // Example

    void Start()
    {
        currentHealth = maxHealth;
        // Trigger the event initially to show starting health on UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"Player health initialized: {currentHealth}/{maxHealth}");

        // --- ADDED SECTION FOR FADE-IN ---
        // Ensure ScreenFader exists and fade IN from black (to alpha 0)
        // This relies on ScreenFader running its Awake() first (check Script Execution Order)
        if (ScreenFader.Instance != null)
        {
            Debug.Log("PlayerHealth.Start: Calling ScreenFader FadeIn (FadeToColor Black, Alpha 0)");
            // Fade TO Alpha 0 (transparent) using Black as the base color over 1 second (default duration)
            ScreenFader.Instance.FadeToColor(Color.black, 0f, 1.0f);
            // Alternatively, if FadeScreen image is already black and opaque:
            // ScreenFader.Instance.FadeAlpha(0f, 1.0f);
        }
        else
        {
            // This might happen if Script Execution Order is wrong or ScreenFader object is inactive
            Debug.LogWarning("PlayerHealth.Start: ScreenFader.Instance was null. Cannot perform fade-in.");
        }
        // --- END ADDED SECTION ---
    }

    // Method called by enemies or hazards to deal damage
    public void TakeDamage(int damageAmount)
    {
        // Ignore damage if invincible
        if (isInvincible)
        {
            // Debug.Log("Player is invincible. Damage ignored."); // Optional debug
            return; // Exit the method immediately
        }

        // Ignore damage if already dead
        if (currentHealth <= 0) return;

        // Apply damage
        currentHealth -= damageAmount;
        // Clamp health between 0 and maxHealth
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player took {damageAmount} damage. Current health: {currentHealth}/{maxHealth}");

        // Notify listeners (like UI) that health changed
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Check if player died
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Handles triggering the player's death sequence
    private void Die()
    {
        Debug.Log("Player has died!");
        // --- Trigger Death Sequence Coroutine ---
        // Optional: Disable player controls immediately here if needed
        // var playerController = GetComponent<YourVRControllerScript>();
        // if(playerController != null) playerController.enabled = false;

        // Start the coroutine that handles fading and reloading
        StartCoroutine(DeathSequenceCoroutine());
        // --- End Trigger ---
    }

    // --- Coroutine for death sequence (fade out and reload) ---
    private IEnumerator DeathSequenceCoroutine()
    {
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.SetIsDeathFade(true);
            Debug.Log("Starting fade to black for death...");
            yield return ScreenFader.Instance.FadeToColor(Color.black, 1f, 1.0f); // Fade over 1 sec
            Debug.Log("Fade complete. Showing death screen.");
        }
        else
        {
            Debug.LogError("ScreenFader not found. Reloading immediately.");
        }

        yield return new WaitForSeconds(2f); // Delay after showing "You Died"

        // Reload scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    // --- END Coroutine ---


    // Optional: Add a way to heal the player
    public void Heal(int healAmount)
    {
        // Can't heal if already dead
        if (currentHealth <= 0) return;

        // Add health
        currentHealth += healAmount;
        // Clamp health to maxHealth
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Player healed {healAmount}. Current health: {currentHealth}/{maxHealth}");

        // Notify listeners (like UI) that health changed
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Allow other scripts to check current health if needed
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // Allow other scripts to check max health if needed
    public int GetMaxHealth()
    {
        return maxHealth;
    }
} // End of class PlayerHealth
