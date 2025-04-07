using UnityEngine;
using TMPro; // Use TextMeshPro namespace
// using UnityEngine.UI; // Only needed if using standard UI Text, not TMP

public class HealthUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI healthText; // Assign your TextMeshPro object here

    private PlayerHealth playerHealth;

    void Start()
    {
        // Find the PlayerHealth script in the scene using the newer recommended method
        // playerHealth = FindObjectOfType<PlayerHealth>(); // Old, obsolete method
        playerHealth = FindFirstObjectByType<PlayerHealth>(); // <-- MODIFIED LINE: Use newer method

        if (playerHealth != null)
        {
            // Subscribe to the health changed event
            playerHealth.OnHealthChanged.AddListener(UpdateHealthText);

            // Update text immediately with starting health
            UpdateHealthText(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
            Debug.Log("HealthUI successfully found PlayerHealth and subscribed to events."); // Added confirmation log
        }
        else
        {
            // This error means no active GameObject with PlayerHealth script was found
            Debug.LogError("HealthUI could not find an active PlayerHealth script in the scene!");
            if (healthText != null) healthText.text = "Health: Error";
        }

        // Check if the TextMeshPro component was assigned in the Inspector
        if (healthText == null)
        {
            Debug.LogError("Health Text (TMP) reference is not assigned in the HealthUI script's Inspector!", this.gameObject);
        }
    }

    // This method is called by the PlayerHealth event whenever health changes
    private void UpdateHealthText(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            // Format the text display (e.g., "Health: 80 / 100")
            healthText.text = $"Health: {currentHealth} / {maxHealth}";
        }
        else
        {
            // Log error if text component becomes null unexpectedly
            Debug.LogError("Health Text (TMP) reference became null in UpdateHealthText!", this.gameObject);
        }
    }

    // IMPORTANT: Unsubscribe from the event when the UI object is destroyed
    // This prevents errors if the player dies/is destroyed before the UI manager
    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            // Remove the listener to avoid memory leaks or errors
            playerHealth.OnHealthChanged.RemoveListener(UpdateHealthText);
            // Debug.Log("HealthUI unsubscribed from PlayerHealth events."); // Optional log
        }
    }
}
