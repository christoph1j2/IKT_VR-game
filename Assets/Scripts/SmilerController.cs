using UnityEngine;
using UnityEngine.AI; // Required for NavMesh

public class SmilerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;
    public float detectionRange = 10f;
    public bool useNavMesh = false; // Set true if using NavMesh
    
    [Header("References")]
    private Transform playerTransform;
    private NavMeshAgent navAgent;
    
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
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            if (useNavMesh && navAgent != null)
            {
                // NavMesh movement (more advanced, handles obstacles)
                navAgent.SetDestination(playerTransform.position);
            }
            else
            {
                // Simple direct movement
                // Calculate direction to player
                Vector3 direction = (playerTransform.position - transform.position).normalized;
                
                // Rotate towards player
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
                
                // Move towards player
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
            }
        }
    }
    
    // Optional: visualize detection range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
