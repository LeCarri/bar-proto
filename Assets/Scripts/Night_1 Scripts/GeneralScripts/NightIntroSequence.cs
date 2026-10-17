
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NightIntroSequence : MonoBehaviour
{
    [Header("Elementos visuales")]
    [SerializeField] private Image blackOverlay;
    [SerializeField] private CanvasGroup titleGroup;

    [Header("Texto máquina de escribir")]
    [SerializeField] private TMP_Text textoNoche;
    [SerializeField] private string palabra = "NOCHE";
    [SerializeField] private float tiempoPorLetra = 0.14f;

    [Header("Contador 1 a 2")]
    [SerializeField] private GameObject numeroViewport;
    [SerializeField] private RectTransform contador;
    [SerializeField] private float alturaNumero = 120f;
    [SerializeField] private float esperaAntesContador = 0.5f;
    [SerializeField] private float duracionContador = 0.65f;
    [SerializeField] private float demoraSonidoFinal = 0.3f;

    [Header("Tiempos")]
    [SerializeField] private float titleHoldDuration = 1.5f;
    [SerializeField] private float titleFadeOutDuration = 1.5f;
    [SerializeField] private float waitAfterTitle = 1f;
    [SerializeField] private float blackFadeOutDuration = 3f;

    [Header("Sonidos")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoEscritura;
    [SerializeField] private AudioClip sonidoContador;
    [SerializeField] private AudioClip sonidoFinal;

    [Range(0f, 1f)]
    [SerializeField] private float volumenEscritura = 0.4f;

    [Range(0f, 1f)]
    [SerializeField] private float volumenContador = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float volumenFinal = 0.8f;

    [Header("Inicio de Noche 2")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Act2ManagerDemo act2Manager;

    private void Awake()
    {
        // Pantalla completamente negra.
        if (blackOverlay != null)
        {
            Color color = blackOverlay.color;
            color.a = 1f;
            blackOverlay.color = color;
            blackOverlay.gameObject.SetActive(true);
            blackOverlay.raycastTarget = true;
        }

        // Preparar título.
        if (titleGroup != null)
        {
            titleGroup.gameObject.SetActive(true);
            titleGroup.alpha = 0f;
            titleGroup.blocksRaycasts = false;
        }

        // Preparar escritura.
        if (textoNoche != null)
        {
            textoNoche.text = palabra;
            textoNoche.maxVisibleCharacters = 0;
        }

        // Preparar contador.
        if (numeroViewport != null)
            numeroViewport.SetActive(false);

        if (contador != null)
            contador.anchoredPosition = Vector2.zero;

        SetMovementEnabled(false);
    }

    private void Start()
    {
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // 1. Mostrar el contenedor del título.
        if (titleGroup != null)
            titleGroup.alpha = 1f;

        // 2. Escribir NOCHE letra por letra.
        yield return StartCoroutine(EscribirTitulo());

        // 3. Mostrar inicialmente el número 1.
        if (contador != null)
            contador.anchoredPosition = Vector2.zero;

        if (numeroViewport != null)
            numeroViewport.SetActive(true);

        yield return Wait(esperaAntesContador);

        // 4. Cambiar el contador del 1 al 2.
        yield return StartCoroutine(AnimarContador());

        // 5. Mantener NOCHE 2 en pantalla.
        yield return Wait(titleHoldDuration);

        // 6. Desvanecer el título.
        yield return FadeTitle(
            1f,
            0f,
            titleFadeOutDuration
        );

        // 7. Permanecer en negro.
        yield return Wait(waitAfterTitle);

        // 8. Revelar el escenario.
        yield return FadeBlack(
            1f,
            0f,
            blackFadeOutDuration
        );

        if (blackOverlay != null)
            blackOverlay.raycastTarget = false;

        // 9. Habilitar movimiento.
        SetMovementEnabled(true);

        // 10. Iniciar los eventos originales.
        if (act2Manager != null)
        {
            act2Manager.IniciarSecuenciaNoche2();
        }
        else
        {
            Debug.LogError(
                "[NightIntroSequence] Falta Act2ManagerDemo."
            );
        }
    }

    private IEnumerator EscribirTitulo()
    {
        if (textoNoche == null)
            yield break;

        textoNoche.text = palabra;
        textoNoche.maxVisibleCharacters = 0;

        for (int i = 0; i < palabra.Length; i++)
        {
            textoNoche.maxVisibleCharacters = i + 1;

            if (!char.IsWhiteSpace(palabra[i]))
                ReproducirSonido(
                    sonidoEscritura,
                    volumenEscritura
                );

            yield return new WaitForSecondsRealtime(
                tiempoPorLetra
            );
        }
    }

    private IEnumerator AnimarContador()
    {
        if (contador == null)
            yield break;

        ReproducirSonido(
            sonidoContador,
            volumenContador
        );

        Vector2 inicio = Vector2.zero;
        Vector2 final = new Vector2(0f, alturaNumero);

        float tiempo = 0f;

        if (duracionContador > 0f)
        {
            while (tiempo < duracionContador)
            {
                tiempo += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(
                    tiempo / duracionContador
                );

                // Desaceleración mecánica.
                float suavizado =
                    1f - Mathf.Pow(1f - t, 3f);

                contador.anchoredPosition =
                    Vector2.LerpUnclamped(
                        inicio,
                        final,
                        suavizado
                    );

                yield return null;
            }
        }

        contador.anchoredPosition = final;

        // Esperar antes de reproducir el sonido final.
        yield return new WaitForSecondsRealtime(demoraSonidoFinal);

        // Reproducir impacto final.
        ReproducirSonido(
            sonidoFinal,
            volumenFinal
        );
    }

    private void ReproducirSonido(
        AudioClip clip,
        float volumen
    )
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volumen);
        }
    }

    private IEnumerator FadeTitle(
        float from,
        float to,
        float duration
    )
    {
        if (titleGroup == null)
            yield break;

        titleGroup.alpha = from;

        if (duration <= 0f)
        {
            titleGroup.alpha = to;
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                timer / duration
            );

            titleGroup.alpha =
                Mathf.Lerp(from, to, progress);

            yield return null;
        }

        titleGroup.alpha = to;
    }

    private IEnumerator FadeBlack(
        float from,
        float to,
        float duration
    )
    {
        if (blackOverlay == null)
            yield break;

        SetBlackAlpha(from);

        if (duration <= 0f)
        {
            SetBlackAlpha(to);
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                timer / duration
            );

            SetBlackAlpha(
                Mathf.Lerp(from, to, progress)
            );

            yield return null;
        }

        SetBlackAlpha(to);
    }

    private IEnumerator Wait(float duration)
    {
        if (duration > 0f)
            yield return new WaitForSecondsRealtime(duration);
    }

    private void SetBlackAlpha(float alpha)
    {
        if (blackOverlay == null)
            return;

        Color color = blackOverlay.color;
        color.a = alpha;
        blackOverlay.color = color;
    }

    private void SetMovementEnabled(bool enabledState)
    {
        if (playerController != null)
        {
            playerController.controlesBloqueados =
                !enabledState;
        }
    }
}
