using UnityEngine;
using UnityEngine.AI; // Include if using NavMesh

// Controls the Boss behavior: Facing and optional Following.
public class BossController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 6f;
    public float detectionRange = 15f;
    [Tooltip("Use Unity's NavMesh system for pathfinding? Requires NavMeshAgent component.")]
    public bool useNavMesh = false; // Set true if using NavMesh

    [Header("Visual Settings")]
    [Tooltip("Adjust this if the model needs to be rotated to face correctly")]
    public float visualRotationOffset = 0f;

    [Header("State")]
    [SerializeField]
    private bool canFollow = false; // Starts FALSE

    // --- Component References ---
    private Transform playerTransform;
    private NavMeshAgent navAgent;
    private Rigidbody rb;
    private EnemyHealth enemyHealth;

    private bool isSetupComplete = false;
    private bool navMeshAvailable = false; // Track if NavMesh is usable

    void Awake() // Get components early
    {
        rb = GetComponent<Rigidbody>();
        navAgent = GetComponent<NavMeshAgent>(); // Try to get NavMeshAgent
        enemyHealth = GetComponent<EnemyHealth>();
    }

    void Start()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError($"[{gameObject.name}] Player object not found! Disabling BossController.", gameObject);
            enabled = false;
            return;
        }
        playerTransform = player.transform;

        if (enemyHealth == null)
        {
            Debug.LogWarning($"[{gameObject.name}] BossController did not find EnemyHealth script. Boss cannot become vulnerable.", gameObject);
        }

        // Configure Rigidbody if it exists
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.isKinematic = true; // Start kinematic (immovable)
        }

        // Setup NavMeshAgent if intended AND component exists
        if (useNavMesh)
        {
            if (navAgent == null)
            {
                // --- ERROR HANDLING ---
                Debug.LogError($"[{gameObject.name}] 'Use Nav Mesh' is checked, but no NavMeshAgent component found! Please add one or uncheck 'Use Nav Mesh'. Disabling NavMesh logic.", gameObject);
                navMeshAvailable = false; // Mark NavMesh as unavailable
                // --- END ERROR HANDLING ---
            }
            else // NavMeshAgent exists
            {
                navMeshAvailable = true; // Mark NavMesh as available
                navAgent.speed = moveSpeed;
                navAgent.angularSpeed = rotationSpeed * 100;
                navAgent.acceleration = 8f;
                navAgent.stoppingDistance = 1.5f;
                navAgent.enabled = false; // Start disabled (will be enabled in ActivateFollowing)

                // Ensure Rigidbody is kinematic if using NavMesh
                if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
                Debug.Log($"[{gameObject.name}] NavMeshAgent configured but initially disabled.", gameObject);
            }
        }
        else // Not using NavMesh
        {
            navMeshAvailable = false;
            if (navAgent != null)
            {
                navAgent.enabled = false; // Ensure disabled if not using it
            }
            // Ensure Rigidbody is ready if NavMesh is off (will be made non-kinematic later)
            if (rb != null)
            {
                 // Keep it kinematic until ActivateFollowing is called
                 rb.isKinematic = true;
                 rb.useGravity = false; // Gravity will be enabled if made non-kinematic
            }
        }

        // Collider check (kept from before)
        Collider col = GetComponent<Collider>();
        if (col == null) col = GetComponentInChildren<Collider>();
        if (col == null && rb != null && !rb.isKinematic)
        {
             Debug.LogError($"[{gameObject.name}] BossController has an active Rigidbody but no Collider!", gameObject);
        }

        isSetupComplete = true;
        Debug.Log($"[{gameObject.name}] BossController setup complete. Initial canFollow: {canFollow}, NavMesh Available: {navMeshAvailable}", gameObject);
    }

    void Update()
    {
        if (!isSetupComplete || playerTransform == null) return;

        // Always Face the Player
        FacePlayer();

        // Movement Logic (Only if canFollow is true)
        if (canFollow)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= detectionRange)
            {
                // --- Move Towards Player ---
                // Check if NavMesh should be used AND is available/enabled
                if (useNavMesh && navMeshAvailable && navAgent != null && navAgent.enabled)
                {
                    navAgent.SetDestination(playerTransform.position);
                }
                // Otherwise, use Rigidbody if available and not kinematic
                else if (rb != null && !rb.isKinematic)
                {
                    Vector3 direction = playerTransform.position - transform.position;
                    direction.y = 0;
                    direction.Normalize();
                    Vector3 targetVelocity = direction * moveSpeed;
                    targetVelocity.y = rb.linearVelocity.y;
                    rb.linearVelocity = targetVelocity;
                }
                // Fallback to transform movement if not using NavMesh and no suitable Rigidbody
                else if (!useNavMesh)
                {
                    Vector3 direction = playerTransform.position - transform.position;
                    direction.y = 0;
                    direction.Normalize();
                    transform.position += direction * moveSpeed * Time.deltaTime;
                }
            }
            else // Player is outside detection range (but following IS active)
            {
                // --- Stop Movement ---
                if (useNavMesh && navMeshAvailable && navAgent != null && navAgent.enabled)
                {
                    if (navAgent.hasPath) navAgent.ResetPath();
                }
                else if (rb != null && !rb.isKinematic)
                {
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                }
            }
        }
    }

    // Public function to enable following behavior AND disable invincibility
    public void ActivateFollowing()
    {
        if (!canFollow) // Only activate and log once
        {
            canFollow = true;
            Debug.Log($"[{gameObject.name}] Following behavior ACTIVATED.", gameObject);

            // --- MAKE MOVABLE ---
            // Enable NavMesh only if it's supposed to be used AND it exists
            if (useNavMesh && navMeshAvailable && navAgent != null)
            {
                navAgent.enabled = true;
                Debug.Log($"[{gameObject.name}] NavMeshAgent enabled.", gameObject);
            }
            // Otherwise, make Rigidbody non-kinematic if it exists
            else if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true; // Enable gravity when physics takes over
                Debug.Log($"[{gameObject.name}] Rigidbody set to non-kinematic.", gameObject);
            }
            // --- END MAKE MOVABLE ---

            // --- MAKE VULNERABLE ---
            if (enemyHealth != null)
            {
                enemyHealth.BecomeVulnerable();
            }
            else { Debug.LogWarning($"[{gameObject.name}] Cannot make vulnerable: EnemyHealth reference missing."); }
            // --- END MAKE VULNERABLE ---
        }
    }

    // FacePlayer method (remains the same)
    private void FacePlayer()
    {
        if (playerTransform == null) return;
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            if (visualRotationOffset != 0) { lookRotation *= Quaternion.Euler(0, visualRotationOffset, 0); }
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // OnDrawGizmosSelected method (remains the same)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
} // End of class BossController
