using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

/// <summary>
/// Controla el estado visual de "psicosis" del Acto 2:
///  - Activa/desactiva una overlay de colores sobre la pantalla
///  - Mueve el ruido de estática/vignette más rápido
///  - (FOV de cámara desactivado para evitar distorsiones no deseadas)
///
/// SETUP: Colocar en un GameObject vacío llamado "EfectoPsicosis" en la escena.
/// Asignar la cámara, el panel de overlay y el AudioSource de estática.
/// </summary>
public class EfectoPsicosis : MonoBehaviour
{
    [Header("Cámara")]
    public CinemachineCamera cinemachineCam;
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

    private bool psicosisActiva = false;
    private Coroutine coroutinaPulso;

    void Start()
    {
        if (overlayPsicosis != null) overlayPsicosis.alpha = 0f;
    }

    void Update()
    {
        // --- CAMBIO DE FOV DESACTIVADO ---
        // Se comenta este bloque para evitar tiradones de zoom y cambios bruscos de campo de visión.
        /*
        if (cinemachineCam == null) return;

        float fovObjetivo = psicosisActiva ? fovPsicosis : fovNormal;
        var lens = cinemachineCam.Lens;
        lens.FieldOfView = Mathf.Lerp(
            lens.FieldOfView,
            fovObjetivo,
            Time.deltaTime * velocidadFOV
        );
        cinemachineCam.Lens = lens;
        */
    }

    /// <summary>
    /// Activa el estado de psicosis. Llamar desde Act2Manager.
    /// </summary>
    public void ActivarPsicosis()
    {
        psicosisActiva = true;

        if (sonidoEstatica != null) sonidoEstatica.Play();

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