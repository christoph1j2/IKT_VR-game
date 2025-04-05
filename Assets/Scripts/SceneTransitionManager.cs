using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

public class SceneTransitionManager : MonoBehaviour
{
    // Singleton pattern for easy access
    public static SceneTransitionManager Instance { get; private set; }
    
    // Reference to the menu player prefab's transform (usually the [CameraRig])
    private Transform menuPlayerTransform;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TransitionToMainScene(Transform currentPlayerTransform)
    {
        // Store reference to menu player
        menuPlayerTransform = currentPlayerTransform;
        StartCoroutine(LoadMainSceneAndTransferPlayer());
    }

    private IEnumerator LoadMainSceneAndTransferPlayer()
    {
        // Store initial player position and rotation
        Vector3 playerPosition = menuPlayerTransform.position;
        Quaternion playerRotation = menuPlayerTransform.rotation;
        
        // Load the main scene additively
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Additive);
        
        // Wait until the scene is loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Find the player prefab in the new scene
        GameObject mainScenePlayer = GameObject.FindGameObjectWithTag("Player");
        
        if (mainScenePlayer != null)
        {
            // Position the new player at the same location as the menu player
            mainScenePlayer.transform.position = playerPosition;
            mainScenePlayer.transform.rotation = playerRotation;
            
            // Enable the main scene player if it was disabled
            mainScenePlayer.SetActive(true);
        }
        else
        {
            Debug.LogError("Could not find player prefab in main scene! Make sure it has the 'Player' tag.");
        }
        
        // Set the main scene as active
        Scene mainScene = SceneManager.GetSceneByName("MainScene");
        SceneManager.SetActiveScene(mainScene);
        
        // Disable the menu player
        menuPlayerTransform.gameObject.SetActive(false);
        
        // Unload the menu scene
        Scene menuScene = menuPlayerTransform.gameObject.scene;
        yield return SceneManager.UnloadSceneAsync(menuScene);
    }
}
