using UnityEngine;

public class CajaMusicalController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource musica;

    [Header("Partes móviles")]
    [SerializeField] private Transform espejo;
    [SerializeField] private Transform bailarina;
    [SerializeField] private Transform llave;

    [Header("Jugador")]
    [SerializeField] private Transform jugador;

    [Header("Inicio")]
    [Tooltip("Tiempo que la caja funciona normalmente antes de empezar a acelerarse.")]
    [SerializeField] private float tiempoNormal = 3f;

    [Header("Control de aceleración")]
    [Tooltip("Cuánto aumenta la velocidad por segundo después del tiempo normal.")]
    [SerializeField] private float aceleracion = 0.05f;

    [Tooltip("Pitch normal de la música.")]
    [SerializeField] private float pitchInicial = 1f;

    [Tooltip("Pitch máximo que puede alcanzar.")]
    [SerializeField] private float pitchMaximo = 2f;

    [Header("Volumen")]
    [Range(0f, 1f)]
    [SerializeField] private float volumen = 0.8f;

    [Header("Proximidad")]
    [SerializeField] private float distanciaMaxima = 12f;
    [SerializeField] private float distanciaMinima = 2f;

    [Tooltip("Aceleración adicional cuando Lucas se acerca.")]
    [SerializeField] private float aceleracionPorProximidad = 0.5f;

    [Header("Giros")]
    [Tooltip("Velocidad inicial de las piezas.")]
    [SerializeField] private float velocidadGiroInicial = 30f;

    [Tooltip("Cuánto se exagera la aceleración visual de los giros respecto a la música.")]
    [SerializeField] private float multiplicadorAceleracionGiro = 2f;

    [Tooltip("Multiplicador del giro del espejo. Probá X, Y o Z.")]
    [SerializeField] private Vector3 ejeEspejo = new Vector3(0, 1, 0);

    [Tooltip("Multiplicador del giro de la bailarina. Probá X, Y o Z.")]
    [SerializeField] private Vector3 ejeBailarina = new Vector3(0, 0, 1);

    [Tooltip("Multiplicador del giro de la llave. Probá X, Y o Z.")]
    [SerializeField] private Vector3 ejeLlave = new Vector3(1, 0, 0);

    private bool activa;
    private float tiempoActiva;

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
        // SOLO PARA PRUEBAS
        if (Input.GetKeyDown(KeyCode.M))
        {
            ActivarCaja();
        }

        if (!activa)
            return;

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

        // =========================
        // ACELERACIÓN POR CERCANÍA
        // =========================

        float proximidad = 0f;

        if (jugador != null)
        {
            float distancia = Vector3.Distance(
                jugador.position,
                transform.position
            );

            proximidad = Mathf.InverseLerp(
                distanciaMaxima,
                distanciaMinima,
                distancia
            );
        }

        float aumentoCercania =
            proximidad * aceleracionPorProximidad;

        // =========================
        // INTENSIDAD TOTAL
        // =========================

        float intensidad =
            1f + aumentoTiempo + aumentoCercania;

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

        // Cuánto se aceleró realmente la música.
        // Ejemplo:
        // pitch 1.0 = 1x
        // pitch 1.5 = 1.5x

        float multiplicadorMusica = pitchActual / pitchInicial;

        // Exageramos visualmente esa aceleración.
        // Con multiplicadorAceleracionGiro = 2:
        // 1.0 -> 1x
        // 1.5 -> 2.25x

        float multiplicadorGiro = Mathf.Pow(
            multiplicadorMusica,
            multiplicadorAceleracionGiro
        );

        float velocidadGiro =
            velocidadGiroInicial * multiplicadorGiro;

        if (espejo != null)
        {
            espejo.Rotate(
                ejeEspejo * velocidadGiro * Time.deltaTime,
                Space.Self
            );
        }

        if (bailarina != null)
        {
            bailarina.Rotate(
                ejeBailarina * velocidadGiro * Time.deltaTime,
                Space.Self
            );
        }

        if (llave != null)
        {
            llave.Rotate(
                ejeLlave * velocidadGiro * Time.deltaTime,
                Space.Self
            );
        }
    }

    public void ActivarCaja()
    {
        if (activa)
            return;

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
}