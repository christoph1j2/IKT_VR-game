using UnityEngine;
using UnityEngine.AI;

public class SmilerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f; // Speed for turning
    public float detectionRange = 10f;
    public bool useNavMesh = false;

    [Header("Visual Settings")]
    [Tooltip("Adjust this if the model needs to be rotated to face correctly")]
    public float visualRotationOffset = 90f;

    [Header("Arm Animation")]
    public Transform leftArm;
    public Transform rightArm;
    public float armRaiseSpeed = 5f;

    [Tooltip("Z rotation for raised left arm")]
    public float leftArmRaisedRotationZ = -80.729f;

    [Tooltip("Z rotation for raised right arm")]
    public float rightArmRaisedRotationZ = 80.729f; // Mirror of left arm, adjust as needed

    // Store initial arm rotations
    private Quaternion leftArmInitialRotation;
    private Quaternion rightArmInitialRotation;
    private Quaternion leftArmRaisedRotation;
    private Quaternion rightArmRaisedRotation;

    // Track if arms are raised
    private bool armsRaised = false;

    [Header("References")]
    private Transform playerTransform;
    private NavMeshAgent navAgent;
    private Rigidbody rb;

    private bool isSetupComplete = false; // Flag to ensure setup runs correctly

    private void Start()
    {
        Debug.Log($"[{gameObject.name}] Smiler Start() called.");

        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError($"[{gameObject.name}] Player object not found! Make sure Player has the 'Player' tag. Disabling Smiler.", gameObject);
            enabled = false;
            return;
        }
        playerTransform = player.transform;

        // Store initial arm rotations
        if (leftArm != null)
        {
            leftArmInitialRotation = leftArm.localRotation;
            leftArmRaisedRotation = Quaternion.Euler(0, 0, leftArmRaisedRotationZ);
        }
        if (rightArm != null)
        {
            rightArmInitialRotation = rightArm.localRotation;
            rightArmRaisedRotation = Quaternion.Euler(0, 0, rightArmRaisedRotationZ);
        }

        // Get rigidbody if exists
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
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
            navAgent.acceleration = 8f;
            // Set stopping distance based on how close you want it to get before stopping movement
            navAgent.stoppingDistance = 1.0f; // Adjust as needed
            navAgent.enabled = true;

            if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }
        }
        else if (navAgent != null)
        {
            navAgent.enabled = false;
            if (rb != null) { rb.isKinematic = false; rb.useGravity = true; }
        }

        // Check for collider if using Rigidbody physics
        Collider col = GetComponent<Collider>();
        if (col == null) col = GetComponentInChildren<Collider>();
        if (col == null && rb != null && !rb.isKinematic)
        {
             Debug.LogError($"[{gameObject.name}] Smiler has an active Rigidbody but no Collider! Add one for physics interactions.", gameObject);
        }

        isSetupComplete = true;
        Debug.Log($"[{gameObject.name}] Smiler setup complete.", gameObject);
    }

    private void Update()
    {
        if (!isSetupComplete || playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        FacePlayer();
        UpdateArmPositions(distanceToPlayer <= detectionRange);

        // Movement Logic (only based on detection range now)
        if (distanceToPlayer <= detectionRange)
        {
            // Move towards player
            if (useNavMesh && navAgent != null && navAgent.enabled)
            {
                navAgent.SetDestination(playerTransform.position);
            }
            else if (rb != null && !rb.isKinematic)
            {
                Vector3 direction = playerTransform.position - transform.position;
                direction.y = 0;
                direction.Normalize();
                Vector3 targetVelocity = direction * moveSpeed;
                targetVelocity.y = rb.linearVelocity.y;
                rb.linearVelocity = targetVelocity;
            }
            else if (!useNavMesh)
            {
                Vector3 direction = playerTransform.position - transform.position;
                direction.y = 0;
                direction.Normalize();
                transform.position += direction * moveSpeed * Time.deltaTime;
            }
        }
        else // Player is outside detection range
        {
            // Stop Movement
            if (useNavMesh && navAgent != null && navAgent.enabled)
            {
                if (navAgent.hasPath) navAgent.ResetPath();
            }
            else if (rb != null && !rb.isKinematic)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            }
        }
    }

    private void FacePlayer()
    {
        if (playerTransform == null) return;
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            lookRotation *= Quaternion.Euler(0, visualRotationOffset, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void UpdateArmPositions(bool playerInRange)
    {
        if (leftArm == null || rightArm == null) return;
        Quaternion targetLeftRot = playerInRange ? leftArmRaisedRotation : leftArmInitialRotation;
        Quaternion targetRightRot = playerInRange ? rightArmRaisedRotation : rightArmInitialRotation;
        leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, targetLeftRot, Time.deltaTime * armRaiseSpeed);
        rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, targetRightRot, Time.deltaTime * armRaiseSpeed);
        armsRaised = playerInRange;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; // Only draw detection range now
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
