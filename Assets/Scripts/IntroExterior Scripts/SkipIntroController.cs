using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SkipIntroController : MonoBehaviour
{
    [Header("Indicacion Saltar")]
    [SerializeField] private CanvasGroup skipPrompt;
    [SerializeField] private Image progressRing;
    [SerializeField] private float promptFadeDuration = 1f;
    [SerializeField] private float holdDuration = 5f;

    [Header("Fade a negro")]
    [SerializeField] private Image skipFadeImage;
    [SerializeField] private float fadeDuration = 4f;

    [Header("Escena")]
    [SerializeField] private string sceneToLoad = "Night_1 Scene";

    private float holdTimer;
    private bool promptVisible;
    private bool isLoading;
    private Coroutine promptCoroutine;

    private void Awake()
    {
        if (skipPrompt != null)
        {
            skipPrompt.alpha = 0f;
            skipPrompt.interactable = false;
            skipPrompt.blocksRaycasts = false;
        }

        if (progressRing != null)
            progressRing.fillAmount = 0f;

        if (skipFadeImage != null)
        {
            Color c = skipFadeImage.color;
            c.a = 0f;
            skipFadeImage.color = c;
            skipFadeImage.raycastTarget = false;
        }
    }

    private void Update()
    {
        if (!promptVisible || isLoading)
            return;

        if (Input.GetKey(KeyCode.E))
        {
            holdTimer += Time.unscaledDeltaTime;

            if (progressRing != null)
                progressRing.fillAmount = Mathf.Clamp01(holdTimer / holdDuration);

            if (holdTimer >= holdDuration)
                StartCoroutine(FadeAndLoad());
        }
        else
        {
            holdTimer = 0f;

            if (progressRing != null)
                progressRing.fillAmount = 0f;
        }
    }

    // Llamar desde un Signal de Timeline
    public void ShowSkipPrompt()
    {
        if (isLoading || promptVisible)
            return;

        promptVisible = true;

        if (promptCoroutine != null)
            StopCoroutine(promptCoroutine);

        promptCoroutine = StartCoroutine(FadeInPrompt());
    }

    private IEnumerator FadeInPrompt()
    {
        if (skipPrompt == null)
        {
            Debug.LogError("SkipIntroController: falta asignar Skip Prompt.");
            yield break;
        }

        skipPrompt.alpha = 0f;

        float timer = 0f;

        while (timer < promptFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            skipPrompt.alpha = Mathf.Clamp01(timer / promptFadeDuration);
            yield return null;
        }

        skipPrompt.alpha = 1f;
        promptCoroutine = null;
    }

    private IEnumerator FadeAndLoad()
    {
        isLoading = true;
        promptVisible = false;

        if (skipFadeImage == null)
        {
            Debug.LogError("SkipIntroController: falta asignar Skip Fade Image.");
            isLoading = false;
            yield break;
        }

        skipFadeImage.gameObject.SetActive(true);
        skipFadeImage.transform.SetAsLastSibling();

        Color c = skipFadeImage.color;
        c.a = 0f;
        skipFadeImage.color = c;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            c.a = Mathf.Clamp01(timer / fadeDuration);
            skipFadeImage.color = c;
            yield return null;
        }

        c.a = 1f;
        skipFadeImage.color = c;

        // Esperar un frame para mostrar el negro completo
        yield return null;

        SceneManager.LoadScene(sceneToLoad);
    }
}
