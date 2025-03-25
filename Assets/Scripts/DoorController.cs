using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("The door GameObject that will rotate")]
    public GameObject door;
    
    [Tooltip("How far the door should open in degrees")]
    public float openAngle = 90f;
    
    [Tooltip("How fast the door opens")]
    public float openSpeed = 2f;
    
    [Tooltip("Should the door close after player leaves?")]
    public bool autoClose = true;
    
    [Header("Debug Options")]
    [Tooltip("Enable to test door opening without player interaction")]
    public bool testMode = true;
    
    // Door state tracking
    private bool isOpen = false;
    private bool isPlayerNear = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;
    
    private void Start()
    {
        // If no door is assigned, use the parent object
        if (door == null)
        {
            door = transform.parent.gameObject;
            Debug.Log("No door assigned, using parent: " + door.name);
        }
        else
        {
            Debug.Log("Using assigned door: " + door.name);
        }
        
        // Store the initial rotation
        initialRotation = door.transform.rotation;
        Debug.Log("Initial rotation set: " + initialRotation.eulerAngles);
        
        // Calculate the open rotation
        targetRotation = initialRotation * Quaternion.Euler(0, openAngle, 0);
        Debug.Log("Target rotation set: " + targetRotation.eulerAngles);
        
        // If test mode is enabled, simulate player being near
        if (testMode)
        {
            Debug.Log("TEST MODE ENABLED - Door will open automatically");
            isPlayerNear = true;
        }
    }
    
    private void Update()
    {
        // Handle door movement
        if ((isPlayerNear || testMode) && !isOpen)
        {
            Debug.Log("Opening door...");
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
                Debug.Log("Door fully opened");
            }
        }
        else if (!isPlayerNear && isOpen && autoClose)
        {
            Debug.Log("Closing door...");
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
                Debug.Log("Door fully closed");
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered by: " + other.gameObject.name + " with tag: " + other.tag);
        
        // Check if the player entered the trigger
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detected near door!");
            isPlayerNear = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger exited by: " + other.gameObject.name);
        
        // Check if the player exited the trigger
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player left door area");
            isPlayerNear = false;
        }
    }
}
