using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

// Manages the health of an enemy and handles its death sequence,
// including an optional disintegration effect (fall apart only).
public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField]
    [Tooltip("Maximum and starting health of the enemy.")]
    private int maxHealth = 50;

    // Tracks the current health
    private int currentHealth;

    [Header("Disintegration Effect")]
    [Tooltip("Should the enemy fall apart into pieces on death? Requires child GameObjects for pieces.")]
    [SerializeField]
    private bool fallApartOnDeath = true;

    // Removed Explosion Force/Radius

    [Tooltip("Delay in seconds after falling apart before pieces are destroyed.")]
    [SerializeField]
    private float pieceDestroyDelay = 2.0f; // Renamed for clarity, default 2 seconds

    // Removed Fade Duration

    // Optional: Event to trigger other actions on death (e.g., score update)
    // public UnityEvent OnEnemyDied;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth; // Initialize health

        // Check if the GameObject has the correct tag
        if (!gameObject.CompareTag("Enemy"))
        {
            Debug.LogWarning($"Object '{gameObject.name}' has EnemyHealth script but is not tagged 'Enemy'. Please add the tag for consistent behavior.", gameObject);
        }
    }

    // Public method to allow other scripts to deal damage to this enemy
    public void TakeDamage(int damageAmount)
    {
        // Ignore damage if already dead
        if (currentHealth <= 0) return;

        // Reduce health
        currentHealth -= damageAmount;
        // Ensure health doesn't go below zero
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Log damage taken (optional)
        // Debug.Log($"Enemy '{gameObject.name}' took {damageAmount} damage. Current health: {currentHealth}/{maxHealth}", gameObject);

        // Check if health has reached zero
        if (currentHealth <= 0)
        {
            Die(); // Trigger the death sequence
        }
    }

    // Handles the death sequence - Simplified to only fall apart and destroy pieces after delay
    private void Die()
    {
        // Log death event
        Debug.Log($"Enemy '{gameObject.name}' has died! Falling apart.", gameObject);

        // Optional: Trigger death event for other systems
        // OnEnemyDied?.Invoke();

        // --- Disintegration Logic (Simplified) ---
        if (fallApartOnDeath)
        {
            // 1. Disable main components on the root object
            foreach(Collider col in GetComponents<Collider>()) {
                if (col != null) col.enabled = false;
            }
            // Disable AI/Movement/Damage scripts
            var smilerController = GetComponent<SmilerController>();
            if (smilerController != null) smilerController.enabled = false;
            var starfishController = GetComponent<StarfishController>();
            if (starfishController != null) starfishController.enabled = false;
            var enemyMeleeDamage = GetComponent<EnemyMeleeDamage>();
            if (enemyMeleeDamage != null) enemyMeleeDamage.enabled = false;
            // Add any other custom AI/Movement scripts here

            Rigidbody mainRb = GetComponent<Rigidbody>();
            if(mainRb != null) mainRb.isKinematic = true; // Stop root physics
            UnityEngine.AI.NavMeshAgent mainAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if(mainAgent != null) mainAgent.enabled = false; // Disable NavMesh

            // Hide the root renderer if it exists and isn't a piece itself
            Renderer rootRenderer = GetComponent<Renderer>();
             if(rootRenderer != null)
             {
                 bool isChildRenderer = false;
                 foreach (Transform child in transform) {
                     if (child.GetComponent<Renderer>() == rootRenderer) {
                         isChildRenderer = true;
                         break;
                     }
                 }
                 if (!isChildRenderer) rootRenderer.enabled = false;
             }

            // Collect ALL relevant visual pieces' transforms
            List<Transform> piecesToProcess = new List<Transform>();
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true);

            foreach (Renderer rend in allRenderers)
            {
                if (rend.gameObject == this.gameObject) continue; // Skip root renderer
                piecesToProcess.Add(rend.transform);
                rend.enabled = false; // Disable temporarily
            }

            // 2. Process each collected piece
            Debug.Log($"[{gameObject.name}] Processing {piecesToProcess.Count} pieces for falling apart.");
            foreach (Transform piece in piecesToProcess)
            {
                 piece.gameObject.SetActive(true); // Ensure piece is active
                 piece.SetParent(null, true); // Unparent

                 // Re-enable its renderer now that it's separate
                 Renderer pieceRenderer = piece.GetComponent<Renderer>();
                 if(pieceRenderer != null) pieceRenderer.enabled = true;

                // Add/Configure Rigidbody for physics simulation
                Rigidbody pieceRb = piece.GetComponent<Rigidbody>();
                if (pieceRb == null) { pieceRb = piece.gameObject.AddComponent<Rigidbody>(); }
                // Use default mass/drag or values set on prefab piece if Rigidbody existed
                pieceRb.isKinematic = false;
                pieceRb.useGravity = true;
                // --- NO EXPLOSION FORCE ---

                // Ensure a Collider exists for ground/other collisions
                Collider pieceCol = piece.GetComponent<Collider>();
                if (pieceCol == null)
                {
                    piece.gameObject.AddComponent<BoxCollider>();
                }
                else
                {
                    pieceCol.enabled = true; // Ensure collider is enabled
                    pieceCol.isTrigger = false; // Ensure it's NOT a trigger
                }

                // --- DESTROY PIECE AFTER DELAY ---
                // Use the pieceDestroyDelay variable for how long pieces last
                Destroy(piece.gameObject, pieceDestroyDelay); // Destroy this piece after the delay
                // --- END DESTROY ---

                // --- REMOVED FadeAndDestroyPiece ---
            }

            // 3. Destroy the original root GameObject immediately
            Destroy(gameObject);
        }
        else // Fallback: If fallApartOnDeath is false
        {
            gameObject.SetActive(false); // Just disable
        }
        // --- End Disintegration Logic ---
    }


    // Public method to get current health
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    // Public method to get maximum health
    public int GetMaxHealth()
    {
        return maxHealth;
    }
} // End of class EnemyHealth
