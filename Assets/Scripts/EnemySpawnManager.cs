using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab; // Your Smiler prefab
    public Transform spawnPointsParent; // Parent containing all spawn points
    public int numberOfEnemies = 3; // Default number of enemies to spawn
    public float spawnDelay = 0.5f; // Delay between spawns
    
    [Header("Debug")]
    public bool spawnOnStart = false; // For testing
    
    private List<Transform> spawnPoints = new List<Transform>();
    
    private void Start()
    {
        // Collect all spawn points
        if (spawnPointsParent != null)
        {
            foreach (Transform child in spawnPointsParent)
            {
                spawnPoints.Add(child);
            }
            
            Debug.Log($"Found {spawnPoints.Count} spawn points");
        }
        else
        {
            Debug.LogError("No spawn points parent assigned!");
        }
        
        if (spawnOnStart)
        {
            SpawnEnemies();
        }
    }
    
    public void SpawnEnemies()
    {
        StartCoroutine(SpawnEnemiesCoroutine());
    }
    
    private IEnumerator SpawnEnemiesCoroutine()
    {
        if (enemyPrefab == null || spawnPoints.Count == 0)
        {
            Debug.LogError("Cannot spawn enemies: Missing prefab or spawn points!");
            yield break;
        }
        
        // Choose random spawn points without duplicates
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);
        int enemiesToSpawn = Mathf.Min(numberOfEnemies, availableSpawnPoints.Count);
        
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // Select a random spawn point
            int randomIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[randomIndex];
            
            // Remove the used spawn point
            availableSpawnPoints.RemoveAt(randomIndex);
            
            // Spawn the enemy
            GameObject newEnemy = Instantiate(
                enemyPrefab, 
                spawnPoint.position, 
                spawnPoint.rotation
            );
            
            Debug.Log($"Enemy {i+1} spawned at {spawnPoint.name}");
            
            // Wait before spawning next enemy
            yield return new WaitForSeconds(spawnDelay);
        }
    }
}
