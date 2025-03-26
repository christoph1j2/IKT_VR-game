using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public GameObject door;
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool autoClose = true;
    
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
                Debug.Log("Door closed, reset spawn state");
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Prevent multiple triggers from firing at the same time
        if (processingTrigger) return;
        
        // Lock to prevent concurrent processing
        processingTrigger = true;
        
        // Check if the player entered the trigger
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            
            // Only spawn enemies if door is closed and enemies haven't spawned yet
            if (spawnEnemiesOnOpen && spawnManager != null && !isOpen && !enemiesSpawned)
            {
                Debug.Log("Triggering enemy spawn!");
                spawnManager.SpawnEnemies();
                enemiesSpawned = true;
            }
        }
        
        // Release the lock after processing
        processingTrigger = false;
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}
