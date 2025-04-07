using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuManager : MonoBehaviour
{
    [SerializeField] private Transform playerTransform; // Reference to your SteamVR [CameraRig]
    
    public void StartGame()
    {
        Debug.Log("Starting game...");
        SceneManager.LoadScene("MainScene");
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
