using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// NOCHE 2 — Zona que se limpia MANTENIENDO [E] (montículo para barrer, parte de la barra,
/// exhibidor o inodoro).
///
/// El Act2Manager encuentra solo todas las zonas de la escena y cuenta cuántas hay por tarea,
/// así que no hace falta cargar totales a mano.
///
/// EXTRAS DEL GUION:
///  - "manchaCercana": la mancha oscura que está cerca de un montículo. Cuando falta UN solo
///    montículo, el Manager activa la mancha de ese montículo (ver ManchaFantasmaAct2).
///  - "susurroAlLimpiar": voz muy baja que suena al pasar el trapo por el exhibidor.
///
/// SETUP:
///  - Collider en el objeto + capa "Interactable".
///  - suciedadVisual: el montículo / la mancha que se achica mientras se limpia.
/// </summary>
public class ZonaLimpiezaAct2 : MonoBehaviour, IInteractable
{
    [Header("Tarea")]
    public TareaAct2 tarea = TareaAct2.Barrer;
    public HerramientaAct2 herramientaRequerida = HerramientaAct2.Automatica;
    [Tooltip("Texto del cartel de interacción. Vacío = automático según la tarea.")]
    public string textoAccion = "";
    [Tooltip("Segundos manteniendo [E] para completar. 0 = instantáneo.")]
    public float tiempoNecesario = 2f;
    [Tooltip("Si el jugador se aleja más que esto, deja de limpiar.")]
    public float distanciaMaxima = 3.5f;

    [Header("Visual")]
    [Tooltip("Montículo / mancha / suciedad que se achica mientras se limpia y desaparece al final.")]
    public GameObject suciedadVisual;

    [Header("Audio")]
    [Tooltip("Loop de barrido / frotado mientras se mantiene [E].")]
    public AudioSource sonidoLimpieza;

    [Header("Evento: mancha fantasma (Tarea 1)")]
    [Tooltip("Mancha oscura cercana a este montículo. Se activa sola cuando este es el ÚLTIMO montículo.")]
    public ManchaFantasmaAct2 manchaCercana;

    [Header("Evento: susurro (Tarea 2 — exhibidor)")]
    [Tooltip("Voz muy baja que suena una sola vez mientras se limpia esta zona.")]
    public AudioSource susurroAlLimpiar;
    [Range(0f, 1f)]
    [Tooltip("En qué porcentaje de la limpieza suena el susurro.")]
    public float momentoSusurro = 0.5f;

    [Header("Eventos extra (opcional)")]
    public UnityEvent alCompletar;

    /// <summary>Se dispara al terminar de limpiar (lo usa SombraPasaCubiculoAct2).</summary>
    public event System.Action<ZonaLimpiezaAct2> Completada;

    public bool EstaCompletada { get; private set; }

    private bool limpiando;
    private bool susurroReproducido;
    private float progreso;
    private Vector3 escalaInicial;
    private Transform jugador;

    void Start()
    {
        if (suciedadVisual != null) escalaInicial = suciedadVisual.transform.localScale;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;

        if (sonidoLimpieza != null) { sonidoLimpieza.playOnAwake = false; sonidoLimpieza.loop = true; }
    }

    // ----------------------------------------------------------
    // IInteractable
    // ----------------------------------------------------------
    public bool CanInteract()
    {
        if (EstaCompletada) return false;
        Act2Manager m = Act2Manager.Instance;
        return m == null || m.PuedeHacerTarea(tarea);
    }

    public string GetDescription()
    {
        HerramientaAct2 h = HerramientaNecesaria();
        Act2Manager m = Act2Manager.Instance;
        if (m != null && !m.TieneHerramienta(h))
            return "Necesitás " + NombreHerramienta(h);

        if (!string.IsNullOrEmpty(textoAccion)) return textoAccion;

        switch (tarea)
        {
            case TareaAct2.Barrer: return "Mantené [E] para barrer";
            case TareaAct2.Barra:  return "Mantené [E] para limpiar";
            default:               return "Mantené [E] para limpiar el inodoro";
        }
    }

    public void Interact()
    {
        if (EstaCompletada) return;

        Act2Manager m = Act2Manager.Instance;
        if (m != null && !m.TieneHerramienta(HerramientaNecesaria())) return;

        if (tiempoNecesario <= 0f) { Completar(); return; }
        limpiando = true;
    }

    // ----------------------------------------------------------
    // Progreso
    // ----------------------------------------------------------
    void Update()
    {
        if (EstaCompletada) return;

        bool sigue = limpiando && Input.GetKey(KeyCode.E) && JugadorCerca();
        if (!sigue)
        {
            limpiando = false;
            if (sonidoLimpieza != null && sonidoLimpieza.isPlaying) sonidoLimpieza.Stop();
            return;
        }

        if (sonidoLimpieza != null && !sonidoLimpieza.isPlaying) sonidoLimpieza.Play();

        progreso += Time.deltaTime;
        float t = Mathf.Clamp01(progreso / tiempoNecesario);

        if (suciedadVisual != null)
            suciedadVisual.transform.localScale = Vector3.Lerp(escalaInicial, escalaInicial * 0.15f, t);

        if (!susurroReproducido && susurroAlLimpiar != null && t >= momentoSusurro)
        {
            susurroReproducido = true;
            susurroAlLimpiar.Play();
        }

        if (t >= 1f) Completar();
    }

    bool JugadorCerca()
    {
        if (jugador == null) return true;
        return Vector3.Distance(jugador.position, transform.position) <= distanciaMaxima;
    }

    void Completar()
    {
        if (EstaCompletada) return;
        EstaCompletada = true;
        limpiando = false;

        if (sonidoLimpieza != null) sonidoLimpieza.Stop();
        if (suciedadVisual != null) suciedadVisual.SetActive(false);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Completada?.Invoke(this);
        alCompletar?.Invoke();

        Act2Manager.Instance?.RegistrarZonaLimpia(this);
    }

    // ----------------------------------------------------------
    // Utilidades
    // ----------------------------------------------------------
    public HerramientaAct2 HerramientaNecesaria()
    {
        if (herramientaRequerida != HerramientaAct2.Automatica) return herramientaRequerida;
        switch (tarea)
        {
            case TareaAct2.Barrer: return HerramientaAct2.Escoba;
            case TareaAct2.Barra:  return HerramientaAct2.Trapo;
            default:               return HerramientaAct2.Cepillo;
        }
    }

    static string NombreHerramienta(HerramientaAct2 h)
    {
        switch (h)
        {
            case HerramientaAct2.Escoba:  return "la escoba (depósito)";
            case HerramientaAct2.Trapo:   return "el trapo";
            case HerramientaAct2.Cepillo: return "el cepillo";
            default:                      return "una herramienta";
        }
    }

    void OnDisable()
    {
        limpiando = false;
        if (sonidoLimpieza != null) sonidoLimpieza.Stop();
    }
}
