using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Copia de EfectoPsicosis.cs adaptada a la reestructura.
/// (El original sigue intacto para la demo.)
///
/// Estado visual de "psicosis" durante el combate:
///  - Abre el FOV de la cámara (visión distorsionada).
///  - Hace pulsar un panel de color sobre la pantalla.
///  - Hace sonar la estática.
///
/// DIFERENCIA CON EL ORIGINAL: el original forzaba el FOV todo el tiempo y eso anulaba el efecto del
/// pasillo. Esta copia solo toca el FOV mientras la psicosis está activa y mientras vuelve a la normalidad.
///
/// SETUP: GameObject vacío "EfectoPsicosis" con este script. Asignar la CinemachineCamera del jugador,
/// el panel de overlay (con CanvasGroup) y el AudioSource de estática.
/// </summary>
public class EfectoPsicosisReestructura : MonoBehaviour
{
    [Header("Cámara")]
    public CinemachineCamera cinemachineCam;
    public float fovNormal = 60f;
    public float fovPsicosis = 75f;
    [Tooltip("Velocidad de interpolación del FOV.")]
    public float velocidadFOV = 3f;

    [Header("Overlay visual")]
    [Tooltip("Panel UI de color rojo/verde semitransparente que cubre la pantalla (con CanvasGroup).")]
    public CanvasGroup overlayPsicosis;
    [Tooltip("Alpha máximo del overlay (0 = invisible, 0.4 = fuerte sin cegar).")]
    public float alphaMaximoOverlay = 0.35f;
    [Tooltip("Velocidad del pulso del overlay.")]
    public float velocidadPulso = 2.5f;

    [Header("Audio")]
    [Tooltip("Estática/ruido que suena durante la psicosis.")]
    public AudioSource sonidoEstatica;

    private bool psicosisActiva;
    private bool restaurandoFOV;
    private Coroutine rutinaPulso;

    void Start()
    {
        if (overlayPsicosis != null) overlayPsicosis.alpha = 0f;
    }

    void Update()
    {
        if (cinemachineCam == null) return;
        if (!psicosisActiva && !restaurandoFOV) return;   // no pisa el FOV del pasillo

        float fovObjetivo = psicosisActiva ? fovPsicosis : fovNormal;
        LensSettings lens = cinemachineCam.Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, fovObjetivo, Time.deltaTime * velocidadFOV);
        cinemachineCam.Lens = lens;

        if (!psicosisActiva && Mathf.Abs(lens.FieldOfView - fovNormal) < 0.1f)
        {
            lens.FieldOfView = fovNormal;
            cinemachineCam.Lens = lens;
            restaurandoFOV = false;
        }
    }

    /// <summary>Activa la psicosis (la llama el Act2ManagerReestructura al empezar el combate).</summary>
    public void ActivarPsicosis()
    {
        if (psicosisActiva) return;
        psicosisActiva = true;
        restaurandoFOV = false;

        if (sonidoEstatica != null) sonidoEstatica.Play();
        if (overlayPsicosis != null) rutinaPulso = StartCoroutine(PulsarOverlay());
    }

    /// <summary>Desactiva la psicosis (la llama el Manager al abrir la puerta del sótano).</summary>
    public void DesactivarPsicosis()
    {
        if (!psicosisActiva) return;
        psicosisActiva = false;
        restaurandoFOV = true;

        if (sonidoEstatica != null) sonidoEstatica.Stop();
        if (rutinaPulso != null) StopCoroutine(rutinaPulso);
        StartCoroutine(ApagarOverlay());
    }

    IEnumerator PulsarOverlay()
    {
        while (psicosisActiva)
        {
            float t = Mathf.Sin(Time.time * velocidadPulso) * 0.5f + 0.5f;
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
