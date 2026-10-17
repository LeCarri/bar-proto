
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SkipIntroController : MonoBehaviour
{
    [Header("Indicación para saltar")]
    [SerializeField] private CanvasGroup skipPrompt;
    [SerializeField] private float promptFadeDuration = 0.5f;

    [Header("Fade a negro")]
    [SerializeField] private Image skipFadeImage;
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Escena")]
    [SerializeField] private string sceneToLoad = "Night_1 Scene";

    private bool promptVisible = false;
    private bool isLoading = false;
    private Coroutine promptCoroutine;

    private void Awake()
    {
        // Ocultar inicialmente la indicación.
        if (skipPrompt != null)
        {
            skipPrompt.alpha = 0f;
            skipPrompt.interactable = false;
            skipPrompt.blocksRaycasts = false;
        }

        // Preparar el fondo negro transparente.
        if (skipFadeImage != null)
        {
            Color c = skipFadeImage.color;
            c.a = 0f;
            skipFadeImage.color = c;
            skipFadeImage.raycastTarget = false;
        }
    }

    private void Start()
    {
        // Mostrar automáticamente la opción de saltar.
        ShowSkipPrompt();
    }

    private void Update()
    {
        if (!promptVisible || isLoading)
            return;

        // UNA SOLA PULSACIÓN DE E.
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(FadeAndLoad());
        }
    }

    public void ShowSkipPrompt()
    {
        if (isLoading || promptVisible)
            return;

        promptVisible = true;

        if (skipPrompt != null)
        {
            skipPrompt.gameObject.SetActive(true);

            if (promptCoroutine != null)
                StopCoroutine(promptCoroutine);

            promptCoroutine = StartCoroutine(FadeInPrompt());
        }
    }

    private IEnumerator FadeInPrompt()
    {
        float timer = 0f;

        if (promptFadeDuration <= 0f)
        {
            skipPrompt.alpha = 1f;
            promptCoroutine = null;
            yield break;
        }

        while (timer < promptFadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            skipPrompt.alpha = Mathf.Clamp01(
                timer / promptFadeDuration
            );

            yield return null;
        }

        skipPrompt.alpha = 1f;
        promptCoroutine = null;
    }

    private IEnumerator FadeAndLoad()
    {
        isLoading = true;
        promptVisible = false;

        // Detener la animación del aviso.
        if (promptCoroutine != null)
        {
            StopCoroutine(promptCoroutine);
            promptCoroutine = null;
        }

        if (skipPrompt != null)
            skipPrompt.alpha = 0f;

        if (skipFadeImage == null)
        {
            Debug.LogWarning(
                "[SkipIntro] No hay imagen para el fade."
            );

            CargarNoche1();
            yield break;
        }

        // Activar fondo negro.
        skipFadeImage.gameObject.SetActive(true);
        skipFadeImage.transform.SetAsLastSibling();
        skipFadeImage.raycastTarget = true;

        Color c = skipFadeImage.color;
        c.a = 0f;
        skipFadeImage.color = c;

        float timer = 0f;

        // Fundido de pantalla.
        if (fadeDuration > 0f)
        {
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;

                c.a = Mathf.Clamp01(
                    timer / fadeDuration
                );

                skipFadeImage.color = c;

                yield return null;
            }
        }

        // Pantalla completamente negra.
        c.a = 1f;
        skipFadeImage.color = c;

        yield return null;

        // Cargar directamente la Noche 1.
        CargarNoche1();
    }

    private void CargarNoche1()
    {
        Time.timeScale = 1f;

        Debug.Log(
            "[SkipIntro] Introducción saltada. Cargando Noche 1."
        );

        SceneManager.LoadScene(sceneToLoad);
    }
}
