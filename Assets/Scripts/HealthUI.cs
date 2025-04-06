using UnityEngine;
using TMPro; // Use TextMeshPro namespace

public class HealthUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI healthText; // Assign your TextMeshPro object here

    private PlayerHealth playerHealth;

    void Start()
    {
        // Find the PlayerHealth script in the scene
        playerHealth = FindObjectOfType<PlayerHealth>(); // Simple way to find it if there's only one player

        if (playerHealth != null)
        {
            // Subscribe to the health changed event
            playerHealth.OnHealthChanged.AddListener(UpdateHealthText);

            // Update text immediately with starting health
            UpdateHealthText(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
        }
        else
        {
            Debug.LogError("HealthUI could not find PlayerHealth script in the scene!");
            if (healthText != null) healthText.text = "Health: Error";
        }

        if (healthText == null)
        {
            Debug.LogError("Health Text (TMP) is not assigned in the HealthUI script!", this.gameObject);
        }
    }

    // This method is called by the PlayerHealth event
    private void UpdateHealthText(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            // Format the text display
            healthText.text = $"Health: {currentHealth} / {maxHealth}";
            // Or just: healthText.text = "Health: " + currentHealth;
        }
    }

    // IMPORTANT: Unsubscribe when the UI object is destroyed to prevent errors
    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(UpdateHealthText);
        }
    }
}
