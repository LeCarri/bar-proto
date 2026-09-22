using UnityEngine;

public class CajaMusicalController : MonoBehaviour, IInteractable
{
    [Header("Audio")]
    [SerializeField] private AudioSource musica;

    [Header("Partes móviles")]
    [SerializeField] private Transform espejo;
    [SerializeField] private Transform bailarina;
    [SerializeField] private Transform llave;

    [Header("Inicio y Tiempo")]
    [Tooltip("Tiempo que la caja funciona normalmente antes de empezar a acelerarse.")]
    [SerializeField] private float tiempoNormal = 2f;

    [Header("Control de aceleración")]
    [Tooltip("Cuánto aumenta la velocidad por segundo después del tiempo normal.")]
    [SerializeField] private float aceleracion = 0.25f;

    [Tooltip("Pitch normal de la música.")]
    [SerializeField] private float pitchInicial = 1f;

    [Tooltip("Pitch máximo que puede alcanzar.")]
    [SerializeField] private float pitchMaximo = 2.2f;

    [Header("Volumen")]
    [Range(0f, 1f)]
    [SerializeField] private float volumen = 0.8f;

    [Header("Giros")]
    [Tooltip("Velocidad inicial de las piezas.")]
    [SerializeField] private float velocidadGiroInicial = 30f;

    [Tooltip("Cuánto se exagera la aceleración visual de los giros respecto a la música.")]
    [SerializeField] private float multiplicadorAceleracionGiro = 2f;

    [Tooltip("Multiplicador del giro del espejo.")]
    [SerializeField] private Vector3 ejeEspejo = new Vector3(0, 1, 0);

    [Tooltip("Multiplicador del giro de la bailarina.")]
    [SerializeField] private Vector3 ejeBailarina = new Vector3(0, 0, 1);

    [Tooltip("Multiplicador del giro de la llave.")]
    [SerializeField] private Vector3 ejeLlave = new Vector3(1, 0, 0);

    private bool activa;
    private float tiempoActiva;
    private bool yaFueInteractuada = false;

    private void Start()
    {
        if (musica != null)
        {
            musica.loop = true;
            musica.playOnAwake = false;
            musica.Stop();
        }
    }

    private void Update()
    {
        if (!activa) return;

        tiempoActiva += Time.deltaTime;

        // =========================
        // ACELERACIÓN POR TIEMPO
        // =========================
        float aumentoTiempo = 0f;

        if (tiempoActiva > tiempoNormal)
        {
            float tiempoAcelerando = tiempoActiva - tiempoNormal;
            aumentoTiempo = tiempoAcelerando * aceleracion;
        }

        float intensidad = 1f + aumentoTiempo;

        // =========================
        // MÚSICA
        // =========================
        float pitchActual = Mathf.Clamp(
            pitchInicial * intensidad,
            pitchInicial,
            pitchMaximo
        );

        if (musica != null)
        {
            musica.volume = volumen;
            musica.pitch = pitchActual;
        }

        // =========================
        // GIROS SINCRONIZADOS
        // =========================
        float multiplicadorMusica = pitchActual / pitchInicial;
        float multiplicadorGiro = Mathf.Pow(multiplicadorMusica, multiplicadorAceleracionGiro);
        float velocidadGiro = velocidadGiroInicial * multiplicadorGiro;

        if (espejo != null)
            espejo.Rotate(ejeEspejo * velocidadGiro * Time.deltaTime, Space.Self);

        if (bailarina != null)
            bailarina.Rotate(ejeBailarina * velocidadGiro * Time.deltaTime, Space.Self);

        if (llave != null)
            llave.Rotate(ejeLlave * velocidadGiro * Time.deltaTime, Space.Self);
    }

    public void ActivarCaja()
    {
        if (activa) return;

        activa = true;
        tiempoActiva = 0f;

        if (musica != null)
        {
            musica.volume = volumen;
            musica.pitch = pitchInicial;
            musica.Play();
        }

        Debug.Log("[Caja Musical] Activada.");
    }

    public void DetenerCaja()
    {
        activa = false;

        if (musica != null)
            musica.Stop();
    }

    // ==========================================
    // INTERFAZ IINTERACTABLE (SISTEMA DE TECLA E)
    // ==========================================
    public void Interact()
    {
        if (!CanInteract()) return;

        yaFueInteractuada = true;
        ActivarCaja();

        // Notificamos al Act1Manager para que dispare el susto/secuencia
        if (Act1Manager.Instance != null)
        {
            Act1Manager.Instance.InteractuarCajaMusical();
        }
    }

    public string GetDescription()
    {
        if (CanInteract())
            return "Presiona [E] para examinar la caja musical";

        return "";
    }

    public bool CanInteract()
    {
        return !yaFueInteractuada && 
               Act1Manager.Instance != null && 
               Act1Manager.Instance.estadoActual == Act1Manager.ActoState.Quiebre;
    }
}