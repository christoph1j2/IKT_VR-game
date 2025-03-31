using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public GameObject door;
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool autoClose = true;
    
    [Header("VR Settings")]
    [Tooltip("Tags that can interact with the door (add your hand tag)")]
    public string[] interactionTags = new string[] { "Player", "Hand", "VRController", "Controller" };
    [Tooltip("Enable this to see debug messages")]
    public bool showDebug = true;
    
    [Header("Enemy Spawning")]
    public bool spawnEnemiesOnOpen = true;
    public EnemySpawnManager spawnManager;
    
    // Door state tracking
    private bool isOpen = false;
    private bool isPlayerNear = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;
    
    // Spawn control
    private bool enemiesSpawned = false;
    private bool processingTrigger = false;
    
    private void Start()
    {
        // If no door is assigned, use the parent object
        if (door == null)
        {
            door = transform.parent.gameObject;
        }
        
        // Store the initial rotation
        initialRotation = door.transform.rotation;
        
        // Calculate the open rotation
        targetRotation = initialRotation * Quaternion.Euler(0, openAngle, 0);
        
        if (showDebug)
        {
            Debug.Log("Door Controller initialized. Looking for these interaction tags: " + string.Join(", ", interactionTags));
        }
    }
    
    private void Update()
    {
        // Handle door movement
        if (isPlayerNear && !isOpen)
        {
            // Open the door
            door.transform.rotation = Quaternion.Slerp(
                door.transform.rotation, 
                targetRotation, 
                Time.deltaTime * openSpeed
            );
            
            // Check if door is almost fully open
            if (Quaternion.Angle(door.transform.rotation, targetRotation) < 0.1f)
            {
                isOpen = true;
                if (showDebug) Debug.Log("Door fully opened");
            }
        }
        else if (!isPlayerNear && isOpen && autoClose)
        {
            // Close the door
            door.transform.rotation = Quaternion.Slerp(
                door.transform.rotation, 
                initialRotation, 
                Time.deltaTime * openSpeed
            );
            
            // Check if door is almost fully closed
            if (Quaternion.Angle(door.transform.rotation, initialRotation) < 0.1f)
            {
                isOpen = false;
                // Reset spawn status when door fully closes
                enemiesSpawned = false;
                if (showDebug) Debug.Log("Door closed, reset spawn state");
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Prevent multiple triggers from firing at the same time
        if (processingTrigger) return;
        
        // Lock to prevent concurrent processing
        processingTrigger = true;
        
        if (showDebug) Debug.Log($"Trigger entered by: {other.gameObject.name} with tag: {other.tag}");
        
        // Check if the object that entered has one of our valid interaction tags
        foreach (string tag in interactionTags)
        {
            if (other.CompareTag(tag))
            {
                if (showDebug) Debug.Log($"Valid interaction detected from: {other.gameObject.name} with tag: {other.tag}");
                isPlayerNear = true;
                
                // Only spawn enemies if door is closed and enemies haven't spawned yet
                if (spawnEnemiesOnOpen && spawnManager != null && !isOpen && !enemiesSpawned)
                {
                    Debug.Log("Triggering enemy spawn!");
                    spawnManager.SpawnEnemies();
                    enemiesSpawned = true;
                }
                
                // Found a match, no need to check other tags
                break;
            }
        }
        
        // Release the lock after processing
        processingTrigger = false;
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Check if the object that exited has one of our valid interaction tags
        foreach (string tag in interactionTags)
        {
            if (other.CompareTag(tag))
            {
                isPlayerNear = false;
                if (showDebug) Debug.Log($"Interaction object exited: {other.gameObject.name}");
                break;
            }
        }
    }
    
    // This will detect ALL collisions, not just triggers
    private void OnCollisionEnter(Collision collision)
    {
        if (showDebug) Debug.Log($"COLLISION with: {collision.gameObject.name}, tag: {collision.gameObject.tag}");
        
        // Check if the object that collided has one of our valid interaction tags
        foreach (string tag in interactionTags)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                isPlayerNear = true;
                
                // Only spawn enemies if door is closed and enemies haven't spawned yet
                if (spawnEnemiesOnOpen && spawnManager != null && !isOpen && !enemiesSpawned)
                {
                    Debug.Log("Triggering enemy spawn from collision!");
                    spawnManager.SpawnEnemies();
                    enemiesSpawned = true;
                }
                break;
            }
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        // Check if the object that exited collision has one of our valid interaction tags
        foreach (string tag in interactionTags)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                isPlayerNear = false;
                break;
            }
        }
    }
}
