using UnityEngine;
using System.Collections;

/// <summary>
/// Controla el estado visual de "psicosis" del Acto 2:
///  - Cambia el FOV de la cámara (visión distorsionada)
///  - Activa/desactiva una overlay de colores sobre la pantalla
///  - Mueve el ruido de estática/vignette más rápido
///  - Aumenta el consumo de batería de la linterna (si usa Flashlight_Act2)
///
/// SETUP: Colocar en un GameObject vacío llamado "EfectoPsicosis" en la escena.
/// Asignar la cámara, el panel de overlay y el AudioSource de estática.
/// </summary>
public class EfectoPsicosis : MonoBehaviour
{
    [Header("Cámara")]
    public Camera camaraPrincipal;
    public float fovNormal = 60f;
    public float fovPsicosis = 75f;
    [Tooltip("Velocidad de interpolación del FOV")]
    public float velocidadFOV = 3f;

    [Header("Overlay Visual")]
    [Tooltip("Panel UI de color rojo/verde semitransparente que cubre la pantalla")]
    public CanvasGroup overlayPsicosis;
    [Tooltip("Alpha máximo del overlay (0.0 = invisible, 0.4 = fuerte sin cegar)")]
    public float alphaMaximoOverlay = 0.35f;

    [Header("Efecto de Pulso")]
    [Tooltip("El overlay pulsa (se oscurece y aclara) representando el pánico")]
    public float velocidadPulso = 2.5f;

    [Header("Audio")]
    [Tooltip("Sonido de estática/ruido que suena durante la psicosis")]
    public AudioSource sonidoEstatica;

    [Header("Linterna (Acto 2 mode)")]
    [Tooltip("Referencia a Flashlight_Act2 para aumentar el consumo de batería")]
    public Flashlight_Act2 linterna;
    public float multiplicadorConsumoBateria = 2.5f;

    private bool psicosisActiva = false;
    private float fovOriginal;
    private Coroutine coroutinaPulso;

    void Start()
    {
        if (camaraPrincipal == null) camaraPrincipal = Camera.main;
        if (camaraPrincipal != null) fovOriginal = camaraPrincipal.fieldOfView;

        if (overlayPsicosis != null) overlayPsicosis.alpha = 0f;
    }

    void Update()
    {
        if (camaraPrincipal == null) return;

        float fovObjetivo = psicosisActiva ? fovPsicosis : fovNormal;
        camaraPrincipal.fieldOfView = Mathf.Lerp(
            camaraPrincipal.fieldOfView,
            fovObjetivo,
            Time.deltaTime * velocidadFOV
        );
    }

    /// <summary>
    /// Activa el estado de psicosis. Llamar desde Act2Manager.
    /// </summary>
    public void ActivarPsicosis()
    {
        psicosisActiva = true;

        if (sonidoEstatica != null) sonidoEstatica.Play();

        // Aumenta el consumo de batería de la linterna en este estado de pánico
        if (linterna != null)
            linterna.consumptionRate *= multiplicadorConsumoBateria;

        if (overlayPsicosis != null)
        {
            coroutinaPulso = StartCoroutine(PulsarOverlay());
        }
    }

    /// <summary>
    /// Desactiva el estado de psicosis. Llamar desde Act2Manager al final del acto.
    /// </summary>
    public void DesactivarPsicosis()
    {
        psicosisActiva = false;

        if (sonidoEstatica != null) sonidoEstatica.Stop();

        // Restaurar consumo normal de batería
        if (linterna != null)
            linterna.consumptionRate /= multiplicadorConsumoBateria;

        if (coroutinaPulso != null) StopCoroutine(coroutinaPulso);
        StartCoroutine(ApagarOverlay());
    }

    IEnumerator PulsarOverlay()
    {
        while (psicosisActiva)
        {
            float t = Mathf.Sin(Time.time * velocidadPulso) * 0.5f + 0.5f;
            if (overlayPsicosis != null)
                overlayPsicosis.alpha = Mathf.Lerp(alphaMaximoOverlay * 0.3f, alphaMaximoOverlay, t);
            yield return null;
        }
    }

    IEnumerator ApagarOverlay()
    {
        if (overlayPsicosis == null) yield break;

        float alphaInicial = overlayPsicosis.alpha;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            overlayPsicosis.alpha = Mathf.Lerp(alphaInicial, 0f, t);
            yield return null;
        }
        overlayPsicosis.alpha = 0f;
    }
}
