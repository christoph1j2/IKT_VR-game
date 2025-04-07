using UnityEngine;
using UnityEngine.AI; // Include if using NavMesh

// Controls the Boss behavior: Facing and optional Following.
public class BossController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f; // Boss might be faster/slower than Smiler
    public float rotationSpeed = 6f; // Speed for turning
    public float detectionRange = 15f; // Range to start following (after activated)
    public bool useNavMesh = false; // Set true if using NavMesh

    [Header("Visual Settings")]
    [Tooltip("Adjust this if the model needs to be rotated to face correctly")]
    public float visualRotationOffset = 0f; // Adjust if needed

    [Header("State")]
    [SerializeField] // Show in inspector for debugging, but controlled by ActivateFollowing()
    private bool canFollow = false; // Starts as FALSE - only faces player initially

    // References
    private Transform playerTransform;
    private NavMeshAgent navAgent;
    private Rigidbody rb;

    private bool isSetupComplete = false;

    void Start()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError($"[{gameObject.name}] Player object not found! Make sure Player has the 'Player' tag. Disabling BossController.", gameObject);
            enabled = false;
            return;
        }
        playerTransform = player.transform;

        // Get Rigidbody component
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // Prevent tipping
        }

        // Setup NavMesh if enabled
        navAgent = GetComponent<NavMeshAgent>();
        if (useNavMesh)
        {
            if (navAgent == null)
            {
                Debug.LogWarning($"[{gameObject.name}] useNavMesh is true, but no NavMeshAgent component found. Adding one.", gameObject);
                navAgent = gameObject.AddComponent<NavMeshAgent>();
            }
            navAgent.speed = moveSpeed;
            navAgent.angularSpeed = rotationSpeed * 100;
            navAgent.acceleration = 8f; // Adjust as needed
            // Set stopping distance based on how close you want it to get
            navAgent.stoppingDistance = 1.5f; // Example stopping distance
            navAgent.enabled = true; // Enable NavMesh agent

            // Disable Rigidbody physics if using NavMesh
            if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
        }
        else if (navAgent != null)
        {
            // Ensure NavMeshAgent is disabled if not using it
            navAgent.enabled = false;
            // Ensure Rigidbody is active if NavMesh is off
            if (rb != null) { rb.isKinematic = false; rb.useGravity = true; }
        }

        // Check for collider if using Rigidbody physics
        Collider col = GetComponent<Collider>();
        if (col == null) col = GetComponentInChildren<Collider>();
        if (col == null && rb != null && !rb.isKinematic)
        {
             Debug.LogError($"[{gameObject.name}] BossController has an active Rigidbody but no Collider! Add one.", gameObject);
        }

        isSetupComplete = true;
        Debug.Log($"[{gameObject.name}] BossController setup complete. Initial canFollow state: {canFollow}", gameObject);
    }

    void Update()
    {
        if (!isSetupComplete || playerTransform == null) return;

        // --- Always Face the Player ---
        FacePlayer();

        // --- Movement Logic (Only if canFollow is true) ---
        if (canFollow)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= detectionRange)
            {
                // Move towards player
                if (useNavMesh && navAgent != null && navAgent.enabled)
                {
                    navAgent.SetDestination(playerTransform.position);
                }
                else if (rb != null && !rb.isKinematic) // Use Rigidbody movement
                {
                    Vector3 direction = playerTransform.position - transform.position;
                    direction.y = 0;
                    direction.Normalize();
                    Vector3 targetVelocity = direction * moveSpeed;
                    targetVelocity.y = rb.linearVelocity.y; // Preserve gravity
                    rb.linearVelocity = targetVelocity;
                }
                else if (!useNavMesh) // Fallback to transform movement
                {
                    Vector3 direction = playerTransform.position - transform.position;
                    direction.y = 0;
                    direction.Normalize();
                    transform.position += direction * moveSpeed * Time.deltaTime;
                }
            }
            else // Player is outside detection range (but following is active)
            {
                // Stop Movement
                if (useNavMesh && navAgent != null && navAgent.enabled)
                {
                    if (navAgent.hasPath) navAgent.ResetPath(); // Stop NavMesh agent
                }
                else if (rb != null && !rb.isKinematic)
                {
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); // Stop horizontal Rigidbody movement
                }
            }
        }
        // --- End Movement Logic ---
    }

    // --- Public function to enable following behavior ---
    public void ActivateFollowing()
    {
        if (!canFollow) // Only activate and log once
        {
            canFollow = true;
            Debug.Log($"[{gameObject.name}] Following behavior ACTIVATED.", gameObject);
            // Optional: Re-enable NavMesh Agent if it was disabled initially
            if (useNavMesh && navAgent != null)
            {
                navAgent.enabled = true;
            }
        }
    }

    // --- FacePlayer method ---
    private void FacePlayer()
    {
        if (playerTransform == null) return;

        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0; // Keep rotation horizontal

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            // Apply visual offset if needed
            if (visualRotationOffset != 0)
            {
                lookRotation *= Quaternion.Euler(0, visualRotationOffset, 0);
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // --- Optional: Visualize detection range ---
    private void OnDrawGizmosSelected()
    {
        // Only draw gizmo if following is potentially active
        // Or always draw it for setup purposes
        Gizmos.color = Color.magenta; // Use a different color for the boss
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
