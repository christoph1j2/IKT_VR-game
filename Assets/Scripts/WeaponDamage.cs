using System.Collections.Generic; // Needed for Dictionary
using UnityEngine;

// Deals damage periodically to "Enemy" tagged objects while its trigger stays overlapping.
// Attach to the player's weapon or attack trigger zone.
public class WeaponDamage : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField]
    [Tooltip("Amount of damage dealt per hit/tick.")]
    private int damageAmount = 15;

    [SerializeField]
    [Tooltip("Minimum time in seconds between damage ticks on the SAME enemy.")]
    private float damageInterval = 0.5f; // Cooldown between hits on the same enemy

    [Tooltip("Enable or disable the weapon's damage dealing functionality.")]
    public bool damageEnabled = true; // Public checkbox to enable/disable in Inspector

    // Tracks when each overlapping enemy can be damaged next
    private Dictionary<Collider, float> enemyNextDamageTime = new Dictionary<Collider, float>();

    // --- IMPORTANT SETUP NOTE ---
    // Uses OnTriggerStay. Collider on this GameObject MUST be "Is Trigger".
    // This GameObject (or parent) MUST have a Rigidbody (can be Kinematic).
    // Target objects need "Enemy" tag and EnemyHealth script.
    // --- END NOTE ---


    // --- DEBUGGING UPDATE REMOVED/COMMENTED ---
    // This Update method was previously added for debugging the damageEnabled flag.
    // It's commented out now to prevent console spam. Uncomment if needed again.
    /*
    private void Update()
    {
        // Log the current state of damageEnabled every frame IF it's false
        if (!damageEnabled)
        {
            Debug.LogWarning($"[{gameObject.name}] WeaponDamage - damageEnabled is currently FALSE in Update.", gameObject);
        }
    }
    */
    // --- END DEBUGGING UPDATE ---


    // Called every physics frame for each Collider other that is touching the trigger.
    private void OnTriggerStay(Collider other) // Changed from OnTriggerEnter for continuous damage
    {
        // Exit immediately if damage is disabled
        if (!damageEnabled) return;

        // Check if the object we are touching has the "Enemy" tag
        if (other.CompareTag("Enemy"))
        {
            // Check if enough time has passed to damage this specific enemy again
            if (CanDamageEnemy(other))
            {
                // Attempt to get the EnemyHealth component
                EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    // Debug.Log($"[{gameObject.name}] Dealing {damageAmount} damage to Enemy '{other.name}'.", other.gameObject); // Optional Debug
                    enemyHealth.TakeDamage(damageAmount);

                    // Record the time when this enemy can be damaged next
                    RecordDamageTime(other);
                }
                // else
                // {
                    // Debug.LogWarning($"[{gameObject.name}] Touching tagged Enemy '{other.name}' but it has no EnemyHealth script!", other.gameObject); // Optional Debug
                // }
            }
            // else { Debug.Log($"[{gameObject.name}] Still waiting for damage interval on {other.name}"); } // Optional debug
        }
    }

    // Optional: Clear dictionary entries when objects exit
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Remove the enemy from tracking when it leaves the trigger zone
            if (enemyNextDamageTime.ContainsKey(other))
            {
                enemyNextDamageTime.Remove(other);
                // Debug.Log($"[{gameObject.name}] Enemy '{other.name}' exited trigger, removed from damage tracking."); // Optional debug
            }
        }
    }


    // Helper function to check if the interval has passed for a specific enemy
    private bool CanDamageEnemy(Collider enemyCollider)
    {
        if (enemyNextDamageTime.TryGetValue(enemyCollider, out float nextTime))
        {
            return Time.time >= nextTime;
        }
        return true; // Can damage if not in dictionary (first time)
    }

    // Helper function to update the next damage time for an enemy
    private void RecordDamageTime(Collider enemyCollider)
    {
        enemyNextDamageTime[enemyCollider] = Time.time + damageInterval;
    }


    // Optional: Visualize the trigger collider in the editor
    private void OnDrawGizmos()
    {
        // Only draw gizmo if damage is enabled
        if (!damageEnabled) return;

        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger) // Check if collider exists and is a trigger
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f); // Cyan, semi-transparent

            // Draw based on collider type
            if (col is BoxCollider box)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
            else if (col is CapsuleCollider capsule)
            {
                 Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                 Gizmos.DrawWireSphere(capsule.center, capsule.radius); // Approximate
            }
            // Reset matrix after drawing to avoid affecting other gizmos
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
} // End of class WeaponDamage
