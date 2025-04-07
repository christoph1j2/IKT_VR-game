using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StarfishController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    // public float rotationSpeed = 5f; // Removed as FacePlayer is removed
    public float detectionRange = 10f;
    public bool useNavMesh = false;

    [Header("Fall Settings")]
    public float fallSpeed = 5f; // Used if Rigidbody gravity is off
    public float fallRotationDuration = 0.5f; // Time to rotate 180 degrees

    [Header("Audio")]
    [SerializeField]
    [Tooltip("Sound played when starfish lands and starts following the player")]
    private AudioClip landingSound;
    private AudioSource audioSource;

    // States
    private enum StarfishState { Waiting, Falling, Following }
    private StarfishState currentState = StarfishState.Waiting;

    // References
    private Transform playerTransform;
    private Rigidbody rb;
    private NavMeshAgent navAgent;
    private Renderer[] renderers; // Keep for potential future effects

    // This function is called by the Spawner
    public void InitializeWithDelay(float delayBeforeFall)
    {
        StartCoroutine(DelayedFall(delayBeforeFall));
    }

    private void Awake()
    {
        // Get renderers
        renderers = GetComponentsInChildren<Renderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Check for a collider (important for landing)
        Collider col = GetComponent<Collider>();
        if (col == null) col = GetComponentInChildren<Collider>();
        if (col == null)
        {
            Debug.LogError("Starfish is missing a Collider component! Add one for collisions.", gameObject);
        }
    }

    private void Start()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure Player has the 'Player' tag", gameObject);
            enabled = false; // Disable script if no player
            return;
        }

        // Get or add Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Configure rigidbody for initial suspended state
        rb.useGravity = false;
        rb.isKinematic = true; // Start kinematic so it doesn't fall right away
        rb.freezeRotation = true; // Prevent unwanted rotation initially

        // Setup NavMesh if enabled
        if (useNavMesh)
        {
            navAgent = GetComponent<NavMeshAgent>();
            if (navAgent == null)
            {
                navAgent = gameObject.AddComponent<NavMeshAgent>();
                navAgent.speed = moveSpeed;
                // navAgent.angularSpeed = rotationSpeed * 100; // Rotation handled differently or not at all
                navAgent.acceleration = 8f;
                navAgent.stoppingDistance = 1f;
            }
            navAgent.enabled = false; // Disabled until landed

            // Disable Rigidbody physics if using NavMesh
             if (rb != null)
             {
                 rb.isKinematic = true;
                 rb.useGravity = false;
             }
        }
        else if (navAgent != null)
        {
            navAgent.enabled = false;
        }
    }

    private IEnumerator DelayedFall(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartFalling();
    }

    private void StartFalling()
    {
        currentState = StarfishState.Falling;

        // Configure rigidbody for falling
        if (rb != null)
        {
            rb.isKinematic = false; // Allow physics to take over
            rb.useGravity = true;  // Let gravity pull it down
            rb.freezeRotation = false; // Allow rotation during fall if needed, but we control it
        }

        // Start rotation coroutine
        StartCoroutine(RotateDuringFall());
    }

    private IEnumerator RotateDuringFall()
    {
        Quaternion startRotation = transform.rotation;
        // Rotate 180 degrees around the X-axis relative to current rotation
        Quaternion targetRotation = startRotation * Quaternion.Euler(180f, 0f, 0f);
        float elapsedTime = 0f;

        while (elapsedTime < fallRotationDuration)
        {
            // Lerp rotation smoothly
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / fallRotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        transform.rotation = targetRotation; // Ensure final rotation is exact
    }

    // Detect landing
    private void OnCollisionEnter(Collision collision)
    {
        // Check if it was falling and hit something (presumably the ground)
        if (currentState == StarfishState.Falling)
        {
            // Check if the collision object is potentially ground (optional, layer check is better)
            // For simplicity, we assume any collision while falling means landing.
            currentState = StarfishState.Following;

            PlaySound(landingSound);

            // Configure Rigidbody/NavMesh for following
            if (useNavMesh && navAgent != null)
            {
                navAgent.enabled = true;
                // Ensure Rigidbody doesn't interfere
                if(rb != null) {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
            }
            else if (rb != null)
            {
                // Landed, freeze X/Z rotation to prevent tipping
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
             Debug.Log("Starfish landed and started following.", gameObject);
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        switch (currentState)
        {
            case StarfishState.Waiting:
                // Do nothing
                break;

            case StarfishState.Falling:
                // Physics (gravity) handles the fall. Rotation is handled by RotateDuringFall coroutine.
                // Optional: Add manual downward force if needed:
                // if (rb != null) rb.AddForce(Vector3.down * fallSpeed, ForceMode.Acceleration);
                break;

            case StarfishState.Following:
                // Follow player logic
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

                if (distanceToPlayer <= detectionRange)
                {
                    // --- Movement ---
                    if (useNavMesh && navAgent != null && navAgent.enabled)
                    {
                        navAgent.SetDestination(playerTransform.position);
                    }
                    else // Use Rigidbody or Transform movement
                    {
                        Vector3 direction = playerTransform.position - transform.position;
                        direction.y = 0; // Keep movement horizontal
                        direction.Normalize();

                        if (rb != null && !rb.isKinematic)
                        {
                            // Rigidbody movement
                            Vector3 targetVelocity = direction * moveSpeed;
                            targetVelocity.y = rb.linearVelocity.y; // Preserve gravity/vertical velocity
                            rb.linearVelocity = targetVelocity;
                        }
                        else if (!useNavMesh) // Only use transform movement if not using NavMesh or Rigidbody physics
                        {
                            // Simple Transform-based movement
                            transform.position += direction * moveSpeed * Time.deltaTime;
                        }
                    }
                }
                else // Player is outside detection range
                {
                    // Stop movement
                    if (useNavMesh && navAgent != null && navAgent.enabled)
                    {
                        if (navAgent.hasPath) navAgent.ResetPath();
                    }
                    else if (rb != null && !rb.isKinematic)
                    {
                        // Stop horizontal Rigidbody movement
                        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                    }
                }
                break;
        }
    }

    // Removed FacePlayer() method

    // Optional: Visualize detection range in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
