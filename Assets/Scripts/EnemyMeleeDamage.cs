using UnityEngine;
using System.Collections.Generic; // Still needed for dictionary if using OnTriggerStay approach later

// Deals damage periodically to the Player while they stay inside the trigger zone.
// First hit is immediate on enter.
// Attach this script to the Enemy GameObject.
public class EnemyMeleeDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField]
    [Tooltip("Amount of damage dealt per hit/tick.")]
    private int damageAmount = 10;

    [SerializeField]
    [Tooltip("Minimum time in seconds between subsequent damage ticks while player stays in range.")]
    private float damageInterval = 1.5f;

    [Tooltip("Enable or disable this enemy's melee damage.")]
    public bool damageEnabled = true; // Public checkbox to enable/disable in Inspector

    [Header("Audio")]
    [SerializeField] private AudioClip damageSound; // Sound to play when hitting player
    private AudioSource audioSource;

    // Timer & State
    private float timeSinceLastDamage = 0f;
    private bool playerInRange = false; // Track if player is currently inside the trigger

    // --- Component References ---
    private Rigidbody rb;
    private Collider triggerCollider; // Specific reference to the trigger collider

    // --- IMPORTANT SETUP NOTE ---
    // Requires AT LEAST ONE Collider component on this GameObject with "Is Trigger" checked.
    // Requires a Rigidbody component (can be Kinematic).
    // Player needs "Player" tag and PlayerHealth script.
    // --- END NOTE ---

    // Awake runs before Start, used for component checks and references
    private void Awake()
    {
        // --- Setup Checks (Logs removed, but logic kept) ---
        rb = GetComponent<Rigidbody>();
        Collider[] allColliders = GetComponents<Collider>();
        triggerCollider = null;

        foreach (Collider currentCollider in allColliders)
        {
            if (currentCollider.isTrigger)
            {
                triggerCollider = currentCollider;
                break;
            }
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) { 
            // Auto-add AudioSource if missing
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        bool setupError = false;
        if (rb == null) { setupError = true; /* Debug.LogError("Rigidbody missing..."); */ }
        if (triggerCollider == null) { setupError = true; /* Debug.LogError("Trigger Collider missing..."); */ }

        if (setupError)
        {
            // Debug.LogError("Disabling EnemyMeleeDamage due to setup errors...");
            damageEnabled = false;
            enabled = false;
        }
        // --- End Setup Checks ---
    }


    private void Start()
    {
        // Initialize timer to 0, first hit will be handled by OnTriggerEnter
        timeSinceLastDamage = 0f;
    }


    // Update is called once per frame
    private void Update()
    {
        // Increment timer only if the player is known to be in range AND damage is enabled
        // This timer is now primarily for the *subsequent* hits in OnTriggerStay
        if (playerInRange && damageEnabled)
        {
            timeSinceLastDamage += Time.deltaTime;
        }
    }


    // Called continuously while another Collider stays within this trigger
    private void OnTriggerStay(Collider other)
    {
        // Exit if disabled
        if (!damageEnabled) return;

        // Check if it's the player and if enough time has passed *since the last hit*
        if (other.CompareTag("Player"))
        {
            if(timeSinceLastDamage >= damageInterval)
            {
                // Attempt to get the PlayerHealth component
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // Deal subsequent damage
                    playerHealth.TakeDamage(damageAmount);
                    PlayDamageSound(); // Play damage sound
                    timeSinceLastDamage = 0f; // Reset timer after dealing damage
                }
                // else { Debug.LogWarning($"Player in trigger but has no PlayerHealth script!"); } // Optional Warning
            }
        }
    }

    // Called when another Collider enters this trigger
     private void OnTriggerEnter(Collider other)
     {
         // Exit if disabled
         if (!damageEnabled) return;

         if (other.CompareTag("Player"))
         {
             playerInRange = true;
             // --- Deal Immediate Damage on Enter ---
             PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
             if (playerHealth != null)
             {
                 // Deal first hit immediately
                 playerHealth.TakeDamage(damageAmount);
                    PlayDamageSound(); // Play damage sound
                 timeSinceLastDamage = 0f; // Reset timer AFTER the first hit
             }
             // else { Debug.LogWarning($"Player entered trigger but has no PlayerHealth script!"); } // Optional Warning
             // --- End Immediate Damage ---
         }
     }

    // Called when another Collider exits this trigger
    private void OnTriggerExit(Collider other)
    {
         // Exit if disabled
         if (!damageEnabled) return;

        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // Reset timer when player leaves, ready for next immediate hit on re-entry
            timeSinceLastDamage = 0f;
        }
    }

     // Optional: Visualize the trigger collider in the editor
    private void OnDrawGizmos()
    {
        // Use the specific triggerCollider found in Awake
        if (!enabled || !damageEnabled || triggerCollider == null) return;

        Gizmos.color = new Color(1, 0, 0, 0.3f); // Red, semi-transparent

        if (triggerCollider is BoxCollider box)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (triggerCollider is SphereCollider sphere)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawSphere(sphere.center, sphere.radius);
        }
        else if (triggerCollider is CapsuleCollider capsule)
        {
             Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
             Gizmos.DrawWireSphere(capsule.center, capsule.radius);
        }
         Gizmos.matrix = Matrix4x4.identity; // Reset matrix
    }

    private void PlayDamageSound()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
    }

} // End of class
