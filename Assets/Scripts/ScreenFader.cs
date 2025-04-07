using UnityEngine;
using UnityEngine.UI; // Required for Image
using UnityEngine.SceneManagement; // Required for reloading scene
using System.Collections;

// Controls fading a UI Image in/out. Actions after fade are handled by the calling script.
public class ScreenFader : MonoBehaviour
{
    [Tooltip("The UI Image used for fading (assign a black or white image).")]
    [SerializeField] private Image fadeImage;

    [Tooltip("How long the fade in/out animation takes in seconds.")]
    [SerializeField] private float defaultFadeDuration = 1.0f;

    [SerializeField] private GameObject youDiedText;

    private bool isDeathFade = false;



    // Singleton pattern
    public static ScreenFader Instance { get; private set; }

    private Coroutine currentFadeCoroutine = null; // Track active fade

    private void Awake()
    {
        // --- Singleton Setup ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optional: Keep alive between scenes if needed
        // DontDestroyOnLoad(gameObject);
        // --- End Singleton ---

        if (fadeImage == null)
        {
            Debug.LogError("Fade Image is not assigned in the ScreenFader script!", this.gameObject);
            enabled = false;
            return;
        }

        // Start fully transparent, regardless of initial image color
        SetAlpha(0f);
        fadeImage.gameObject.SetActive(false); // Start inactive

        if (youDiedText != null)
        {
            youDiedText.SetActive(false);
        }
    }

    // --- Public Methods to Trigger Fades ---

    // Fades FROM current alpha TO targetAlpha using the image's current RGB color
    public Coroutine FadeAlpha(float targetAlpha, float duration = -1f)
    {
         if (duration < 0) duration = defaultFadeDuration; // Use default if not specified
         // Stop existing fade if running
         if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
         // Start new fade
         currentFadeCoroutine = StartCoroutine(FadeRoutine(fadeImage.color.a, targetAlpha, duration));
         return currentFadeCoroutine;
    }

    // Fades TO targetAlpha using a specific COLOR (sets RGB first)
    public Coroutine FadeToColor(Color targetColor, float targetAlpha = 1f, float duration = -1f)
    {
        if (duration < 0) duration = defaultFadeDuration;
        // Set the RGB color immediately (alpha will be handled by fade)
        fadeImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, fadeImage.color.a);
        // Stop existing fade if running
        if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
        // Start new fade
        currentFadeCoroutine = StartCoroutine(FadeRoutine(fadeImage.color.a, targetAlpha, duration));
        return currentFadeCoroutine;
    }


    // --- Coroutine for the actual alpha fade ---
    private IEnumerator FadeRoutine(float startAlpha, float targetAlpha, float duration)
    {
        // Make sure image is active for fading
        fadeImage.gameObject.SetActive(true);

        float timer = 0f;
        Color currentColor = fadeImage.color;

        // Set starting alpha explicitly
        SetAlpha(startAlpha);

        // Loop until duration is reached
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            SetAlpha(newAlpha);
            yield return null;
        }

        // Ensure target alpha is set exactly at the end
        SetAlpha(targetAlpha);

        // Show "You Died" if fading fully to black and this is a death fade
        if (targetAlpha == 1f && isDeathFade && youDiedText != null)
        {
            Debug.Log("Showing 'You Died' text");
            youDiedText.SetActive(true);
        }

        // If fading fully transparent, disable image and hide text
        if (targetAlpha == 0f)
        {
            fadeImage.gameObject.SetActive(false);
            if (youDiedText != null) 
            {
                youDiedText.SetActive(false);
            }
        }

        currentFadeCoroutine = null;
    }

    // Helper to set alpha only
    private void SetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }

    // --- Scene Reloading is now handled by the calling script ---
    // Keep this method here if PlayerHealth needs it as a fallback
    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
        Debug.Log($"Reloading scene: {currentScene.name}");
    }

    private void LateUpdate()
    {
        // Optional: always face the camera
        if (fadeImage != null && fadeImage.canvas != null)
        {
            fadeImage.canvas.transform.forward = Camera.main.transform.forward;
        }
    }


    // Call this before FadeToColor when it's a death fade
    public void SetIsDeathFade(bool value)
    {
        Debug.Log($"Setting isDeathFade to: {value}");
        isDeathFade = value;
        
        // If we're setting it to false, make sure the text is hidden
        if (!value && youDiedText != null)
        {
            youDiedText.SetActive(false);
        }
    }

}
