using UnityEngine;

public class BossDoorTrigger : MonoBehaviour
{
    public BossFlickerAndTeleport bossController;
    
    private bool hasTriggered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        // Only trigger once and only for the player
        if (!hasTriggered && other.CompareTag("Player"))
        {
            if (bossController != null)
            {
                bossController.TriggerFlickerAndTeleport();
                hasTriggered = true;
                
                // Optional: Reset after some time to allow triggering again
                // Uncomment if you want this behavior
                // Invoke("ResetTrigger", 10f);
            }
            else
            {
                Debug.LogError("No Boss Controller assigned to Door Trigger!");
            }
        }
    }
    
    // Optional: Allow the door to be triggered again after some time
    private void ResetTrigger()
    {
        hasTriggered = false;
    }
}
