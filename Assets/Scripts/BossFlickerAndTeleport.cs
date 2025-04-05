using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFlickerAndTeleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform spawnPoint;
    
    [Header("Flicker Settings")]
    [Range(1f, 10f)]
    public float flickerDuration = 5f;
    [Range(0.01f, 0.5f)]
    public float minFlickerTime = 0.05f;
    [Range(0.05f, 0.8f)]
    public float maxFlickerTime = 0.2f;
    
    [Header("Optional Effects")]
    public AudioSource teleportSound;
    public ParticleSystem teleportEffect;
    
    private Renderer[] renderers;
    private bool isFlickering = false;
    
    private void Awake()
    {
        // Get all renderers on this object and its children
        renderers = GetComponentsInChildren<Renderer>();
    }
    
    public void TriggerFlickerAndTeleport()
    {
        // Only start if not already flickering
        if (!isFlickering)
        {
            StartCoroutine(FlickerAndTeleportRoutine());
        }
    }
    
    private IEnumerator FlickerAndTeleportRoutine()
    {
        isFlickering = true;
        float elapsedTime = 0f;
        
        // Play starting effect if available
        if (teleportEffect != null)
        {
            teleportEffect.Play();
        }
        
        // Flicker for the specified duration
        while (elapsedTime < flickerDuration)
        {
            // Toggle visibility (all renderers)
            bool visible = Random.value > 0.5f;
            SetVisibility(visible);
            
            // Wait for a random time between min and max
            float waitTime = Random.Range(minFlickerTime, maxFlickerTime);
            yield return new WaitForSeconds(waitTime);
            
            elapsedTime += waitTime;
        }
        
        // Make sure it's invisible for the teleport
        SetVisibility(false);
        
        // Play sound if available
        if (teleportSound != null)
        {
            teleportSound.Play();
        }
        
        // Wait a moment while invisible
        yield return new WaitForSeconds(0.2f);
        
        // Teleport the boss to spawn point
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
        
        // Make boss visible again
        SetVisibility(true);
        
        // Play teleport arrival effect if available
        if (teleportEffect != null)
        {
            teleportEffect.Play();
        }
        
        isFlickering = false;
    }
    
    private void SetVisibility(bool visible)
    {
        foreach (var renderer in renderers)
        {
            renderer.enabled = visible;
        }
    }
}
