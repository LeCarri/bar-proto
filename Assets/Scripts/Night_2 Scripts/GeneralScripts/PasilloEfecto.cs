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

    // ================= REESTRUCTURA NOCHE 2 =================
    [Header("Reestructura Noche 2")]
    [Tooltip("Solo funciona cuando el Act2Manager está en la fase Pasillo (después del pedido del cliente corrupto). " +
             "Así no se dispara cuando vas al depósito a buscar la escoba en las tareas.")]
    public bool soloEnFasePasillo = true;

    [Tooltip("\"La música del bar empieza a alejarse. Cada vez más.\" Arrastrá la música/ambiente del bar.")]
    public AudioSource[] musicaQueSeAleja;

    [TextArea] public string dialogoPasillo = "Lucas: ¿Siempre fue tan largo?";
    public float segundosHastaDialogo = 2.5f;

    [Tooltip("Hacia dónde está el depósito. Vacío = el eje azul (forward) de este trigger.")]
    public Transform direccionDeposito;
    [Tooltip("FOV cuando el jugador mira hacia atrás: el bar parece mucho más lejos.")]
    public float fovMirandoAtras = 115f;
    // REVERTIDO: ya no se usa (el efecto solo se apaga al encontrar los zapatos)
    [HideInInspector] public bool normalizarAlVolverAMirar = false;

    private bool miroHaciaAtras = false;
    private bool dialogoMostrado = false;
    private float[] volumenesMusica;
    // =========================================================

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

        // REESTRUCTURA: guardar el volumen de la música para alejarla
        if (musicaQueSeAleja != null)
        {
            volumenesMusica = new float[musicaQueSeAleja.Length];
            for (int i = 0; i < musicaQueSeAleja.Length; i++)
                volumenesMusica[i] = musicaQueSeAleja[i] != null ? musicaQueSeAleja[i].volume : 0f;
        }
        miroHaciaAtras = false;

        coroutinaEfecto = StartCoroutine(EfectoCorredorInfinito());
    }

    public void DesactivarEfecto()
    {
        efectoActivo = false;

        if (coroutinaEfecto != null) StopCoroutine(coroutinaEfecto);
        // StartCoroutine(RestaurarFOV());   // VERSIÓN ANTERIOR (fallaba si no había cámara asignada)
        if (cinemachineCam != null && isActiveAndEnabled) StartCoroutine(RestaurarFOV());

        if (ambiencePasillo != null) ambiencePasillo.Stop();
        if (sonidoTension != null)   sonidoTension.Stop();

        // Deshabilitar el trigger para que nunca vuelva a disparar el efecto
        foreach (Collider col in GetComponents<Collider>())
            col.enabled = false;
    }

    // ================= REESTRUCTURA NOCHE 2 =================
    // Versión nueva del corredor: además del FOV, la música del bar se aleja, Lucas dice
    // "¿Siempre fue tan largo?", y si el jugador mira hacia atrás el bar parece muchísimo más lejos.
    // El efecto sigue activo hasta que Act2Manager.ZapatosEncontrados() lo apaga para el resto de la noche.
    IEnumerator EfectoCorredorInfinito()
    {
        float tiempoTranscurrido = 0f;
        Transform cam = Camera.main != null ? Camera.main.transform : null;

        while (efectoActivo)
        {
            tiempoTranscurrido += Time.deltaTime;
            float progreso = Mathf.Clamp01(tiempoTranscurrido / Mathf.Max(0.01f, duracionEfecto));

            // Diálogo
            if (!dialogoMostrado && tiempoTranscurrido >= segundosHastaDialogo && !string.IsNullOrEmpty(dialogoPasillo))
            {
                dialogoMostrado = true;
                Act2Manager.Instance?.MostrarDialogo(dialogoPasillo);
            }

            // La música del bar se aleja
            if (musicaQueSeAleja != null && volumenesMusica != null)
                for (int i = 0; i < musicaQueSeAleja.Length; i++)
                    if (musicaQueSeAleja[i] != null)
                        musicaQueSeAleja[i].volume = Mathf.Lerp(volumenesMusica[i], 0f, progreso);

            // ¿Mira hacia atrás?
            if (cam != null)
            {
                Vector3 haciaDeposito = direccionDeposito != null ? direccionDeposito.forward : transform.forward;
                haciaDeposito.y = 0f;
                Vector3 mirada = cam.forward;
                mirada.y = 0f;
                float dot = Vector3.Dot(mirada.normalized, haciaDeposito.normalized);

                // Mientras mira hacia atrás, el bar parece mucho más lejos. Al volver a mirar adelante
                // sigue el efecto normal del pasillo: NO se apaga (se apaga recién al encontrar los zapatos).
                miroHaciaAtras = dot < -0.3f;

                // ---- REVERTIDO: esto apagaba el efecto para siempre ANTES de llegar a los zapatos ----
                // if (dot < -0.3f) miroHaciaAtras = true;
                // else if (miroHaciaAtras && dot > 0.6f && normalizarAlVolverAMirar)
                // {
                //     DesactivarEfecto();
                //     yield break;
                // }
            }

            if (cinemachineCam != null)
            {
                float fovObjetivo = miroHaciaAtras ? fovMirandoAtras : Mathf.Lerp(fovNormal, fovMaximo, progreso);
                cinemachineCam.Lens.FieldOfView = Mathf.Lerp(cinemachineCam.Lens.FieldOfView, fovObjetivo, Time.deltaTime * velocidadFOV);
            }

            if (sonidoTension != null)
                sonidoTension.volume = progreso;

            yield return null;
        }
    }

    /* ---- VERSIÓN ANTERIOR (comentada en la reestructura) ----
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
    ---- FIN VERSIÓN ANTERIOR ---- */

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
        // ---- VERSIÓN ANTERIOR ----
        // if (other.CompareTag("Player"))
        //     ActivarEfecto();

        // ---- REESTRUCTURA: solo en la fase Pasillo ----
        if (other.CompareTag("Player") && EsFasePasillo())
            ActivarEfecto();
    }

    // REESTRUCTURA: si el jugador ya estaba adentro del trigger cuando empieza la fase Pasillo
    void OnTriggerStay(Collider other)
    {
        if (!efectoActivo && !dialogoMostrado && other.CompareTag("Player") && EsFasePasillo())
            ActivarEfecto();
    }

    void OnTriggerExit(Collider other)
    {
        // ---- VERSIÓN ANTERIOR ----
        // if (other.CompareTag("Player"))
        //     DesactivarEfecto();

        // ---- REESTRUCTURA: solo si el efecto estaba activo (si no, apagaba el trigger para siempre al pasar en las tareas) ----
        if (other.CompareTag("Player") && efectoActivo)
            DesactivarEfecto();
    }

    bool EsFasePasillo()
    {
        if (!soloEnFasePasillo) return true;
        Act2Manager m = Act2Manager.Instance;
        return m == null || m.estadoActual == Act2Manager.Act2State.Pasillo;
    }
}
