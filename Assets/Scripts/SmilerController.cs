using UnityEngine;
using UnityEngine.AI;

public class SmilerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;
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
            Debug.LogError("Player not found! Make sure Player has the 'Player' tag");
        }
        
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
        if (useNavMesh)
        {
            navAgent = GetComponent<NavMeshAgent>();
            if (navAgent == null)
            {
                navAgent = gameObject.AddComponent<NavMeshAgent>();
                navAgent.speed = moveSpeed;
                navAgent.angularSpeed = rotationSpeed * 100;
                navAgent.acceleration = 8f;
                navAgent.stoppingDistance = 1f;
            }
        }
        
        // Setup collider to ignore other enemies
        Physics.IgnoreLayerCollision(gameObject.layer, gameObject.layer, true);
    }
    
    private void Update()
    {
        if (playerTransform == null) return;
        
        // Always face the player regardless of distance
        FacePlayer();
        
        // Check distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Handle arm animation based on distance
        UpdateArmPositions(distanceToPlayer <= detectionRange);
        
        // Only move toward the player when in detection range
        if (distanceToPlayer <= detectionRange)
        {
            // Calculate direction to player
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            
            if (useNavMesh && navAgent != null)
            {
                // NavMesh movement
                navAgent.SetDestination(playerTransform.position);
            }
            else
            {
                // Calculate movement in the correct direction
                Vector3 movementDirection = direction * moveSpeed * Time.deltaTime;
                
                // Apply movement directly toward player
                if (rb != null)
                {
                    // Rigidbody movement - maintain Y velocity for gravity
                    Vector3 velocity = new Vector3(direction.x * moveSpeed, rb.linearVelocity.y, direction.z * moveSpeed);
                    rb.linearVelocity = velocity;
                }
                else
                {
                    // Transform-based movement
                    transform.position += movementDirection;
                }
            }
        }
        else if (rb != null)
        {
            // When not in range, maintain vertical velocity but zero out horizontal
            Vector3 velocity = new Vector3(0, rb.linearVelocity.y, 0);
            rb.linearVelocity = velocity;
        }
    }
    
    private void FacePlayer()
    {
        // Calculate direction to player
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        
        // Rotate the character to face the player with offset
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        lookRotation *= Quaternion.Euler(0, visualRotationOffset, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
    
    private void UpdateArmPositions(bool playerInRange)
    {
        // No arms assigned, nothing to do
        if (leftArm == null || rightArm == null) return;
        
        if (playerInRange && !armsRaised)
        {
            // Raise arms when player enters range
            leftArm.localRotation = Quaternion.Slerp(
                leftArm.localRotation,
                leftArmRaisedRotation,
                Time.deltaTime * armRaiseSpeed
            );
            
            rightArm.localRotation = Quaternion.Slerp(
                rightArm.localRotation,
                rightArmRaisedRotation,
                Time.deltaTime * armRaiseSpeed
            );
            
            // Check if arms are nearly in raised position
            if (Quaternion.Angle(leftArm.localRotation, leftArmRaisedRotation) < 0.1f)
            {
                armsRaised = true;
            }
        }
        else if (!playerInRange && armsRaised)
        {
            // Lower arms when player exits range
            leftArm.localRotation = Quaternion.Slerp(
                leftArm.localRotation,
                leftArmInitialRotation,
                Time.deltaTime * armRaiseSpeed
            );
            
            rightArm.localRotation = Quaternion.Slerp(
                rightArm.localRotation,
                rightArmInitialRotation, 
                Time.deltaTime * armRaiseSpeed
            );
            
            // Check if arms are nearly in lowered position
            if (Quaternion.Angle(leftArm.localRotation, leftArmInitialRotation) < 0.1f)
            {
                armsRaised = false;
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
