using System.Collections;
using UnityEngine;

/// <summary>
/// Poner en un GameObject ACTIVO con BoxCollider (Is Trigger) en el pasillo.
/// Pilar es otro objeto distinto, oculto hasta que se cruce el trigger.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class EventoPilarPasillo : MonoBehaviour
{
    [Header("Pilar")]
    [SerializeField] private GameObject pilar;
    [SerializeField] private Animator animatorPilar;
    [Tooltip("Nombre del estado del Animator que contiene la animación de correr. Vacío = dejar el estado por defecto.")]
    [SerializeField] private string animacionCorrer = "Run";
    [SerializeField] private GameObject pilarSentada;

    [Header("Recorrido")]
    [Tooltip("El primer punto es donde aparece. Los siguientes marcan por dónde corre.")]
    [SerializeField] private Transform[] puntosRecorrido;
    [Min(0.1f)][SerializeField] private float velocidad = 4.5f;
    [Min(0f)][SerializeField] private float suavidadGiro = 12f;
    [Min(0f)][SerializeField] private float esperaFinal = 0.1f;

    [Header("Risa")]
    [Tooltip("Preferentemente, un AudioSource 3D en Pilar para que la risa se desplace con ella.")]
    [SerializeField] private AudioSource audioRisa;
    [SerializeField] private bool repetirRisaMientrasCorre = true;
    [Range(0f, 3f)]
    [SerializeField] private float duracionFadeOutRisa = 0.8f;

    [Header("Llanto 3D - Pilar sentada")]
    [Tooltip("AudioSource en Pilar sentada (o en un Empty en su posicion). NO ponerlo en Pilar que corre: se desactiva al terminar.")]
    [SerializeField] private AudioSource audioLlanto;


    public enum MomentoInicioLlanto
    {
        AlSalirDelTrigger,
        AlTerminarRecorrido
    }

    [Tooltip("Al salir del trigger: cuando Lucas termina de cruzarlo. Al terminar recorrido: cuando Pilar deja de correr.")]
    [SerializeField] private MomentoInicioLlanto momentoInicioLlanto = MomentoInicioLlanto.AlSalirDelTrigger;
    [SerializeField] private bool repetirLlanto = true;

    private bool jugadorDentroTrigger;
    private Collider colliderJugadorQueEntro;
    private bool llantoIniciado;
    private bool habilitado;
    private bool ejecutado;
    private Coroutine rutina;
    public bool RecorridoFinalizado { get; private set; }

    private void Awake()
    {
        PrepararEvento();
    }

    public void PrepararEvento()
    {
        if (pilarSentada != null)
            pilarSentada.SetActive(false);
        habilitado = false;
        ejecutado = false;
        jugadorDentroTrigger = false;
        colliderJugadorQueEntro = null;
        llantoIniciado = false;
        RecorridoFinalizado = false;

        if (audioLlanto != null)
        {
            audioLlanto.Stop();
            audioLlanto.playOnAwake = false;
            audioLlanto.spatialBlend = 0f; // Audio 2D

            if (pilar != null && audioLlanto.transform.IsChildOf(pilar.transform))
                Debug.LogWarning("[Pilar] El AudioSource del llanto esta dentro de Pilar que corre. Colocalo en Pilar sentada o en un Empty independiente, o se cortara cuando desaparezca la que corre.");
        }

        if (rutina != null)
        {
            StopCoroutine(rutina);
            rutina = null;
        }

        if (audioRisa != null) audioRisa.Stop();
        if (pilar != null) pilar.SetActive(false);
    }

    public void HabilitarEvento()
    {
        if (!ejecutado) habilitado = true;
    }

    private void OnTriggerEnter(Collider otro)
    {
        if (!habilitado || ejecutado) return;
        bool esJugador = otro.CompareTag("Player") || otro.transform.root.CompareTag("Player");
        if (!esJugador) return;

        if (Act2ManagerDemo.Instance == null ||
            Act2ManagerDemo.Instance.estadoActual != Act2ManagerDemo.Act2State.Psicosis)
            return;

        if (pilar == null || puntosRecorrido == null || puntosRecorrido.Length < 2)
        {
            Debug.LogError("[Pilar] Asigná el modelo de Pilar y al menos 2 puntos de recorrido.");
            return;
        }

        for (int i = 0; i < puntosRecorrido.Length; i++)
        {
            if (puntosRecorrido[i] == null)
            {
                Debug.LogError("[Pilar] Hay un punto de recorrido sin asignar: " + i);
                return;
            }
        }

        habilitado = false;
        ejecutado = true;
        jugadorDentroTrigger = true;
        colliderJugadorQueEntro = otro;
        rutina = StartCoroutine(CorrerPorPasillo());
    }

    private void OnTriggerExit(Collider otro)
    {
        // Se dispara cuando Lucas TERMINA de atravesar el BoxCollider.
        if (!jugadorDentroTrigger || otro != colliderJugadorQueEntro) return;

        jugadorDentroTrigger = false;
        colliderJugadorQueEntro = null;

        if (momentoInicioLlanto == MomentoInicioLlanto.AlSalirDelTrigger)
            IniciarLlanto3D();
    }

    private void IniciarLlanto3D()
    {
        if (!ejecutado || llantoIniciado) return;
        if (pilarSentada != null) pilarSentada.SetActive(true);

        if (audioLlanto == null)
        {
            Debug.LogWarning("[Pilar] Falta asignar Audio Llanto (3D) en el Inspector de TriggerPilarPasillo.");
            return;
        }

        if (!audioLlanto.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[Pilar] El objeto del AudioSource del llanto esta desactivado. Debe quedar activo incluso cuando Pilar que corre desaparece.");
            return;
        }


        llantoIniciado = true;

        // El llanto se escucha en 2D, sin depender de la distancia.
        audioLlanto.spatialBlend = 0f;
        audioLlanto.loop = repetirLlanto;
        audioLlanto.volume = 1f;
        audioLlanto.Play();

        Debug.Log("[Pilar] Comenzo el llanto 2D.");

        Debug.Log("[Pilar] Comenzo el llanto 3D en: " + audioLlanto.transform.position);
    }

    public void DetenerLlanto3D()
    {
        if (audioLlanto != null) audioLlanto.Stop();
        llantoIniciado = false;
    }

    private IEnumerator CorrerPorPasillo()
    {
        pilar.transform.position = puntosRecorrido[0].position;
        OrientarHacia(puntosRecorrido[1].position, true);
        pilar.SetActive(true);

        if (animatorPilar != null)
        {
            animatorPilar.applyRootMotion = false;
            if (!string.IsNullOrEmpty(animacionCorrer))
                animatorPilar.Play(animacionCorrer, 0, 0f);
        }

        if (audioRisa != null)
        {
            audioRisa.loop = repetirRisaMientrasCorre;
            audioRisa.Play();
        }

        for (int i = 1; i < puntosRecorrido.Length; i++)
        {
            Vector3 destino = puntosRecorrido[i].position;

            while (Vector3.Distance(pilar.transform.position, destino) > 0.03f)
            {
                OrientarHacia(destino, false);
                pilar.transform.position = Vector3.MoveTowards(
                    pilar.transform.position, destino, velocidad * Time.deltaTime);
                yield return null;
            }
            pilar.transform.position = destino;
        }

        // Pilar terminó de correr: avisamos AL INSTANTE para iniciar BaseEnemy2.
        RecorridoFinalizado = true;
        // No esperamos a que termine la pausa final o se oculte el modelo.
        if (Act2ManagerDemo.Instance != null)
            Act2ManagerDemo.Instance.NotificarFinRecorridoPilar();

        if (momentoInicioLlanto == MomentoInicioLlanto.AlTerminarRecorrido)
            IniciarLlanto3D();

        if (esperaFinal > 0f)
            yield return new WaitForSeconds(esperaFinal);

        if (audioRisa != null)
            yield return StartCoroutine(FadeOutRisa());

        pilar.SetActive(false);
    }

    private IEnumerator FadeOutRisa()
    {
        if (audioRisa == null || !audioRisa.isPlaying)
            yield break;

        float volumenInicial = audioRisa.volume;
        float tiempo = 0f;

        while (tiempo < duracionFadeOutRisa)
        {
            tiempo += Time.deltaTime;

            audioRisa.volume = Mathf.Lerp(
                volumenInicial,
                0f,
                Mathf.Clamp01(tiempo / Mathf.Max(0.01f, duracionFadeOutRisa))
            );

            yield return null;
        }

        audioRisa.Stop();
        audioRisa.volume = volumenInicial;
    }

    private void OrientarHacia(Vector3 destino, bool instantaneo)
    {
        Vector3 direccion = destino - pilar.transform.position;
        direccion.y = 0f;
        if (direccion.sqrMagnitude < 0.001f) return;

        Quaternion objetivo = Quaternion.LookRotation(direccion.normalized, Vector3.up);
        pilar.transform.rotation = instantaneo
            ? objetivo
            : Quaternion.Slerp(pilar.transform.rotation, objetivo,
                suavidadGiro * Time.deltaTime);
    }

    [ContextMenu("TEST: Reiniciar evento Pilar")]
    private void ReiniciarEventoDesdeInspector()
    {
        PrepararEvento();
        HabilitarEvento();
        Debug.Log("[Pilar] Evento rearmado. Cruzá el trigger.");
    }
}
