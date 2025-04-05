using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Transform playerTransform; // Reference to your SteamVR [CameraRig]
    
    public void StartGame()
    {
        Debug.Log("Starting game and morphing player...");
        
        // Find the transition manager or create one if it doesn't exist
        SceneTransitionManager transitionManager = FindObjectOfType<SceneTransitionManager>();
        if (transitionManager == null)
        {
            GameObject managerObject = new GameObject("SceneTransitionManager");
            transitionManager = managerObject.AddComponent<SceneTransitionManager>();
        }
        
        // Start the transition
        transitionManager.TransitionToMainScene(playerTransform);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
