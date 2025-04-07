using UnityEngine;
using UnityEngine.SceneManagement; // Needed for reloading
using System.Collections; // Needed for Coroutine
using TMPro; // Needed for TextMeshProUGUI

// Attach this script to the "Finish" trigger object.
public class FinishTrigger : MonoBehaviour
{
    [Header("Win Sequence")]
    [Tooltip("The TextMeshPro UI element displaying 'You Win'.")]
    [SerializeField] private TextMeshProUGUI winText;

    [Tooltip("How long the 'You Win' text stays visible after fading in.")]
    [SerializeField] private float winDisplayTime = 5.0f;

    [Tooltip("The color to fade the screen to (e.g., white).")]
    [SerializeField] private Color fadeColor = Color.white;

    [Tooltip("Duration of the fade effect.")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Optional Feedback")]
    [Tooltip("(Optional) Sound effect to play on finish.")]
    [SerializeField] private AudioSource successSound;

    // Prevent triggering multiple times
    private bool hasFinished = false;

    private void Start()
    {
        // Ensure collider is a trigger
        Collider col = GetComponent<Collider>();
        if (col == null || !col.isTrigger)
        {
            Debug.LogWarning($"FinishTrigger on '{gameObject.name}' requires a Collider with 'Is Trigger' checked.", gameObject);
        }

        // Ensure Win Text is assigned and disable it initially
        if (winText == null)
        {
            Debug.LogError($"Win Text is not assigned in the FinishTrigger script on '{gameObject.name}'!", gameObject);
        }
        else
        {
            winText.gameObject.SetActive(false); // Start hidden
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if already finished or if it's not the player
        if (hasFinished || !other.CompareTag("Player"))
        {
            return;
        }

        // Mark as finished to prevent re-triggering
        hasFinished = true;
        Debug.Log($"Player touched Finish object '{gameObject.name}'. Starting Win Sequence.");

        // Start the win sequence coroutine
        StartCoroutine(WinSequenceCoroutine());
    }

    private IEnumerator WinSequenceCoroutine()
    {
        // 1. Play Sound (Optional)
        if (successSound != null)
        {
            successSound.Play();
        }

        // 2. Activate Win Text
        if (winText != null)
        {
            winText.gameObject.SetActive(true);
        }

        // 3. Start Fade Out to specified color and wait for it to finish
        if (ScreenFader.Instance != null)
        {
            Debug.Log("Starting fade to white...");
            // Start the fade AND wait for the coroutine it returns to complete
            yield return ScreenFader.Instance.FadeToColor(fadeColor, 1f, fadeDuration);
            Debug.Log("Fade to white complete.");
        }
        else
        {
            Debug.LogError("ScreenFader Instance not found! Cannot fade.", gameObject);
        }

        // 4. Wait for the specified display time
        Debug.Log($"Waiting {winDisplayTime} seconds...");
        yield return new WaitForSeconds(winDisplayTime);

        // 5. Reload the current scene
        Debug.Log("Reloading scene...");
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
