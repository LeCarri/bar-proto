using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Copia de PasilloEfecto.cs adaptada a la reestructura.
/// (El original sigue intacto para la demo.)
///
/// "3. ATRAVESAMOS EL PASILLO": efecto Dolly Zoom. A medida que el jugador avanza, el FOV se abre
/// justo lo necesario para que la puerta del fondo NO parezca acercarse ("la puerta parece que nunca
/// se acerca") mientras las paredes se estiran. La música del bar se aleja, Lucas dice
/// "¿Siempre fue tan largo?" y si el jugador mira hacia atrás el bar parece muchísimo más lejos.
///
/// CUÁNDO FUNCIONA:
///  - Solo en la fase Pasillo, o sea DESPUÉS de que el cliente corrupto hace el pedido.
///    En las tareas el jugador pasa por acá a buscar la escoba y no pasa nada.
///  - Si el jugador sale del trigger, el efecto se pausa (el FOV vuelve a la normalidad) y se
///    retoma si vuelve a entrar.
///  - Al encontrar los zapatos, el Act2ManagerReestructura lo apaga PARA EL RESTO DE LA NOCHE.
///
/// SETUP: en un GameObject con Box Collider "Is Trigger" que cubra el pasillo hacia el depósito
/// (de punta a punta). Asignar la CinemachineCamera del jugador. La dirección del depósito y el fondo
/// del pasillo se calculan solos con la caja del trigger (el lado largo de la caja, alejándose del
/// lado por donde entra el jugador). Si querés marcarlos a mano, usá "Final Del Pasillo".
/// </summary>
[DefaultExecutionOrder(-100)]   // corre antes que el CinemachineBrain para que el FOV se aplique en el mismo frame
public class PasilloEfectoReestructura : MonoBehaviour
{
    [Header("Cámara")]
    [Tooltip("La CinemachineCamera del jugador (la que se usa al jugar). Vacío = la busca sola.")]
    public CinemachineCamera cinemachineCam;

    [Header("Dolly zoom")]
    [Tooltip("FOV normal del jugador.")]
    public float fovNormal = 60f;
    [Tooltip("FOV máximo al que puede llegar caminando por el pasillo. Más alto = el efecto dura más.")]
    public float fovMaximo = 100f;
    [Tooltip("Qué tan rápido sigue el FOV a su objetivo (más alto = más pegado).")]
    public float suavizado = 6f;
    [Tooltip("OPCIONAL. Un vacío en la puerta del depósito (el fondo del pasillo). Vacío = el fondo de la caja del trigger.")]
    public Transform finalDelPasillo;
    [Tooltip("Si no se puede calcular el fondo del pasillo, el FOV llega al máximo en estos segundos.")]
    public float duracionSinFondo = 8f;

    [Header("Mirar hacia atrás")]
    [Tooltip("FOV cuando el jugador mira hacia atrás: el bar parece mucho más lejos.")]
    public float fovMirandoAtras = 115f;

    [Header("Audio")]
    public AudioSource ambiencePasillo;
    [Tooltip("Sonido de latido/tensión que crece en el pasillo.")]
    public AudioSource sonidoTension;
    [Tooltip("\"La música del bar empieza a alejarse. Cada vez más.\" Arrastrá la música/ambiente del bar.")]
    public AudioSource[] musicaQueSeAleja;

    [Header("Diálogo")]
    [TextArea] public string dialogoPasillo = "Lucas: ¿Siempre fue tan largo?";
    public float segundosHastaDialogo = 2.5f;

    [Header("Testeo")]
    [Tooltip("Dejalo marcado. Si lo desmarcás, el efecto funciona en cualquier fase (solo para probarlo).")]
    public bool soloEnFasePasillo = true;
    [Tooltip("Dibuja en la vista Scene la dirección calculada del depósito (flecha cian) y el fondo del pasillo (esfera).")]
    public bool mostrarAyudas = true;

    // Estado
    private bool efectoActivo;
    private bool apagadoParaSiempre;
    private bool restaurando;
    private bool dialogoMostrado;
    private bool direccionCalculada;
    private float fovActual;
    private float fovOriginal;
    private float tiempoActivo;
    private float distanciaInicial;
    private float progreso;
    private Vector3 haciaDeposito = Vector3.forward;
    private Vector3 puntoFinal;
    private bool hayFondo;
    private float[] volumenesMusica;
    private Transform jugador;
    private Transform camaraJugador;

    void Start()
    {
        if (cinemachineCam == null) cinemachineCam = BuscarCamaraDelJugador();
        if (cinemachineCam != null)
        {
            fovOriginal = cinemachineCam.Lens.FieldOfView;
            if (fovOriginal <= 1f) fovOriginal = fovNormal;
        }
        else
        {
            fovOriginal = fovNormal;
            Debug.LogError($"[PasilloEfectoReestructura] '{name}': no hay 'Cinemachine Cam' — no va a haber efecto de FOV. Asigná la CinemachineCamera del jugador.");
        }

        Collider c = GetComponent<Collider>();
        if (c == null || !c.isTrigger)
            Debug.LogError($"[PasilloEfectoReestructura] '{name}': necesita un Box Collider con 'Is Trigger' marcado.");
    }

    CinemachineCamera BuscarCamaraDelJugador()
    {
        // La de mayor prioridad que esté activa (la del jugador)
        CinemachineCamera mejor = null;
        foreach (CinemachineCamera cam in FindObjectsByType<CinemachineCamera>())
            if (cam.isActiveAndEnabled && (mejor == null || cam.Priority.Value > mejor.Priority.Value)) mejor = cam;
        if (mejor != null) Debug.LogWarning($"[PasilloEfectoReestructura] 'Cinemachine Cam' estaba vacío: se usa '{mejor.name}'. Asignalo a mano para estar seguro.");
        return mejor;
    }

    // ─────────────────────────────────────────────────────────
    //  Trigger
    // ─────────────────────────────────────────────────────────
    void OnTriggerEnter(Collider other) => IntentarActivar(other);
    void OnTriggerStay(Collider other) => IntentarActivar(other);   // por si la fase empieza con el jugador adentro

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && efectoActivo) Pausar();
    }

    void IntentarActivar(Collider other)
    {
        if (efectoActivo || apagadoParaSiempre || !other.CompareTag("Player")) return;
        if (!EsFasePasillo()) return;
        jugador = other.transform;
        ActivarEfecto();
    }

    bool EsFasePasillo()
    {
        if (!soloEnFasePasillo) return true;
        Act2ManagerReestructura m = Act2ManagerReestructura.Instance;
        return m != null && m.estadoActual == Act2ManagerReestructura.Act2State.Pasillo;
    }

    // ─────────────────────────────────────────────────────────
    //  API
    // ─────────────────────────────────────────────────────────
    public void ActivarEfecto()
    {
        if (efectoActivo || apagadoParaSiempre) return;

        if (jugador == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) jugador = p.transform;
        }
        camaraJugador = Camera.main != null ? Camera.main.transform : null;

        if (!direccionCalculada) CalcularDireccion();
        distanciaInicial = hayFondo && jugador != null ? Mathf.Max(0.5f, DistanciaAlFondo()) : 0f;

        efectoActivo = true;
        restaurando = false;
        tiempoActivo = 0f;
        progreso = 0f;
        fovActual = cinemachineCam != null ? cinemachineCam.Lens.FieldOfView : fovNormal;

        if (ambiencePasillo != null) ambiencePasillo.Play();
        if (sonidoTension != null) { sonidoTension.volume = 0f; sonidoTension.Play(); }

        // Guardar el volumen de la música para alejarla (y devolverlo si el efecto se pausa)
        if (musicaQueSeAleja != null)
        {
            volumenesMusica = new float[musicaQueSeAleja.Length];
            for (int i = 0; i < musicaQueSeAleja.Length; i++)
                volumenesMusica[i] = musicaQueSeAleja[i] != null ? musicaQueSeAleja[i].volume : 0f;
        }
    }

    /// <summary>Lo llama el Act2ManagerReestructura al encontrar los zapatos: se apaga para el resto de la noche.</summary>
    public void DesactivarEfecto()
    {
        apagadoParaSiempre = true;
        if (efectoActivo) Pausar();

        foreach (Collider col in GetComponents<Collider>())
            col.enabled = false;
    }

    void Pausar()
    {
        efectoActivo = false;

        if (ambiencePasillo != null) ambiencePasillo.Stop();
        if (sonidoTension != null) sonidoTension.Stop();

        if (musicaQueSeAleja != null && volumenesMusica != null)
            for (int i = 0; i < musicaQueSeAleja.Length && i < volumenesMusica.Length; i++)
                if (musicaQueSeAleja[i] != null) musicaQueSeAleja[i].volume = volumenesMusica[i];

        restaurando = cinemachineCam != null;   // LateUpdate vuelve el FOV a la normalidad
    }

    // ─────────────────────────────────────────────────────────
    //  Efecto (en LateUpdate: así ningún otro script que toque el FOV en Update lo pisa)
    // ─────────────────────────────────────────────────────────
    void LateUpdate()
    {
        if (efectoActivo) ActualizarEfecto();
        else if (restaurando) RestaurarFOV();
    }

    void ActualizarEfecto()
    {
        tiempoActivo += Time.deltaTime;

        // FOV del dolly zoom: tan(fov/2) * distancia = constante → la puerta del fondo no cambia de tamaño
        float fovDolly;
        if (hayFondo && jugador != null && distanciaInicial > 0f)
        {
            float d = Mathf.Max(0.3f, DistanciaAlFondo());
            float tanInicial = Mathf.Tan(fovNormal * 0.5f * Mathf.Deg2Rad);
            fovDolly = 2f * Mathf.Atan(tanInicial * distanciaInicial / d) * Mathf.Rad2Deg;
            fovDolly = Mathf.Clamp(fovDolly, fovNormal, fovMaximo);
        }
        else
        {
            fovDolly = Mathf.Lerp(fovNormal, fovMaximo, tiempoActivo / Mathf.Max(0.01f, duracionSinFondo));
        }
        progreso = Mathf.Max(progreso, Mathf.InverseLerp(fovNormal, fovMaximo, fovDolly));

        // "¿Siempre fue tan largo?"
        if (!dialogoMostrado && tiempoActivo >= segundosHastaDialogo && !string.IsNullOrEmpty(dialogoPasillo))
        {
            dialogoMostrado = true;
            Act2ManagerReestructura.Instance?.MostrarDialogo(dialogoPasillo);
        }

        // La música del bar se aleja
        if (musicaQueSeAleja != null && volumenesMusica != null)
            for (int i = 0; i < musicaQueSeAleja.Length && i < volumenesMusica.Length; i++)
                if (musicaQueSeAleja[i] != null)
                    musicaQueSeAleja[i].volume = Mathf.Lerp(volumenesMusica[i], volumenesMusica[i] * 0.05f, progreso);

        if (sonidoTension != null) sonidoTension.volume = progreso;

        // ¿Mira hacia atrás? Mientras mira hacia atrás el bar parece mucho más lejos.
        // Al volver a mirar adelante sigue el efecto normal del pasillo.
        float fovObjetivo = MiraHaciaAtras() ? fovMirandoAtras : fovDolly;

        fovActual = Mathf.Lerp(fovActual, fovObjetivo, 1f - Mathf.Exp(-suavizado * Time.deltaTime));
        AplicarFOV(fovActual);
    }

    void RestaurarFOV()
    {
        fovActual = Mathf.Lerp(fovActual, fovOriginal, 1f - Mathf.Exp(-4f * Time.deltaTime));
        if (Mathf.Abs(fovActual - fovOriginal) < 0.1f)
        {
            fovActual = fovOriginal;
            restaurando = false;
        }
        AplicarFOV(fovActual);
    }

    void AplicarFOV(float fov)
    {
        if (cinemachineCam == null) return;
        LensSettings lens = cinemachineCam.Lens;
        lens.FieldOfView = fov;
        cinemachineCam.Lens = lens;
    }

    bool MiraHaciaAtras()
    {
        if (camaraJugador == null) return false;
        Vector3 mirada = camaraJugador.forward;
        mirada.y = 0f;
        if (mirada.sqrMagnitude < 0.001f) return false;
        return Vector3.Dot(mirada.normalized, haciaDeposito) < -0.4f;
    }

    // ─────────────────────────────────────────────────────────
    //  Dirección del depósito y fondo del pasillo (automático)
    // ─────────────────────────────────────────────────────────
    void CalcularDireccion()
    {
        direccionCalculada = true;
        Vector3 posJugador = jugador != null ? jugador.position : transform.position - transform.forward;

        if (finalDelPasillo != null)
        {
            puntoFinal = finalDelPasillo.position;
            Vector3 d = puntoFinal - posJugador; d.y = 0f;
            haciaDeposito = d.sqrMagnitude > 0.01f ? d.normalized : Plano(transform.forward);
            hayFondo = true;
            return;
        }

        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
        {
            // Sin caja: hacia donde mira el jugador al entrar
            haciaDeposito = camaraJugador != null ? Plano(camaraJugador.forward) : Plano(transform.forward);
            hayFondo = false;
            return;
        }

        // El eje largo (horizontal) de la caja es el pasillo
        Vector3 ejeX = transform.TransformVector(new Vector3(box.size.x, 0f, 0f));
        Vector3 ejeZ = transform.TransformVector(new Vector3(0f, 0f, box.size.z));
        Vector3 eje = Plano(ejeX).sqrMagnitude >= Plano(ejeZ).sqrMagnitude ? ejeX : ejeZ;
        float largo = Plano(eje).magnitude;
        Vector3 dir = Plano(eje).normalized;
        Vector3 centro = transform.TransformPoint(box.center);

        // Apunta hacia el lado contrario al que entró el jugador
        Vector3 desdeCentro = posJugador - centro; desdeCentro.y = 0f;
        if (Vector3.Dot(desdeCentro, dir) > 0f) dir = -dir;

        haciaDeposito = dir;
        puntoFinal = centro + dir * (largo * 0.5f);
        hayFondo = largo > 1f;
    }

    float DistanciaAlFondo()
    {
        Vector3 d = puntoFinal - jugador.position;
        d.y = 0f;
        return Mathf.Max(0f, Vector3.Dot(d, haciaDeposito));
    }

    static Vector3 Plano(Vector3 v) { v.y = 0f; return v.sqrMagnitude > 0.0001f ? v : Vector3.forward; }

    void OnDrawGizmosSelected()
    {
        if (!mostrarAyudas) return;
        Gizmos.color = Color.cyan;
        if (Application.isPlaying && direccionCalculada)
        {
            Gizmos.DrawRay(transform.position, haciaDeposito * 3f);
            if (hayFondo) Gizmos.DrawWireSphere(puntoFinal, 0.4f);
        }
        else if (finalDelPasillo != null)
        {
            Gizmos.DrawLine(transform.position, finalDelPasillo.position);
            Gizmos.DrawWireSphere(finalDelPasillo.position, 0.4f);
        }
    }
}
