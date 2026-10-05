using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

/// <summary>
/// Efecto "Dolly Zoom" / corredor infinito.
/// Al activarse, el FOV de la cámara aumenta mientras la cámara avanza,
/// haciendo que el pasillo parezca alargarse infinitamente (truco hitchcockiano).
///
/// SETUP: Colocar este script en un trigger box que cubra la entrada del pasillo.
/// Asignar la Main Camera en el Inspector.
/// El efecto se desactiva solo cuando el jugador llega al final del pasillo.
/// </summary>
public class PasilloEfecto : MonoBehaviour
{
    [Header("Cámara")]
    [Tooltip("Arrastrar aquí la Main Camera del jugador")]
    public CinemachineCamera cinemachineCam;

    [Header("Configuración del Efecto")]
    public float fovNormal = 60f;
    public float fovMaximo = 90f;
    [Tooltip("Qué tan rápido se expande el FOV")]
    public float velocidadFOV = 8f;

    [Header("Distorsión Adicional")]
    [Tooltip("Desplazamiento de cámara desactivado — era 0 para evitar que el cápsule del jugador tape la pantalla")]
    [HideInInspector] public float desplazamientoCamara = 0f;
    [HideInInspector] public float velocidadDesplazamiento = 2f;

    [Header("Audio Ambiente del Pasillo")]
    public AudioSource ambiencePasillo;
    [Tooltip("Sonido de latido/tensión que crece en el pasillo")]
    public AudioSource sonidoTension;

    [Header("Duración")]
    [Tooltip("Segundos hasta que el efecto llega al máximo antes de que el jugador avance")]
    public float duracionEfecto = 8f;

    private bool efectoActivo = false;
    private float fovOriginal;
    private Vector3 posOriginalCamara;
    private Coroutine coroutinaEfecto;

    void Start()
    {
        if (cinemachineCam != null)
        {
            fovOriginal = cinemachineCam.Lens.FieldOfView;
        }
    }

    /// <summary>
    /// Llamado por Act2Manager cuando el jugador se dirige a la cocina.
    /// </summary>
    public void ActivarEfecto()
    {
        if (efectoActivo) return;
        efectoActivo = true;

        if (ambiencePasillo != null) ambiencePasillo.Play();
        if (sonidoTension != null)   sonidoTension.Play();

        coroutinaEfecto = StartCoroutine(EfectoCorredorInfinito());
    }

    public void DesactivarEfecto()
    {
        efectoActivo = false;

        if (coroutinaEfecto != null) StopCoroutine(coroutinaEfecto);
        StartCoroutine(RestaurarFOV());

        if (ambiencePasillo != null) ambiencePasillo.Stop();
        if (sonidoTension != null)   sonidoTension.Stop();

        // Deshabilitar el trigger para que nunca vuelva a disparar el efecto
        foreach (Collider col in GetComponents<Collider>())
            col.enabled = false;
    }

    IEnumerator EfectoCorredorInfinito()
    {
        float tiempoTranscurrido = 0f;

        while (efectoActivo && tiempoTranscurrido < duracionEfecto)
        {
            tiempoTranscurrido += Time.deltaTime;

            // Expande el FOV gradualmente hasta el máximo
            if (cinemachineCam != null)
            {
                float fovObjetivo = Mathf.Lerp(fovNormal, fovMaximo, tiempoTranscurrido / duracionEfecto);
                cinemachineCam.Lens.FieldOfView = Mathf.Lerp(
                cinemachineCam.Lens.FieldOfView,
                fovObjetivo,
                Time.deltaTime * velocidadFOV
                );

            }

            // El audio de tensión sube gradualmente
            if (sonidoTension != null)
            {
                sonidoTension.volume = Mathf.Lerp(0f, 1f, tiempoTranscurrido / duracionEfecto);
            }

            yield return null;
        }

        // Mantener en máximo hasta que se desactive manualmente
        if (cinemachineCam != null)
            cinemachineCam.Lens.FieldOfView = fovMaximo;
    }

    IEnumerator RestaurarFOV()
    {
        float t = 0f;
        float fovInicio = cinemachineCam.Lens.FieldOfView;

        while (t < 1f)
        {
            t += Time.deltaTime * 2f;

            cinemachineCam.Lens.FieldOfView =
                Mathf.Lerp(fovInicio, fovOriginal, t);

            yield return null;
        }
    }

    // Trigger opcional: si el jugador entra al pasillo activa automáticamente
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            ActivarEfecto();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            DesactivarEfecto();
    }
}
