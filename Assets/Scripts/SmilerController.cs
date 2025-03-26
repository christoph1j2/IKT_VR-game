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
        
        // Only move toward the player when in detection range
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
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
                // Calculate movement in the correct direction (regardless of facing)
                Vector3 movementDirection = direction * moveSpeed * Time.deltaTime;
                
                // Apply movement directly toward player (ignoring where the model faces)
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
        
        // The model rotation is separate from the movement direction
        // Rotate the character to face the player with offset
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        lookRotation *= Quaternion.Euler(0, visualRotationOffset, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
