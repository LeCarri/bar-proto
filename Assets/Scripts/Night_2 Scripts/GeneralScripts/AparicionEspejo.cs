using UnityEngine;
using System.Collections;

public class AparicionEspejo : MonoBehaviour
{
    [Header("Aparición Frontal")]
    [Tooltip("El modelo 3D de la figura/Vigilante ubicado frente a la salida del baño")]
    public GameObject modeloVigilante;
    public float duracionAparicion = 1.5f;

    [Header("Luz del Entorno y Linterna")]
    [Tooltip("Luz principal del baño o pasillo para el efecto de parpadeo")]
    public Light luzBano; 
    [Tooltip("Luz de la linterna del jugador (si no se asigna, intentará buscarla)")]
    public Light linternaJugador;

    [Header("Audio")]
    public AudioSource sonidoSusto;
    public AudioClip sonidoJumpscare;
    public AudioClip sonidoPortazo;
    public AudioClip sonidoFalloLinterna; // Opcional: Sonido de 'click' o chispazo

    [Header("Efecto Cámara")]
    public CameraShake cameraShake;
    public float duracionShake = 0.5f;
    public float intensidadShake = 2.0f;

    private bool yaSeDisparo = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Trigger] Algo tocó el collider: {other.name} con tag {other.tag}");

        if (yaSeDisparo) return;

        if (other.CompareTag("Player"))
        {
            if (Act2ManagerDemo.Instance == null)
            {
                Debug.LogError("[AparicionEspejo] ERROR: Act2ManagerDemo.Instance es NULL en la escena.");
                return;
            }

            // Solo se dispara si el jugador ya agarró la llave
            if (Act2ManagerDemo.Instance.llaveTenida)
            {
                yaSeDisparo = true;
                StartCoroutine(EjecutarAparicionFrontal());
            }
        }
    }

    private IEnumerator EjecutarAparicionFrontal()
    {
        Debug.Log("[AparicionEspejo] ¡Iniciando jumpscare frontal!");

        // 1. Notificar cambio de estado a Psicosis en el Manager
        if (Act2ManagerDemo.Instance != null)
        {
            Act2ManagerDemo.Instance.LlaveRecogida();
        }

        // --- MANEJO DE LINTERNA ---
        // Si no asignaste la luz de la linterna en el inspector, intentamos encontrarla
        if (linternaJugador == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                // Busca una Light dentro de los hijos del Player
                linternaJugador = player.GetComponentInChildren<Light>();
            }
        }

        bool linternaEstabaEncendida = false;
        if (linternaJugador != null && linternaJugador.enabled)
        {
            linternaEstabaEncendida = true;
            
            // Sonido opcional de fallo mecánico/eléctrico
            if (sonidoSusto != null && sonidoFalloLinterna != null)
            {
                sonidoSusto.PlayOneShot(sonidoFalloLinterna);
            }

            // Titileo rápido de la linterna antes de morir
            linternaJugador.enabled = false;
            yield return new WaitForSeconds(0.05f);
            linternaJugador.enabled = true;
            yield return new WaitForSeconds(0.04f);
            linternaJugador.enabled = false; // Se apaga definitivamente
        }

        // 2. Apagón relámpago del baño (120ms)
        if (luzBano != null) luzBano.enabled = false;
        if (sonidoSusto != null && sonidoPortazo != null)
        {
            sonidoSusto.PlayOneShot(sonidoPortazo);
        }

        yield return new WaitForSeconds(0.12f);

        // 3. Activar el modelo pegado al jugador y encender luz del baño + sonido
        if (modeloVigilante != null)
        {
            modeloVigilante.SetActive(true);
        }

        if (luzBano != null) luzBano.enabled = true;

        if (sonidoSusto != null)
        {
            if (sonidoJumpscare != null)
                sonidoSusto.PlayOneShot(sonidoJumpscare);
            else
                sonidoSusto.Play();
        }

        // 4. Zoom de pánico en la cámara (FOV Punch)
        Camera cam = Camera.main;
        float fovOriginal = cam != null ? cam.fieldOfView : 60f;
        if (cam != null) cam.fieldOfView = fovOriginal - 10f;

        // 5. Shake violento
        if (cameraShake != null)
        {
            cameraShake.DispararShake(duracionShake, intensidadShake);
        }

        // Recuperar FOV rápidamente
        float t = 0;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            if (cam != null) cam.fieldOfView = Mathf.Lerp(fovOriginal - 10f, fovOriginal, t / 0.2f);
            yield return null;
        }
        if (cam != null) cam.fieldOfView = fovOriginal;

        // 6. Efecto de luz parpadeante del baño durante la aparición
        float tiempoTranscurrido = 0f;
        while (tiempoTranscurrido < duracionAparicion)
        {
            tiempoTranscurrido += 0.15f;
            if (luzBano != null) luzBano.enabled = !luzBano.enabled;
            yield return new WaitForSeconds(0.15f);
        }

        // 7. Apagón final: La entidad desaparece en la oscuridad
        if (luzBano != null) luzBano.enabled = false;
        if (modeloVigilante != null) modeloVigilante.SetActive(false);
        
        yield return new WaitForSeconds(0.25f);
        
        // Vuelve la luz del entorno pero sin la entidad
        if (luzBano != null) luzBano.enabled = true;

        // Si la linterna estaba encendida antes del susto, la devolvemos encendida (o podrías dejarla apagada para mayor tensión)
        if (linternaJugador != null && linternaEstabaEncendida)
        {
            linternaJugador.enabled = true;
        }

        // 8. Iniciar secuencia de Psicosis
        if (Act2ManagerDemo.Instance != null)
        {
            Act2ManagerDemo.Instance.StartCoroutine(Act2ManagerDemo.Instance.SecuenciaPsicosis());
        }

        gameObject.SetActive(false);
    }
}