using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarfishSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject starfishPrefab;
    public Transform[] spawnPoints;
    [Range(0.1f, 5f)]
    public float delayBeforeFall = 1f;
    [Range(0.1f, 3f)]
    public float spawnDelayBetweenStarfish = 0.3f;
    [Tooltip("Initial rotation offset applied at spawn (e.g., 180 on Y-axis)")]
    public Vector3 initialRotationOffset = new Vector3(0, 180, 0); // Added this line

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger once and only for the player
        if (!hasTriggered && other.CompareTag("Player"))
        {
            StartCoroutine(SpawnStarfishSequence());
            hasTriggered = true;
        }
    }

    private IEnumerator SpawnStarfishSequence()
    {
        if (starfishPrefab == null)
        {
            Debug.LogError("No Starfish prefab assigned!");
            yield break;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            yield break;
        }

        // Calculate the initial offset rotation once
        Quaternion offsetRotation = Quaternion.Euler(initialRotationOffset); // Added this line

        // Spawn a Starfish at each spawn point with a slight delay between spawns
        foreach (Transform spawnPoint in spawnPoints)
        {
            // --- MODIFIED LINE ---
            // Apply the initial offset rotation to the spawn point's rotation
            Quaternion initialSpawnRotation = spawnPoint.rotation * offsetRotation;

            // Instantiate with the calculated initial rotation
            GameObject starfish = Instantiate(starfishPrefab, spawnPoint.position, initialSpawnRotation); // Modified this line

            // Get the starfish controller and tell it to initialize with the delay
            StarfishController controller = starfish.GetComponent<StarfishController>();
            if (controller != null)
            {
                controller.InitializeWithDelay(delayBeforeFall);
            }

            // Add slight delay between spawns for a more dramatic effect
            yield return new WaitForSeconds(spawnDelayBetweenStarfish);
        }
    }

    // Optional: For debugging/setup in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (spawnPoints != null)
        {
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
                    // Draw downward arrow to indicate fall direction
                    Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.down * 2);

                    // Draw forward direction with offset applied
                    Quaternion offsetRotation = Quaternion.Euler(initialRotationOffset);
                    Quaternion initialSpawnRotation = spawnPoint.rotation * offsetRotation;
                    Gizmos.color = Color.blue; // Blue for forward direction
                    Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + (initialSpawnRotation * Vector3.forward) * 1.0f);
                }
            }
        }
    }
}
