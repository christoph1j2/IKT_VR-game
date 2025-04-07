using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

// Manages the health of an enemy and handles its death sequence,
// including an optional disintegration effect. Can start invincible.
public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField]
    [Tooltip("Maximum and starting health of the enemy.")]
    private int maxHealth = 50; // Example Boss health

    [SerializeField]
    [Tooltip("Start the enemy in an invincible state?")]
    private bool startInvincible = false; // Set this TRUE for the Boss initially

    // Tracks the current health
    private int currentHealth;
    private bool isInvincible = false; // Current invincibility state

    [Header("Disintegration Effect")]
    [Tooltip("Should the enemy fall apart into pieces on death? Requires child GameObjects for pieces.")]
    [SerializeField]
    private bool fallApartOnDeath = true; // Maybe false for Boss? Set as needed.

    [Tooltip("Force applied to pieces when falling apart, originating from the enemy's center.")]
    [SerializeField]
    private float explosionForce = 50f;

    [Tooltip("Radius of the explosion force origin.")]
    [SerializeField]
    private float explosionRadius = 1.0f;

    [Tooltip("Delay in seconds after falling apart before pieces are destroyed.")]
    [SerializeField]
    private float pieceDestroyDelay = 2.0f; // Renamed for clarity

    [Header("Piece Physics (Optional Overrides)")]
    [Tooltip("Mass assigned to falling pieces (lower values move more easily).")]
    [SerializeField] private float pieceMass = 0.5f;
    [Tooltip("Drag assigned to falling pieces (higher values slow down faster).")]
    [SerializeField] private float pieceDrag = 0.5f;


    // Optional: Event to trigger other actions on death (e.g., score update)
    // public UnityEvent OnEnemyDied;

    void Awake() // Use Awake to set initial invincibility before Start
    {
        isInvincible = startInvincible; // Set initial state based on Inspector
    }

    void Start()
    {
        currentHealth = maxHealth; // Initialize health

        if (!gameObject.CompareTag("Enemy"))
        {
            Debug.LogWarning($"Object '{gameObject.name}' has EnemyHealth script but is not tagged 'Enemy'. Please add the tag.", gameObject);
        }
        Debug.Log($"[{gameObject.name}] EnemyHealth Start. Initial Invincible State: {isInvincible}");
    }

    // Public method to allow other scripts to deal damage to this enemy
    public void TakeDamage(int damageAmount)
    {
        // Ignore damage if invincible
        if (isInvincible)
        {
            // Debug.Log($"[{gameObject.name}] Damage blocked: Currently invincible."); // Optional log
            return; // Ignore damage if invincible
        }

        if (currentHealth <= 0) return; // Already dead

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Debug.Log($"Enemy '{gameObject.name}' took {damageAmount} damage. Current health: {currentHealth}/{maxHealth}", gameObject); // Optional log

        if (currentHealth <= 0)
        {
            Die(); // Trigger the death sequence
        }
    }

    // Public method to disable invincibility (called by BossController)
    public void BecomeVulnerable()
    {
        if (isInvincible) // Only change and log if it was previously invincible
        {
            isInvincible = false;
            Debug.Log($"[{gameObject.name}] Is no longer invincible.", gameObject);
        }
    }

    // Handles the death sequence
    private void Die()
    {
        // Log death event
        Debug.Log($"Enemy '{gameObject.name}' has died!", gameObject);

        // Optional: Trigger death event for other systems
        // OnEnemyDied?.Invoke();

        // --- Disintegration Logic ---
        if (fallApartOnDeath)
        {
            // 1. Disable main colliders on the root object
            foreach(Collider col in GetComponents<Collider>()) {
                if (col != null) col.enabled = false;
            }

            // --- REMOVED component disabling lines ---
            // GetComponent<BossController>()?.enabled = false; // REMOVED
            // GetComponent<EnemyMeleeDamage>()?.enabled = false; // REMOVED
            // Add any other custom AI/Movement scripts here if needed

            // Stop physics/NavMesh on the root object
            Rigidbody mainRb = GetComponent<Rigidbody>();
            if(mainRb != null) mainRb.isKinematic = true; // Make kinematic to stop external forces
            UnityEngine.AI.NavMeshAgent mainAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if(mainAgent != null) mainAgent.enabled = false; // Disable NavMesh agent

            // Optionally hide the root renderer if it exists and isn't meant to be a piece
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
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>(true); // Include inactive

            foreach (Renderer rend in allRenderers)
            {
                // Exclude the root object's renderer if it exists and shouldn't be a piece
                if (rend.gameObject == this.gameObject) continue;
                piecesToProcess.Add(rend.transform);
                // Temporarily disable renderer - will be re-enabled on the piece
                rend.enabled = false;
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
                pieceRb.mass = pieceMass; // Use inspector values
                pieceRb.linearDamping = pieceDrag; // Use inspector values
                pieceRb.isKinematic = false;
                pieceRb.useGravity = true;
                // Apply explosion force relative to the original enemy's center
                pieceRb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0.0f, ForceMode.Impulse); // Using Impulse

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

                // Destroy piece after delay
                Destroy(piece.gameObject, pieceDestroyDelay);
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
