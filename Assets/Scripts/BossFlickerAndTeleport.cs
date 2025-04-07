using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles flickering effect and teleportation for the Boss.
// Also activates the BossController's following behavior after teleporting.
// Attach this script to the Boss GameObject.
public class BossFlickerAndTeleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("The location where the boss should teleport to.")]
    public Transform spawnPoint;

    [Header("Flicker Settings")]
    [Tooltip("Total duration of the flickering effect in seconds.")]
    [Range(1f, 10f)]
    public float flickerDuration = 5f;

    [Tooltip("Minimum time the boss stays visible/invisible during a flicker.")]
    [Range(0.01f, 0.5f)]
    public float minFlickerTime = 0.05f;

    [Tooltip("Maximum time the boss stays visible/invisible during a flicker.")]
    [Range(0.05f, 0.8f)]
    public float maxFlickerTime = 0.2f;

    [Header("Optional Effects")]
    [Tooltip("(Optional) Sound effect to play when teleporting.")]
    [SerializeField] private AudioClip teleportSound;

    [Tooltip("(Optional) Jumpscare sound to play when boss reappears.")]
    [SerializeField] private AudioClip jumpscareSound;

    [Tooltip("(Optional) Particle system to play during flicker/teleport.")]
    public ParticleSystem teleportEffect;

    private AudioSource audioSource; // Reference to the AudioSource component for sound effects

    // --- Component References ---
    private Renderer[] renderers; // Array to hold all renderers for visibility toggling
    private BossController bossController; // Reference to the BossController script

    // --- State Flag ---
    private bool isFlickering = false; // Prevents starting the effect multiple times

    // Awake is called when the script instance is being loaded.
    private void Awake()
    {
        // Find all Renderer components in this GameObject and its children
        renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] BossFlickerAndTeleport found no Renderer components in children. Flickering will not be visible.", gameObject);
        }

        // Get the BossController component from the same GameObject
        bossController = GetComponent<BossController>();
        if (bossController == null)
        {
            // Log an error if the controller script is missing, as we need it later
            Debug.LogError($"[{gameObject.name}] BossFlickerAndTeleport could not find BossController script on the same GameObject! Following cannot be activated.", gameObject);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log($"[{gameObject.name}] Added AudioSource component for BossFlickerAndTeleport sounds.", gameObject);
        }
    }

    // Public method to be called by other scripts (like a door trigger) to start the effect
    public void TriggerFlickerAndTeleport()
    {
        // Only start the coroutine if it's not already running
        if (!isFlickering)
        {
            StartCoroutine(FlickerAndTeleportRoutine());
        }
        else
        {
            Debug.Log($"[{gameObject.name}] FlickerAndTeleport already in progress. Trigger ignored.", gameObject);
        }
    }

    // Coroutine that handles the timing of flickering and teleportation
    private IEnumerator FlickerAndTeleportRoutine()
    {
        isFlickering = true; // Set flag to indicate the process has started
        float elapsedTime = 0f; // Timer to track flicker duration

        Debug.Log($"[{gameObject.name}] FlickerAndTeleportRoutine started. Duration: {flickerDuration}s", gameObject);

        if (jumpscareSound != null)
        {
            PlayJumpscareSound(); // Play jumpscare sound if assigned
        }

        // Play starting particle effect if assigned
        if (teleportEffect != null)
        {
            teleportEffect.Play();
        }

        // --- Flicker Loop ---
        while (elapsedTime < flickerDuration)
        {
            // Randomly decide whether to be visible or invisible
            bool visible = Random.value > 0.5f;
            SetVisibility(visible); // Apply visibility change

            // Wait for a random duration within the specified min/max range
            float waitTime = Random.Range(minFlickerTime, maxFlickerTime);
            yield return new WaitForSeconds(waitTime); // Pause execution

            elapsedTime += waitTime; // Add elapsed time to the timer
        }
        // --- End Flicker Loop ---

        // Ensure the boss is invisible right before teleporting
        SetVisibility(false);
        Debug.Log($"[{gameObject.name}] Flickering finished. Boss invisible.", gameObject);

        // Play teleport sound effect if assigned
        if (teleportSound != null)
        {
            PlayTeleportSound(); // Play teleport sound
        }

        // Short delay while invisible before teleporting (for effect)
        yield return new WaitForSeconds(0.2f);

        // --- Teleport ---
        if (spawnPoint != null)
        {
            // Move the boss to the spawn point's position and rotation
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
            Debug.Log($"[{gameObject.name}] Boss teleported to spawn point: {spawnPoint.name}", gameObject);
        }
        else
        {
             Debug.LogError($"[{gameObject.name}] SpawnPoint not assigned in BossFlickerAndTeleport! Cannot teleport.", gameObject);
        }
        // --- End Teleport ---

        // Make boss visible again after teleporting
        SetVisibility(true);

        // Play arrival particle effect if assigned
        if (teleportEffect != null)
        {
            // Consider using Play() or Emit() depending on particle system setup
            teleportEffect.Play();
        }

        // --- Activate Following Behavior ---
        if (bossController != null)
        {
            // Call the public method on the BossController script
            bossController.ActivateFollowing();
        }
        else
        {
             Debug.LogError($"[{gameObject.name}] Cannot activate following because BossController reference is missing!", gameObject);
        }
        // --- End Activate Following ---

        isFlickering = false; // Reset flag, allowing the effect to be triggered again later
        Debug.Log($"[{gameObject.name}] FlickerAndTeleportRoutine finished.", gameObject);
    }

    // Helper method to enable/disable all Renderer components found in Awake
    private void SetVisibility(bool visible)
    {
        foreach (var renderer in renderers)
        {
            if (renderer != null) // Check if renderer still exists
            {
                renderer.enabled = visible;
            }
        }
    }

    private void PlayJumpscareSound()
    {
        if (jumpscareSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpscareSound); // Play jumpscare sound
        }
    }
    private void PlayTeleportSound()
    {
        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound); // Play teleport sound
        }
    }
} // End of class BossFlickerAndTeleport
