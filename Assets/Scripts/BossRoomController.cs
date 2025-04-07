using UnityEngine;

public class BossRoomController : MonoBehaviour
{
    public string playerTag = "Player";           // Make sure your VR rig or root GameObject has this tag
    public AudioClip triggerSound;                // Sound to play on entry

    private AudioSource audioSource;
    private bool hasTriggered = false;            // Flag to track if already triggered

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Only trigger if it's the player AND we haven't triggered before
        if (other.CompareTag(playerTag) && !hasTriggered)
        {
            // Play sound if available
            if (triggerSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(triggerSound);
                Debug.Log("Boss room trigger activated - playing one-time sound");
            }
            
            // Mark as triggered so it won't activate again
            hasTriggered = true;
        }
    }
    
    // Optional: Public method to reset the trigger (useful for testing or level resets)
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}