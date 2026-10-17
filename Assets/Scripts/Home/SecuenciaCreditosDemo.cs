
using System.Collections;
using UnityEngine;

public class SecuenciaCreditosDemo : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private CanvasGroup panelCierreNarrativo;
    [SerializeField] private CanvasGroup panelFinDemo;
    [SerializeField] private CanvasGroup panelInvitacion;

    [Header("Transformaciones")]
    [SerializeField] private RectTransform rectCierreNarrativo;
    [SerializeField] private RectTransform rectFinDemo;

    [Header("1 - Cierre narrativo")]
    [SerializeField] private float entradaCierre = 1.5f;
    [SerializeField] private float tiempoCierre = 4f;
    [SerializeField] private float salidaCierre = 1.2f;
    [SerializeField] private float desplazamientoCierre = 35f;

    [Header("2 - Fin de la demo")]
    [SerializeField] private float entradaFinDemo = 1f;
    [SerializeField] private float tiempoFinDemo = 2.5f;
    [SerializeField] private float salidaFinDemo = 1f;
    [SerializeField] private float escalaInicialFinDemo = 0.92f;

    [Header("3 - Invitacion y creditos")]
    [SerializeField] private float entradaInvitacion = 1.5f;

    [Header("Pausa entre paneles")]
    [SerializeField] private float pausaEntrePaneles = 0.3f;

    [Header("Audio opcional")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoCierre;
    [SerializeField] private AudioClip sonidoFinDemo;
    [SerializeField] private AudioClip sonidoInvitacion;

    private Vector2 posicionOriginalCierre;
    private Vector3 escalaOriginalFinDemo;

    private void Awake()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (rectCierreNarrativo != null)
            posicionOriginalCierre =
                rectCierreNarrativo.anchoredPosition;

        if (rectFinDemo != null)
            escalaOriginalFinDemo =
                rectFinDemo.localScale;

        PrepararPanel(panelCierreNarrativo);
        PrepararPanel(panelFinDemo);
        PrepararPanel(panelInvitacion);

        if (rectCierreNarrativo != null)
        {
            rectCierreNarrativo.anchoredPosition =
                posicionOriginalCierre +
                Vector2.down * desplazamientoCierre;
        }

        if (rectFinDemo != null)
            rectFinDemo.localScale =
                escalaOriginalFinDemo * escalaInicialFinDemo;
    }

    private IEnumerator Start()
    {
        if (panelCierreNarrativo == null ||
            panelFinDemo == null ||
            panelInvitacion == null)
        {
            Debug.LogError(
                "[Creditos] Faltan paneles en el Inspector."
            );
            yield break;
        }

        yield return StartCoroutine(ReproducirSecuencia());
    }

    private IEnumerator ReproducirSecuencia()
    {
        // ======================================
        // 1. CIERRE NARRATIVO
        // ======================================

        ReproducirSonido(sonidoCierre);

        yield return StartCoroutine(
            AnimarEntradaCierre()
        );

        yield return new WaitForSecondsRealtime(tiempoCierre);

        yield return StartCoroutine(
            FadePanel(
                panelCierreNarrativo,
                1f,
                0f,
                salidaCierre
            )
        );

        panelCierreNarrativo.gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(
            pausaEntrePaneles
        );

        // ======================================
        // 2. FIN DE LA DEMO
        // ======================================

        ReproducirSonido(sonidoFinDemo);

        yield return StartCoroutine(
            AnimarEntradaFinDemo()
        );

        yield return new WaitForSecondsRealtime(tiempoFinDemo);

        yield return StartCoroutine(
            FadePanel(
                panelFinDemo,
                1f,
                0f,
                salidaFinDemo
            )
        );

        panelFinDemo.gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(
            pausaEntrePaneles
        );

        // ======================================
        // 3. INVITACIÓN Y CRÉDITOS
        // ======================================

        ReproducirSonido(sonidoInvitacion);

        yield return StartCoroutine(
            FadePanel(
                panelInvitacion,
                0f,
                1f,
                entradaInvitacion
            )
        );

        // Habilitar los botones.
        panelInvitacion.interactable = true;
        panelInvitacion.blocksRaycasts = true;

        // El panel permanece visible.
        // No se oculta automáticamente.

        Debug.Log(
            "[Creditos] Secuencia finalizada. Esperando interacción."
        );
    }

    private void PrepararPanel(CanvasGroup grupo)
    {
        if (grupo == null)
            return;

        grupo.gameObject.SetActive(true);
        grupo.alpha = 0f;
        grupo.interactable = false;
        grupo.blocksRaycasts = false;
    }

    private IEnumerator AnimarEntradaCierre()
    {
        float tiempo = 0f;

        Vector2 origen =
            posicionOriginalCierre +
            Vector2.down * desplazamientoCierre;

        Vector2 destino = posicionOriginalCierre;

        while (tiempo < entradaCierre)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = entradaCierre > 0f
                ? Mathf.Clamp01(tiempo / entradaCierre)
                : 1f;

            float suave = Mathf.SmoothStep(0f, 1f, t);

            panelCierreNarrativo.alpha = suave;

            if (rectCierreNarrativo != null)
            {
                rectCierreNarrativo.anchoredPosition =
                    Vector2.Lerp(origen, destino, suave);
            }

            yield return null;
        }

        panelCierreNarrativo.alpha = 1f;

        if (rectCierreNarrativo != null)
            rectCierreNarrativo.anchoredPosition = destino;
    }

    private IEnumerator AnimarEntradaFinDemo()
    {
        float tiempo = 0f;

        Vector3 escalaInicial =
            escalaOriginalFinDemo * escalaInicialFinDemo;

        while (tiempo < entradaFinDemo)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = entradaFinDemo > 0f
                ? Mathf.Clamp01(tiempo / entradaFinDemo)
                : 1f;

            float suave = Mathf.SmoothStep(0f, 1f, t);

            panelFinDemo.alpha = suave;

            if (rectFinDemo != null)
            {
                rectFinDemo.localScale =
                    Vector3.Lerp(
                        escalaInicial,
                        escalaOriginalFinDemo,
                        suave
                    );
            }

            yield return null;
        }

        panelFinDemo.alpha = 1f;

        if (rectFinDemo != null)
            rectFinDemo.localScale = escalaOriginalFinDemo;
    }

    private IEnumerator FadePanel(
        CanvasGroup grupo,
        float desde,
        float hasta,
        float duracion
    )
    {
        if (grupo == null)
            yield break;

        grupo.alpha = desde;

        if (duracion <= 0f)
        {
            grupo.alpha = hasta;
            yield break;
        }

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                tiempo / duracion
            );

            grupo.alpha = Mathf.Lerp(
                desde,
                hasta,
                t
            );

            yield return null;
        }

        grupo.alpha = hasta;
    }

    private void ReproducirSonido(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
