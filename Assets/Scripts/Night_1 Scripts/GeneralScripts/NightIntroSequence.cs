using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class NightIntroSequence : MonoBehaviour
{
    [Header("Elementos visuales")]
    [SerializeField] private Image blackOverlay;
    [SerializeField] private CanvasGroup titleGroup;

    [Header("Tiempos")]
    [SerializeField] private float titleFadeInDuration = 2f;
    [SerializeField] private float titleHoldDuration = 2f;
    [SerializeField] private float titleFadeOutDuration = 2f;
    [SerializeField] private float waitAfterTitle = 1f;
    [SerializeField] private float blackFadeOutDuration = 3f;

    //[Header("Control del jugador")]
    //[SerializeField] private PlayerController playerController;

    [Header("Inicio de Noche 2")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Act2ManagerDemo act2Manager;


    private void Awake()
    {
        // La pantalla comienza completamente negra.
        if (blackOverlay != null)
        {
            Color color = blackOverlay.color;
            color.a = 1f;
            blackOverlay.color = color;
            blackOverlay.gameObject.SetActive(true);
        }

        // El título comienza oculto.
        if (titleGroup != null)
        {
            titleGroup.alpha = 0f;
            titleGroup.gameObject.SetActive(true);
        }

        // Bloquear el movimiento durante la presentación.
        SetMovementEnabled(false);
    }

    private void Start()
    {
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // 1. Aparece suavemente el título sobre la pantalla negra.
        yield return FadeTitle(0f, 1f, titleFadeInDuration);

        // 2. El título permanece visible.
        yield return Wait(titleHoldDuration);

        // 3. El título desaparece suavemente.
        // La pantalla permanece completamente negra.
        yield return FadeTitle(1f, 0f, titleFadeOutDuration);

        // 4. Espera un segundo con la pantalla todavía negra.
        yield return Wait(waitAfterTitle);

        // 5. El negro desaparece y revela el escenario.
        yield return FadeBlack(1f, 0f, blackFadeOutDuration);

        // 6. Se habilita el control del jugador.
        SetMovementEnabled(true);

        // 7. Comenzar el diálogo y los eventos de Noche 2.
        if (act2Manager != null)
        {
            act2Manager.IniciarSecuenciaNoche2();
        }
        else
        {
            Debug.LogError("[NightIntroSequence] Falta asignar Act2ManagerDemo.");
        }
    }

    private IEnumerator FadeTitle(float from, float to, float duration)
    {
        if (titleGroup == null)
            yield break;

        // Forzar la transparencia inicial.
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

            float progress = Mathf.Clamp01(timer / duration);
            titleGroup.alpha = Mathf.Lerp(from, to, progress);

            yield return null;
        }

        titleGroup.alpha = to;
    }

    private IEnumerator FadeBlack(float from, float to, float duration)
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

            float progress = Mathf.Clamp01(timer / duration);
            SetBlackAlpha(Mathf.Lerp(from, to, progress));

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
            playerController.controlesBloqueados = !enabledState;
        }
    }
}