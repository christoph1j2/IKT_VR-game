using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class FinishTrigger : MonoBehaviour
{
    [Header("Win Sequence")]
    [SerializeField] private TextMeshProUGUI winText;
//    [SerializeField] private GameObject FadeCanvas; // Canvas to fade in/out
    [SerializeField] private float winDisplayTime = 5.0f;
    [SerializeField] private Color fadeColor = Color.white;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private CanvasGroup winTextCanvasGroup;
    [SerializeField] private float textFadeDuration = 1.0f;

    [Header("Optional Feedback")]
    [SerializeField] private AudioSource successSound;

    private bool hasFinished = false;
    //private CanvasGroup winTextCanvasGroup;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col == null || !col.isTrigger)
        {
            Debug.LogWarning($"FinishTrigger on '{gameObject.name}' requires a Collider with 'Is Trigger' checked.", gameObject);
        }

        if (winText == null)
        {
            Debug.LogError($"Win Text is not assigned!", gameObject);
        }
        else
        {
            winText.gameObject.SetActive(true); // Make sure it's enabled
            winTextCanvasGroup = winText.GetComponent<CanvasGroup>();
            if (winTextCanvasGroup == null)
            {
                winTextCanvasGroup = winText.gameObject.AddComponent<CanvasGroup>();
            }
            winTextCanvasGroup.alpha = 0f; // Hide initially
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasFinished || !other.CompareTag("Player")) return;
        hasFinished = true;
        StartCoroutine(WinSequenceCoroutine());
    }

    private IEnumerator WinSequenceCoroutine()
    {
        if (successSound != null) successSound.Play();

        // Fade in the text BEFORE fading the screen
        if (winTextCanvasGroup != null)
        {
            yield return StartCoroutine(FadeInText(winTextCanvasGroup, textFadeDuration));
        }

        // Short pause to let text settle
        yield return new WaitForSeconds(0.5f);

        // Fade the screen
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.SetIsDeathFade(false); // This is a win, not a death
            yield return ScreenFader.Instance.FadeToColor(fadeColor, 1f, fadeDuration);
        }


        yield return new WaitForSeconds(winDisplayTime);

        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene("MainMenuScene"); // Load the main menu scene
    }


    // private IEnumerator FadeInText(CanvasGroup canvasGroup, float duration)
    // {
    //     float elapsed = 0f;
    //     while (elapsed < duration)
    //     {
    //         canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
    //         elapsed += Time.deltaTime;
    //         yield return null;
    //     }
    //     canvasGroup.alpha = 1f;
    // }

    private IEnumerator FadeInText(CanvasGroup canvasGroup, float duration)
    {
        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
    }
}
