using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
/// <summary>
/// Orquestador maestro del Acto 2 - La Fisura de la Realidad.
/// Maneja la máquina de estados que controla cada fase del acto.
/// Colocar en un GameObject vacío llamado "Act2Manager" en la escena Act_2.
///
/// TIP RÁPIDO: Hacé clic derecho sobre este componente en el Inspector
/// y elegí "Auto-buscar referencias" para conectar todo automáticamente.
/// Solo asigná a mano lo que no encuentre.
/// </summary>
public class Act2ManagerDemo : MonoBehaviour
{
    public static Act2ManagerDemo Instance { get; private set; }
    public enum Act2State
    {
        Inicio,     // Parpadeos + cansancio del barman
        Servicio,   // Clientes corruptos entran al bar
        Pasillo,    // Efecto de corredor infinito hacia la cocina
        Sotano,     // Puerta del sótano: golpes y nota
        Bano,       // Baño: buscar la llave
        Psicosis,   // Jumpscare del baño + evento de Pilar
        Cierre      // La llave se rompe, el bar vuelve a la normalidad
    }
    [Header("Estado Actual")]
    public Act2State estadoActual = Act2State.Inicio;
    [Header("UI y Diálogos")]
    public TextMeshProUGUI textoSubtitulos;
    public CanvasGroup canvasGroupDialogo; // Asegúrate de que el fondo tenga este componente
    public float velocidadEscritura = 0.04f;
    public float velocidadFade = 3f; // Nueva variable para controlar la suavidad
    [Header("Sistema de Iluminación")]
    public GameObject lucesNormales;    // Luces normales del bar (amarillas)
    public GameObject lucesServicio;    // Luces de servicio (violetas)
    public GameObject lucesPsicosis;    // Luces de psicosis (rojas/verdes distorsionadas)
    [Header("Efectos Visuales")]
    public EffectoParpadeo efectoParpadeo;
    public ParpadeoBarCambio parpadeoBarCambio;
    public EfectoPsicosis efectoPsicosis;
    public CameraShake sacudidaCamara;
    [Header("Clientes Corruptos")]
    public GameObject grupoClientesCorruptos;
    [Header("Servicio de cerveza opcional")]
    public ItemSO itemCerveza;
    public ItemSO itemVasoVacio;
    public GameObject vasoServicio;
    public ServicioCervezaVisual servicioCervezaVisual;
    [HideInInspector] public bool vasoEnCanilla = false;
    private bool sirviendoCerveza = false;
    [Header("Secuencia del Pasillo")]
    public PasilloEfecto pasilloEfecto;
    [Header("Sótano")]
    public PuertaSotanoAct2 puertaSotanoL;
    public PuertaSotanoAct2 puertaSotanoR;
    public NotaPuerta notaPuerta;
    public AudioSource sonidoGolpesSotano;
    [Header("Baño")]
    public LlaveInteractuable llaveObjeto;
    public VigenteMirror vigilanteMirror;
    [HideInInspector] public bool llaveTenida = false;
    [Header("Psicosis - referencias antiguas (solo para desactivar)")]
    [Tooltip("Sistema antiguo: se desactiva al empezar la escena, ya no inicia combate.")]
    public SombrasCombate sombrasCombate;
    [Tooltip("Figura anterior: se oculta, Pilar ahora usa EventoPilarPasillo.")]
    public FiguraNino figuraNino;
    [Tooltip("Opcional: grupo que contiene SOLAMENTE a los enemigos anteriores. Se apagará entero.")]
    public GameObject grupoEnemigosAnteriores;
    public Collider triggerDesaparicion;
    public Collider triggerAparicionJumpscare;
    private int sombrasDerrotadas = 0;
    public int totalSombras = 3;
    private bool parpadeandoLuces = false;
    private bool parpadeandoLuces2 = false;
    private Coroutine corrutinaActiva;
    [Header("Cierre del Acto")]
    public CanvasGroup fadeCanvasGroup;
    public AudioSource sonidoLlaveCrack;
    public AudioSource sonidoEstatica;
    [Tooltip("Trigger cerca de la puerta del sótano — se habilita al recoger la llave y dispara la secuencia de cierre")]
    public Collider triggerCierreSotano;
    [Header("Audio Ambiental")]
    public AudioSource ambientBar;
    public AudioSource musicBar;
    public AudioSource audioBasement;
    [SerializeField] private AudioSource musicaSuspenso;
    [HideInInspector] public AudioSource musicaCombate; // Referencia antigua: nunca se reproduce.
    public AudioSource sonidoVozPilar;

    [Header("Nuevas bases del combate")]
    [Tooltip("Empieza EXACTAMENTE cuando el evento de Naniela informa que terminó.")]
    public AudioSource baseEnemy1;
    [Tooltip("Empieza cuando Pilar llega al último punto de su recorrido.")]
    public AudioSource baseEnemy2;
    [Range(0f, 1f)] public float volumenBaseEnemy1 = 0.8f;
    [Range(0f, 1f)] public float volumenBaseEnemy2 = 0.8f;
    [Min(0f)] public float duracionTransicionBases = 0.5f;

    private bool faseCombateIniciada;
    private bool segundaBaseIniciada;
    private Coroutine transicionBasesActiva;
    [Header("Objetivos")]
    public TextMeshProUGUI textoObjetivo;

    [Header("Pilar - aparición en el pasillo")]
    public EventoPilarPasillo eventoPilarPasillo;

    [Header("Nuevo enemigo - Noche 2")]
    [Tooltip("Enemigo nuevo con NavMeshAgent. Se muestra en Idle al terminar Naniela y persigue solo al cruzar su trigger.")]
    public EnemigoNoche2AI enemigoNoche2;

    [Header("DEBUG - Empezar después del jumpscare del baño")]
    [Tooltip("Al pulsar Play salta el inicio, baño y jumpscare, y arranca el combate.")]
    public bool debugIniciarPostJumpscare = false;
    [Tooltip("Colocar a Lucas justo al salir del baño, antes de cruzar el trigger de Pilar.")]
    public Transform puntoDebugPostJumpscare;
    [Tooltip("Arrastra aca el GameObject principal de Lucas, que tiene PlayerController y CharacterController. Si no lo asignas, se buscara automaticamente.")]
    public PlayerController jugadorDebug;
    void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }
    Instance = this;
    if (Object.FindAnyObjectByType<Act1Manager>() != null)
        Debug.LogError("[Act2Manager] ¡ACT1MANAGER DETECTADO EN LA ESCENA! Eliminalo del hierarchy.");
}
    void Start()
    {
        // Si el texto de subtítulos no está asignado, lo crea automáticamente
        if (textoSubtitulos == null) textoSubtitulos = CrearTextoEmergencia();
        if (textoSubtitulos != null) textoSubtitulos.text = "";

        // Anular los enemigos viejos tanto en una partida normal como en el DEBUG.
        DesactivarEnemigosAnteriores();
        DetenerBasesCombate();
        faseCombateIniciada = false;
        segundaBaseIniciada = false;

        // Mantener a Pilar oculta hasta que termine Naniela y se cruce el trigger.
        if (eventoPilarPasillo != null) eventoPilarPasillo.PrepararEvento();
        if (enemigoNoche2 != null) enemigoNoche2.PrepararAntesDelBano();

        if (debugIniciarPostJumpscare)
        {
            IniciarDebugPostJumpscare();
            return; // Evita fade inicial, clientes y SecuenciaInicio.
        }

        estadoActual = Act2State.Inicio;
        if (fadeCanvasGroup != null)
        {
            // Forzamos que el objeto esté activo y sea negro al 100%
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
            Debug.Log("Iniciando fundido de entrada...");
            StartCoroutine(FundirDesdeNegro());
        }
        CambiarIluminacion("Normal");
        if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(false);
        if (llaveObjeto != null)            llaveObjeto.gameObject.SetActive(false);
        // FiguraNino antigua: no se usa nunca más (se apagó arriba).
        if (notaPuerta != null)             notaPuerta.gameObject.SetActive(false);
        StartCoroutine(SecuenciaInicio());
        ActualizarObjetivo("Preparate para el servicio...");
    }
    public void IniciarSecuenciaClienteCorrupto(ClienteCorrupto cliente)
{
    if (cliente == null) return;
    // 1. Activar el efecto de shake en el componente de UI si existe
    TMP_TextWobble wobble = textoSubtitulos.GetComponent<TMP_TextWobble>();
    if (wobble != null)
    {
        wobble.enabled = true;
        wobble.applyToAllText = true; // Activa el temblor en todo el diálogo del cliente
    }
    // 2. Reproducir voz y mostrar diálogo
    cliente.ReproducirVozDistorsionada();
    MostrarDialogo(cliente.nombreCliente + ": " + cliente.dialogoPedido);
    // 3. Girar la cabeza
    Transform jugador = GameObject.FindWithTag("Player")?.transform;
    if (jugador != null)
    {
        StartCoroutine(cliente.GirarHaciaJugador(jugador, 1.0f));
    }
}
    public void ActualizarObjetivo(string nuevoObjetivo)
    {
        if (textoObjetivo != null)
        {
            textoObjetivo.text = "- " + nuevoObjetivo;
            // Opcional: Podés disparar una pequeña animación de escala o color 
            // para que el jugador note que el objetivo cambió.
        }
    }
        // Crea un TMP de emergencia en pantalla si no hay uno asignado
        TextMeshProUGUI CrearTextoEmergencia()
    {
        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[Act2Manager] No hay Canvas en la escena. El texto de diálogo no se mostrará.");
            return null;
        }
        var go = new GameObject("TextoSubtitulos_Auto");
        go.transform.SetParent(canvas.transform, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.05f);
        rect.anchorMax = new Vector2(0.9f, 0.2f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        Debug.Log("[Act2Manager] TextoSubtitulos creado automáticamente. Asignalo en el Inspector para evitar esto.");
        return tmp;
    }
    // =========================================================
    // ESTADO 1: INICIO — Parpadeos y cansancio del barman
    // =========================================================
    IEnumerator FundirDesdeNegro()
    {
        // Aseguramos que empiece totalmente negro
        fadeCanvasGroup.alpha = 1f;
        yield return new WaitForSeconds(0.5f); // Un breve momento de suspenso en negro
        float duracionFade = 2.0f; // Qué tan lento querés que aclare
        float tiempo = 0;
        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            // Va de 1 (negro) a 0 (transparente)
            fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, tiempo / duracionFade);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        // Desactivamos el Raycast para que no bloquee el click del mouse al jugar
        fadeCanvasGroup.blocksRaycasts = false;
    }
    IEnumerator SecuenciaInicio()
    {
        yield return new WaitForSeconds(2f);
        MostrarDialogo("Lucas: No pude dormir nada ayer... estoy que me desmayo...");
        yield return new WaitForSeconds(3f);
        ActualizarObjetivo("Prepárate para el servicio...");
        Paranoia(10f);
        yield return new WaitForSeconds(4f);
        // Serie de parpadeos que van cambiando el bar sutilmente
        if (parpadeoBarCambio != null)
        {
            parpadeoBarCambio.EjecutarParpadeoConCambio(0);
            yield return new WaitForSeconds(3f);
            parpadeoBarCambio.EjecutarParpadeoConCambio(1);
            yield return new WaitForSeconds(3f);
            parpadeoBarCambio.EjecutarParpadeoConCambio(2);
        }
        yield return new WaitForSeconds(2f);
        Paranoia(10f);
        yield return new WaitForSeconds(3f);
        estadoActual = Act2State.Servicio;
        StartCoroutine(SecuenciaServicio());
    }
    // =========================================================
    // ESTADO 2: SERVICIO — Clientes corruptos
    // =========================================================
    IEnumerator SecuenciaServicio()
    {
        if (efectoParpadeo != null) efectoParpadeo.IniciarParpadeo();
        yield return new WaitForSeconds(1f);
        CambiarIluminacion("Servicio");
        if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(true);
        // Si había alguna música del bar sonando antes, la podés detener
        if (musicBar != null && musicBar.isPlaying) musicBar.Stop();
        // --- INICIO CON FADE IN ---
        // Sube el volumen de 0 a 1.0f en 2.5 segundos (podés ajustar el tiempo)
        if (musicaSuspenso != null)
        {
            StartCoroutine(FadeInAudio(musicaSuspenso, 3, 0.3f));
        }
        else if (ambientBar != null)
        {
            StartCoroutine(FadeInAudio(ambientBar, 3f, 0.3f));
        }
        yield return new WaitForSeconds(3f);
        ActualizarObjetivo("Atiende a los Clientes.");
        Paranoia(15f);
    }
    // Llamado por ClienteCorrupto al completar la interacción de ir a cocina
    public void IrACocina()
    {
        estadoActual = Act2State.Pasillo;
        ActualizarObjetivo("Busca la bebida especial en la cocina.");
    }
    // =========================================================
    // ESTADO 3: PASILLO — Zapatos del niño en la cocina
    // =========================================================
    public void ZapatosEncontrados()
    {
        if (estadoActual != Act2State.Pasillo) return;
        // Clientes desaparecen cuando el jugador ya está en la cocina (fuera de su vista)
        if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(false);
        // Desactivar el efecto del pasillo y bloquear el trigger para siempre
        if (pasilloEfecto != null) pasilloEfecto.DesactivarEfecto();
        // Cortar ambiente del bar
        if (ambientBar != null) ambientBar.Stop();
        if (musicBar != null)   musicBar.Stop();
        // Luces de servicio parpadean cada 3 segundos
        parpadeandoLuces = true;
        StartCoroutine(ParpadeoLucesServicio());
        Paranoia(30f);
        MostrarDialogo("Lucas: No deberían estar acá.... ¿Qué hacen estos zapatos en la cocina?");
        if (sacudidaCamara != null)
            StartCoroutine(sacudidaCamara.Shake(0.5f, 5f));
        StartCoroutine(SecuenciaDespuesZapatos());
    }
    IEnumerator ParpadeoLucesServicio()
    {
        bool encendidas = true;
        while (parpadeandoLuces)
        {
            encendidas = !encendidas;
            if (lucesServicio != null) lucesServicio.SetActive(encendidas);
            yield return new WaitForSeconds(3f);
        }
    }
    IEnumerator SecuenciaDespuesZapatos()
{
    yield return new WaitForSeconds(3f);
    estadoActual = Act2State.Sotano;
    // Activar golpes rítmicos en la puerta
    if (sonidoGolpesSotano != null) sonidoGolpesSotano.Play();
    if (puertaSotanoL != null)       puertaSotanoL.ActivarGolpes();
    if (notaPuerta != null)         notaPuerta.gameObject.SetActive(true);
    // --- VOZ DE PILAR TRAS LA PUERTA ---
    yield return new WaitForSeconds(0.6f); // Breve pausa tras el primer golpe
    if (sonidoVozPilar != null)
    {
        sonidoVozPilar.Play();
    }
    yield return new WaitForSeconds(3f);
    ActualizarObjetivo("Investiga los ruidos del sotano");
}
    // =========================================================
    // ESTADO 4: SÓTANO — La nota da la pista del baño
    // =========================================================
    public void NotaLeida()
{
    if (sonidoGolpesSotano != null && sonidoGolpesSotano.isPlaying) sonidoGolpesSotano.Stop();
    if (sonidoVozPilar != null && sonidoVozPilar.isPlaying)         sonidoVozPilar.Stop();
    MostrarDialogo("Lucas: ¿La llave? Creo que se me cayó mientras limpiaba el baño.");
    estadoActual = Act2State.Bano;
    ActualizarObjetivo("Ir a buscar la llave del sotano al baño");
    if (llaveObjeto != null) llaveObjeto.gameObject.SetActive(true);
}
    // =========================================================
    // ESTADO 5: BAÑO — La llave y El Vigilante en el espejo
    // =========================================================
    public void LlaveRecogida()
    {
        llaveTenida = true; // Garantiza que el cierre del sótano funcione tras recogerla.
        parpadeandoLuces = false;
        if (triggerCierreSotano == null)
            Debug.LogWarning("[Act2Manager] triggerCierreSotano no asignado — el trigger funciona igual, pero asignalo para tener referencia en el Inspector.");
        // Inmediatamente se activan las luces de psicosis
        CambiarIluminacion("Psicosis");
        StartCoroutine(SecuenciaPsicosis());
        parpadeandoLuces2 = true;
        StartCoroutine(ParpadeoLucesPsicosis());
    }
    IEnumerator ParpadeoLucesPsicosis()
    {
        bool encendidas = true;
        while (parpadeandoLuces2)
        {
            encendidas = !encendidas;
            if (lucesPsicosis != null) lucesPsicosis.SetActive(encendidas);
            yield return new WaitForSeconds(1f);
        }
    }
    // =========================================================
    // ESTADO 6: PSICOSIS - Naniela, Pilar y las nuevas bases
    // =========================================================
    public IEnumerator SecuenciaPsicosis()
    {
        yield return new WaitForSeconds(1.5f);
        if (estadoActual == Act2State.Cierre) yield break;

        estadoActual = Act2State.Psicosis;
        CambiarIluminacion("Psicosis");
        if (efectoPsicosis != null) efectoPsicosis.ActivarPsicosis();
        if (vigilanteMirror != null) vigilanteMirror.gameObject.SetActive(true);
        Paranoia(40f);

        // Aquí continúa el evento de Naniela del baño.
        // NO iniciar enemigos ni música en base a un tiempo estimado.
        // Cuando realmente termine el jumpscare, su script debe llamar
        // Act2ManagerDemo.Instance.NotificarFinJumpscareBano().
    }

    /// <summary>
    /// LLAMAR DESDE EL SCRIPT DEL JUMPSCARE DE NANIELA, AL TERMINAR SU EVENTO.
    /// También es el inicio de BaseEnemy1 en el DEBUG post-jumpscare.
    /// </summary>
    public void NotificarFinJumpscareBano()
    {
        if (estadoActual == Act2State.Cierre || faseCombateIniciada) return;
        if (estadoActual != Act2State.Psicosis && estadoActual != Act2State.Bano)
        {
            Debug.LogWarning("[Act2Manager] Fin de Naniela recibido fuera de la fase de baño. No se inició la música.");
            return;
        }

        estadoActual = Act2State.Psicosis;
        IniciarFaseCombate();
    }

    private void IniciarFaseCombate()
    {
        if (faseCombateIniciada) return;
        faseCombateIniciada = true;
        segundaBaseIniciada = false;
        estadoActual = Act2State.Psicosis;

        DetenerMusicaSuspenso();
        if (musicaCombate != null) musicaCombate.Stop();
        DesactivarEnemigosAnteriores();
        ActualizarObjetivo("SOBREVIVE Y LLEGA AL SOTANO");

        // Base 1: suena al terminar Naniela, SIN invocar SombrasCombate.
        CambiarBaseCombate(baseEnemy1, null, volumenBaseEnemy1);

        // Pilar aparece ÚNICAMENTE cuando Lucas cruza el trigger del pasillo.
        if (eventoPilarPasillo != null)
            eventoPilarPasillo.HabilitarEvento();
        else
            Debug.LogWarning("[Act2Manager] No está asignado EventoPilarPasillo.");

        // El nuevo enemigo queda VISIBLE en Idle, pero todavia NO persigue.
        if (enemigoNoche2 != null)
            enemigoNoche2.AparecerEnIdle();
        else
            Debug.LogWarning("[Act2Manager] Falta asignar Enemigo Noche 2.");

        Debug.Log("[Act2Manager] Terminó Naniela: BaseEnemy1, Pilar habilitada y nuevo enemigo en Idle.");
    }

    /// <summary>
    /// EventoPilarPasillo lo llama cuando Pilar alcanza su ÚLTIMO waypoint.
    /// </summary>
    public void NotificarFinRecorridoPilar()
    {
        if (!faseCombateIniciada || segundaBaseIniciada || estadoActual != Act2State.Psicosis)
            return;

        segundaBaseIniciada = true;
        CambiarBaseCombate(baseEnemy2, baseEnemy1, volumenBaseEnemy2);
        Debug.Log("[Act2Manager] Pilar terminó de correr: cambio a BaseEnemy2.");
    }

    private void CambiarBaseCombate(AudioSource nuevaBase, AudioSource baseAnterior, float volumen)
    {
        if (transicionBasesActiva != null)
        {
            StopCoroutine(transicionBasesActiva);
            transicionBasesActiva = null;
        }

        if (nuevaBase == null)
        {
            Debug.LogWarning("[Act2Manager] Falta asignar una de las bases BaseEnemy1 / BaseEnemy2.");
            if (baseAnterior != null) baseAnterior.Stop();
            return;
        }
        if (!nuevaBase.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[Act2Manager] El AudioSource de la nueva base está en un GameObject inactivo.");
            if (baseAnterior != null) baseAnterior.Stop();
            return;
        }

        if (baseAnterior == nuevaBase) baseAnterior = null;
        transicionBasesActiva = StartCoroutine(TransicionarBases(nuevaBase, baseAnterior, volumen));
    }

    private IEnumerator TransicionarBases(AudioSource nuevaBase, AudioSource baseAnterior, float volumen)
    {
        // Se conserva el volumen actual si una transición anterior aún no había terminado.
        float volumenAnterior = baseAnterior != null && baseAnterior.isPlaying
            ? baseAnterior.volume : 0f;
        nuevaBase.loop = true;
        nuevaBase.volume = 0f;
        nuevaBase.Play();

        float tiempo = 0f;
        if (duracionTransicionBases > 0f)
        {
            while (tiempo < duracionTransicionBases)
            {
                tiempo += Time.deltaTime;
                float t = Mathf.Clamp01(tiempo / duracionTransicionBases);
                nuevaBase.volume = Mathf.Lerp(0f, volumen, t);
                if (baseAnterior != null && baseAnterior.isPlaying)
                    baseAnterior.volume = Mathf.Lerp(volumenAnterior, 0f, t);
                yield return null;
            }
        }

        nuevaBase.volume = volumen;
        if (baseAnterior != null) baseAnterior.Stop();
        transicionBasesActiva = null;
    }

    private void DetenerBasesCombate()
    {
        if (transicionBasesActiva != null)
        {
            StopCoroutine(transicionBasesActiva);
            transicionBasesActiva = null;
        }
        if (baseEnemy1 != null) baseEnemy1.Stop();
        if (baseEnemy2 != null) baseEnemy2.Stop();
        if (musicaCombate != null) musicaCombate.Stop();
    }

    private void DesactivarEnemigosAnteriores()
    {
        // Mantenemos las referencias antiguas únicamente para apagarlas.
        if (sombrasCombate != null)
        {
            sombrasCombate.DesactivarTodo();
            sombrasCombate.enabled = false;
        }
        if (figuraNino != null) figuraNino.gameObject.SetActive(false);
        if (grupoEnemigosAnteriores != null) grupoEnemigosAnteriores.SetActive(false);
    }

   [ContextMenu("▶ TEST: POST JUMPSCARE BAÑO (combate + Pilar)")]
   private void IniciarDebugPostJumpscare()
   {
       if (!Application.isPlaying)
       {
           Debug.LogWarning("[Act2Manager] El debug se ejecuta durante Play, no en modo Edicion.");
           return;
       }

       // Prepara el estado posterior al susto sin iniciar la secuencia de la noche.
       StopAllCoroutines();
       estadoActual = Act2State.Psicosis;
       llaveTenida = true;
       sombrasDerrotadas = 0;
       faseCombateIniciada = false;
       segundaBaseIniciada = false;
       parpadeandoLuces = false;
       parpadeandoLuces2 = false;

       if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(false);
       if (notaPuerta != null) notaPuerta.gameObject.SetActive(false);
       if (llaveObjeto != null) llaveObjeto.gameObject.SetActive(false);
       if (figuraNino != null) figuraNino.gameObject.SetActive(false);
       if (vigilanteMirror != null) vigilanteMirror.gameObject.SetActive(false);
       if (pasilloEfecto != null) pasilloEfecto.DesactivarEfecto();

       if (ambientBar != null) ambientBar.Stop();
       if (musicBar != null) musicBar.Stop();
       if (audioBasement != null) audioBasement.Stop();
       if (musicaSuspenso != null) musicaSuspenso.Stop();
       DetenerBasesCombate();
       DesactivarEnemigosAnteriores();

       // NUEVO: prepara los muebles, cuadros y manchas del estado final
       // de la noche sin reproducir la secuencia de parpadeos ni su música.
       if (parpadeoBarCambio != null)
           parpadeoBarCambio.AplicarTodosLosCambiosSinParpadeo();
       else
           Debug.LogWarning("[Act2Manager] DEBUG: falta asignar ParpadeoBarCambio; el bar no se desordenará.");

       // Si un cambio del escenario activó accidentalmente un enemigo antiguo,
       // garantizamos que siga apagado durante esta nueva fase.
       DesactivarEnemigosAnteriores();

       if (sonidoGolpesSotano != null) sonidoGolpesSotano.Stop();
       if (sonidoVozPilar != null) sonidoVozPilar.Stop();

       if (fadeCanvasGroup != null)
       {
           fadeCanvasGroup.gameObject.SetActive(true);
           fadeCanvasGroup.alpha = 0f;
           fadeCanvasGroup.blocksRaycasts = false;
           fadeCanvasGroup.interactable = false;
       }
       if (textoSubtitulos != null) textoSubtitulos.text = "";
       if (canvasGroupDialogo != null)
       {
           canvasGroupDialogo.alpha = 0f;
           canvasGroupDialogo.blocksRaycasts = false;
       }

       if (eventoPilarPasillo != null) eventoPilarPasillo.PrepararEvento();

       CambiarIluminacion("Psicosis");
       if (efectoPsicosis != null) efectoPsicosis.ActivarPsicosis();
       Paranoia(40f);
       ActualizarObjetivo("SOBREVIVE Y LLEGA AL SOTANO");

       // Se hace DESPUES del Start de los otros componentes para evitar
       // que una inicializacion posterior vuelva a mover a Lucas.
       StartCoroutine(TeletransportarJugadorDebugYEmpezarCombate());

       parpadeandoLuces2 = true;
       StartCoroutine(ParpadeoLucesPsicosis());
       Debug.Log("[Act2Manager] DEBUG post-jumpscare activado. Preparando teleport de Lucas...");
   }

   private IEnumerator TeletransportarJugadorDebugYEmpezarCombate()
   {
       // Dejar que PlayerController y otros Start de la escena se ejecuten.
       yield return null;

       PlayerController jugador = jugadorDebug != null
           ? jugadorDebug
           : FindFirstObjectByType<PlayerController>();

       if (jugador == null)
       {
           Debug.LogError("[Act2Manager] DEBUG: no encontre PlayerController. Asigna Jugador Debug con el objeto principal Lucas.");
       }
       else if (puntoDebugPostJumpscare == null)
       {
           Debug.LogError("[Act2Manager] DEBUG: falta Punto Debug Post Jumpscare en el Inspector.");
       }
       else
       {
           // El CharacterController debe estar desactivado al modificar el Transform.
           // Referenciar PlayerController evita mover por error una camara hija
           // que tambien tenga el tag Player.
           bool estabaBloqueado = jugador.controlesBloqueados;
           jugador.controlesBloqueados = true;
           CharacterController cc = jugador.GetComponent<CharacterController>();
           bool ccEstabaActivo = cc != null && cc.enabled;
           if (ccEstabaActivo) cc.enabled = false;

           Vector3 destino = puntoDebugPostJumpscare.position;
           Quaternion rotacionDestino = puntoDebugPostJumpscare.rotation;
           jugador.transform.SetPositionAndRotation(destino, rotacionDestino);

           if (ccEstabaActivo) cc.enabled = true;
           Physics.SyncTransforms();
           Debug.Log($"[Act2Manager] TELEPORT Lucas a {destino} | posicion actual: {jugador.transform.position}");

           // Comprobamos despues de un frame si algun otro sistema lo mueve.
           yield return null;
           float distancia = Vector3.Distance(jugador.transform.position, destino);
           if (distancia > 0.75f)
           {
               Debug.LogWarning($"[Act2Manager] Lucas cambio de lugar tras el teleport ({distancia:F2} m). Revisar scripts adicionales de spawn, guardado o respawn.");
           }
           jugador.controlesBloqueados = estabaBloqueado;
       }

       // DEBUG: simulamos que el jumpscare de Naniela ya terminó.
       NotificarFinJumpscareBano();
   }

    public void SombraDerrotada()
    {
        sombrasDerrotadas++;
    }
    public bool TieneLlave() => llaveTenida;
    // =========================================================
    // ESTADO 7: CIERRE — La llave se rompe, el bar se normaliza
    // =========================================================
    public void UsarLlave()
    {
        if (!llaveTenida) return;
        StartCoroutine(SecuenciaCierre());
    }
    public IEnumerator SecuenciaCierre()
    {
        estadoActual = Act2State.Cierre;
        // Sonido metálico de la llave rompiéndose
        if (sonidoLlaveCrack != null) sonidoLlaveCrack.Play();
        if (sacudidaCamara != null)
            StartCoroutine(sacudidaCamara.Shake(0.5f, 10f));
        parpadeandoLuces2 = false;
        llaveTenida = false;
        Paranoia(50f);
        yield return new WaitForSeconds(3f);
        // Claridad repentina — todo para
        if (sonidoEstatica != null) sonidoEstatica.Stop();
        if (ambientBar != null)     ambientBar.Stop();
        if (musicBar != null)       musicBar.Stop();
        if (audioBasement != null) audioBasement.Stop();
        DetenerBasesCombate();
        if (eventoPilarPasillo != null) eventoPilarPasillo.DetenerLlanto3D();
        if (enemigoNoche2 != null) enemigoNoche2.DesactivarEnemigo();
        DesactivarEnemigosAnteriores();
        if (efectoPsicosis != null) efectoPsicosis.DesactivarPsicosis();
        CambiarIluminacion("Normal");
        yield return new WaitForSeconds(3f);
        ActualizarObjetivo("???");
        yield return new WaitForSeconds(2f);
        // Recuperación de paranoia
        Paranoia(-50f);
        MostrarDialogo("Lucas: ¿Mi imaginación de nuevo?... No. La llave estaba acá.");
        yield return new WaitForSeconds(4f);
        // La puerta del sótano se abre sola
        if (puertaSotanoL != null)
            puertaSotanoL.AbrirSola();
        else
            Debug.LogError("[Act2Manager] puertaSotano no asignado — la puerta no se abrirá. Asignalo en el Inspector.");
        if (puertaSotanoR != null)
            puertaSotanoR.AbrirSola();
        else
            Debug.LogError("[Act2Manager] puertaSotano no asignado — la puerta no se abrirá. Asignalo en el Inspector.");
        yield return new WaitForSeconds(2f);
        MostrarDialogo("Lucas: No. Esta noche no.");
        yield return new WaitForSeconds(1f);
        StartCoroutine(FundirANegro());
    }
    IEnumerator FundirANegro()
    {
        float duracionFade = 2.5f;
        float tiempoFade = 0;
        while (tiempoFade < duracionFade)
        {
            tiempoFade += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, tiempoFade / duracionFade);
            yield return null;
        }
        Debug.Log("Acto 2 finalizado.");
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("Night_3 Scene");
    }
    // =========================================================
// SERVICIO DE CERVEZA OPCIONAL
// =========================================================
public void ColocarVasoEnCanilla()
{
    // Solo funciona durante Servicio
    if (estadoActual != Act2State.Servicio)
    {
        MostrarDialogo("Lucas: Ahora no es momento de preparar bebidas.");
        return;
    }
    if (ControladorMano3D.Instance == null)
        return;
    // Tiene que llevar específicamente el vaso vacío
    if (ControladorMano3D.Instance.ObtenerItemActual() != itemVasoVacio)
    {
        MostrarDialogo("Lucas: Necesito un vaso primero.");
        return;
    }
    // Sacamos el vaso vacío de la mano
    ControladorMano3D.Instance.VaciarMano();
    // Preparamos el líquido vacío antes de mostrar el vaso
    if (servicioCervezaVisual != null)
    {
        servicioCervezaVisual.PrepararVasoVacio();
    }
    // Aparece el vaso debajo de la canilla
    if (vasoServicio != null)
    {
        vasoServicio.SetActive(true);
    }
    vasoEnCanilla = true;
    // Empieza a servir automáticamente
    ServirCerveza();
}
public void ServirCerveza()
{
    if (!vasoEnCanilla)
        return;
    if (sirviendoCerveza)
        return;
    if (servicioCervezaVisual == null)
    {
        Debug.LogError(
            "[Act2Manager] Falta asignar ServicioCervezaVisual."
        );
        return;
    }
    sirviendoCerveza = true;
    servicioCervezaVisual.Servir(() =>
    {
        sirviendoCerveza = false;
        vasoEnCanilla = false;
        // Sacamos el vaso visual de debajo de la canilla
        if (vasoServicio != null)
        {
            vasoServicio.SetActive(false);
        }
        // Aparece la cerveza llena en la mano
        if (ControladorMano3D.Instance != null)
        {
            ControladorMano3D.Instance.EquiparItem(itemCerveza);
        }
        Debug.Log("[Act2Manager] Cerveza servida correctamente.");
    });
}
    // =========================================================
    // UTILIDADES
    // =========================================================
 public void MostrarDialogo(string mensaje)
{
    if (textoSubtitulos != null) // Quitamos la condición obligatoria de canvasGroupDialogo
    {
        if (corrutinaActiva != null)
        {
            StopCoroutine(corrutinaActiva);
        }
        // Si canvasGroupDialogo no está asignado, mostramos el texto directo
        if (canvasGroupDialogo != null)
        {
            corrutinaActiva = StartCoroutine(SecuenciaDialogo(mensaje));
        }
        else
        {
            textoSubtitulos.text = mensaje;
        }
    }
    else
    {
        Debug.LogError("[Act2Manager] ¡textoSubtitulos NO está asignado en el Inspector!");
    }
}
    IEnumerator SecuenciaDialogo(string frase)
    {
        // 1. Limpieza total antes de empezar la nueva frase
        textoSubtitulos.text = "";
        // Forzamos el fade in (si ya estaba visible, no pasa nada)
        while (canvasGroupDialogo.alpha < 1)
        {
            canvasGroupDialogo.alpha += Time.deltaTime * velocidadFade;
            yield return null;
        }
        // 2. Efecto Typewriter
        foreach (char letra in frase.ToCharArray())
        {
            textoSubtitulos.text += letra;
            yield return new WaitForSeconds(velocidadEscritura);
        }
        // 3. Tiempo de lectura
        yield return new WaitForSeconds(3f);
        // 4. Fade Out
        while (canvasGroupDialogo.alpha > 0)
        {
            canvasGroupDialogo.alpha -= Time.deltaTime * (velocidadFade / 2);
            yield return null;
        }
        // Importante: decimos que ya terminó para limpiar la referencia
        corrutinaActiva = null;
    }
    void LimpiarTexto() => textoSubtitulos.text = "";
    // Wrapper seguro para paranoia (evita NullRef si ParanoiaSystem no está en la escena)
    void Paranoia(float valor)
    {
        if (ParanoiaSystem.Instance != null)
            ParanoiaSystem.Instance.AddParanoia(valor);
        else
            Debug.LogWarning($"[Act2Manager] ParanoiaSystem no encontrado. Valor ignorado: {valor}");
    }
    public void CambiarIluminacion(string estado)
    {
        if (lucesNormales  != null) lucesNormales.SetActive(false);
        if (lucesServicio  != null) lucesServicio.SetActive(false);
        if (lucesPsicosis  != null) lucesPsicosis.SetActive(false);
        switch (estado)
        {
            case "Normal":   if (lucesNormales  != null) lucesNormales.SetActive(true);  break;
            case "Servicio": if (lucesServicio  != null) lucesServicio.SetActive(true);  break;
            case "Psicosis": if (lucesPsicosis  != null) lucesPsicosis.SetActive(true);  break;
            default: Debug.LogWarning("Estado de luz desconocido: " + estado); break;
        }
    }
    public void DetenerMusicaSuspenso()
{
    if (musicaSuspenso != null && musicaSuspenso.isPlaying)
    {
        musicaSuspenso.Stop();
    }
}
// Compatibilidad con otros scripts que todavía llamen este método antiguo.
// Ya NO reproduce musicaCombate: se usa BaseEnemy1.
public void IniciarMusicaCombate()
{
    NotificarFinJumpscareBano();
}

[ContextMenu("▶ TEST: Simular fin de jumpscare Naniela (BaseEnemy1)")]
private void TestFinNaniela()
{
    if (Application.isPlaying) NotificarFinJumpscareBano();
}
    // =========================================================
    // DEBUG — Clic derecho en el Inspector para saltar estados o verificar la escena
    // =========================================================
    [ContextMenu("⚠ Verificar escena (buscar nulos)")]
    void VerificarEscena()
    {
        bool ok = true;
        void Chk(object campo, string nombre)
        {
            if (campo == null) { Debug.LogError($"[Act2Manager] FALTA: {nombre}"); ok = false; }
        }
        Chk(textoSubtitulos,       "textoSubtitulos (TextMeshProUGUI)");
        Chk(fadeCanvasGroup,       "fadeCanvasGroup (panel negro de fade)");
        Chk(lucesNormales,         "lucesNormales");
        Chk(lucesServicio,         "lucesServicio");
        Chk(lucesPsicosis,         "lucesPsicosis");
        Chk(efectoParpadeo,        "efectoParpadeo");
        Chk(parpadeoBarCambio,     "parpadeoBarCambio");
        Chk(efectoPsicosis,        "efectoPsicosis");
        Chk(sacudidaCamara,        "sacudidaCamara");
        Chk(grupoClientesCorruptos,"grupoClientesCorruptos");
        Chk(pasilloEfecto,         "pasilloEfecto");
        Chk(puertaSotanoL,          "puertaSotano");
        Chk(notaPuerta,            "notaPuerta");
        Chk(llaveObjeto,           "llaveObjeto");
        Chk(vigilanteMirror,       "vigilanteMirror");
        Chk(baseEnemy1,            "baseEnemy1 (nueva música tras Naniela)");
        Chk(baseEnemy2,            "baseEnemy2 (nueva música tras Pilar)");
        Chk(eventoPilarPasillo,      "eventoPilarPasillo");
        if (ok) Debug.Log("[Act2Manager] ✓ Todos los campos están asignados.");
        if (Object.FindAnyObjectByType<Act1Manager>() != null)
            Debug.LogError("[Act2Manager] ACT1MANAGER está en la escena — eliminalo.");
        if (ParanoiaSystem.Instance == null)
            Debug.LogError("[Act2Manager] ParanoiaSystem no encontrado en la escena.");
    }
    [ContextMenu("▶ TEST: Saltar a SERVICIO (clientes)")]
    void TestSaltarServicio()
    {
        StopAllCoroutines();
        estadoActual = Act2State.Servicio;
        StartCoroutine(SecuenciaServicio());
        Debug.Log("[Act2Manager] Saltando a SERVICIO.");
    }
    [ContextMenu("▶ TEST: Saltar a PASILLO (corredor)")]
    void TestSaltarPasillo()
    {
        StopAllCoroutines();
        IrACocina();
        Debug.Log("[Act2Manager] Saltando a PASILLO.");
    }
    [ContextMenu("▶ TEST: Saltar a SÓTANO (golpes + nota)")]
    void TestSaltarSotano()
    {
        StopAllCoroutines();
        estadoActual = Act2State.Pasillo;
        ZapatosEncontrados();
        Debug.Log("[Act2Manager] Saltando a SÓTANO.");
    }
    [ContextMenu("▶ TEST: Saltar a BAÑO (llave)")]
    void TestSaltarBano()
    {
        StopAllCoroutines();
        NotaLeida();
        Debug.Log("[Act2Manager] Saltando a BAÑO.");
    }
    [ContextMenu("▶ TEST: Saltar a PSICOSIS (esperar fin Naniela)")]
    void TestSaltarPsicosis()
    {
        StopAllCoroutines();
        llaveTenida = true;
        StartCoroutine(SecuenciaPsicosis());
        Debug.Log("[Act2Manager] Saltando a PSICOSIS.");
    }
    [ContextMenu("▶ TEST: Saltar a CIERRE (final del acto)")]
    void TestSaltarCierre()
    {
        StopAllCoroutines();
        llaveTenida = true;
        UsarLlave();
        Debug.Log("[Act2Manager] Saltando a CIERRE.");
    }
    [ContextMenu("▶ TEST: Subir paranoia +30")]
    void TestSubirParanoia() => Paranoia(30f);
    [ContextMenu("▶ TEST: Resetear paranoia a 0")]
    void TestResetParanoia() => Paranoia(-100f);
    // =========================================================
    // AUTO-SETUP — Clic derecho en el Inspector → "Auto-buscar referencias"
    // Busca y conecta todos los campos vacíos automáticamente.
    // Solo asigná a mano lo que quede en rojo después de ejecutarlo.
    // =========================================================
    [ContextMenu("Auto-buscar referencias")]
    void AutoBuscarReferencias()
    {
        int encontrados = 0;
        // --- UI ---
        if (textoSubtitulos == null)
        {
            var candidato = GameObject.Find("TextoSubtitulos") ?? GameObject.Find("Subtitulos") ?? GameObject.Find("DialogoText");
            if (candidato != null) { textoSubtitulos = candidato.GetComponent<TextMeshProUGUI>(); encontrados++; }
        }
        if (fadeCanvasGroup == null)
        {
            var candidato = GameObject.Find("FadePanel") ?? GameObject.Find("PanelNegro") ?? GameObject.Find("FadeCanvas");
            if (candidato != null) { fadeCanvasGroup = candidato.GetComponent<CanvasGroup>(); encontrados++; }
        }
        // --- Efectos visuales ---
        if (efectoParpadeo == null)
        {
            efectoParpadeo = FindAnyObjectByType<EffectoParpadeo>();
            if (efectoParpadeo != null) encontrados++;
        }
        if (parpadeoBarCambio == null)
        {
            parpadeoBarCambio = FindAnyObjectByType<ParpadeoBarCambio>();
            if (parpadeoBarCambio != null) encontrados++;
        }
        if (efectoPsicosis == null)
        {
            efectoPsicosis = FindAnyObjectByType<EfectoPsicosis>();
            if (efectoPsicosis != null) encontrados++;
        }
        if (sacudidaCamara == null)
        {
            sacudidaCamara = FindAnyObjectByType<CameraShake>();
            if (sacudidaCamara != null) encontrados++;
        }
        // --- Luces (busca por nombre exacto) ---
        if (lucesNormales == null)
        {
            lucesNormales = GameObject.Find("LucesNormales") ?? GameObject.Find("Luces Normales") ?? GameObject.Find("--Lights--");
            if (lucesNormales != null) encontrados++;
        }
        if (lucesServicio == null)
        {
            lucesServicio = GameObject.Find("LucesServicio") ?? GameObject.Find("Luces Servicio");
            if (lucesServicio != null) encontrados++;
        }
        if (lucesPsicosis == null)
        {
            lucesPsicosis = GameObject.Find("LucesPsicosis") ?? GameObject.Find("Luces Psicosis") ?? GameObject.Find("LucesCombate");
            if (lucesPsicosis != null) encontrados++;
        }
        // --- Clientes ---
        if (grupoClientesCorruptos == null)
        {
            grupoClientesCorruptos = GameObject.Find("--ClientGroup--") ?? GameObject.Find("ClientesCorruptos") ?? GameObject.Find("GrupoClientes");
            if (grupoClientesCorruptos != null) encontrados++;
        }
        // --- Pasillo ---
        if (pasilloEfecto == null)
        {
            pasilloEfecto = FindAnyObjectByType<PasilloEfecto>();
            if (pasilloEfecto != null) encontrados++;
        }
        // --- Sótano ---
        if (puertaSotanoL == null)
        {
            puertaSotanoL = FindAnyObjectByType<PuertaSotanoAct2>();
            if (puertaSotanoL    != null) encontrados++;
        }
        if (notaPuerta == null)
        {
            notaPuerta = FindAnyObjectByType<NotaPuerta>();
            if (notaPuerta != null) encontrados++;
        }
        // --- Baño ---
        if (llaveObjeto == null)
        {
            llaveObjeto = FindAnyObjectByType<LlaveInteractuable>();
            if (llaveObjeto != null) encontrados++;
        }
        if (vigilanteMirror == null)
        {
            vigilanteMirror = FindAnyObjectByType<VigenteMirror>();
            if (vigilanteMirror != null) encontrados++;
        }
        // --- Nuevos eventos y audio ---
        if (eventoPilarPasillo == null)
        {
            eventoPilarPasillo = FindAnyObjectByType<EventoPilarPasillo>();
            if (eventoPilarPasillo != null) encontrados++;
        }
        // Las referencias de SombrasCombate y FiguraNino son antiguas.
        // NO se autoasignan ni se vuelven a activar.
        // --- Audio ---
        BuscarAudioSources();
        Debug.Log($"[Act2Manager] Auto-setup: {encontrados} referencias conectadas. " +
                  "Revisá en rojo las que quedaron vacías y asignalas a mano.");
        // Marca el objeto como modificado para que Unity guarde los cambios
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    void BuscarAudioSources()
    {
        // Busca AudioSources en el hijo "--AudioSource--" o similar
        GameObject audioParent = GameObject.Find("--AudioSource--") ?? GameObject.Find("AudioSources") ?? GameObject.Find("Sounds");
        if (audioParent == null) return;
        AudioSource[] fuentes = audioParent.GetComponentsInChildren<AudioSource>(true);
        foreach (AudioSource fuente in fuentes)
        {
            string nombre = fuente.gameObject.name.ToLower();
            if (ambientBar      == null && nombre.Contains("ambient"))  { ambientBar      = fuente; }
            if (musicBar        == null && nombre.Contains("music"))     { musicBar        = fuente; }
            if (sonidoLlaveCrack == null && (nombre.Contains("crack") || nombre.Contains("llave"))) { sonidoLlaveCrack = fuente; }
            if (sonidoEstatica  == null && nombre.Contains("static"))   { sonidoEstatica  = fuente; }
            if (sonidoGolpesSotano == null && (nombre.Contains("golpe") || nombre.Contains("knock"))) { sonidoGolpesSotano = fuente; }
        }
    }
    /// <summary>
    /// Inicia la reproducción del AudioSource y sube su volumen suavemente desde 0 hasta el volumen objetivo.
    /// </summary>
    IEnumerator FadeInAudio(AudioSource audio, float duracionFade, float volumenObjetivo = 1f)
    {
        if (audio == null) yield break;
        audio.volume = 0f;
        audio.loop = true;
        audio.Play();
        float tiempo = 0f;
        while (tiempo < duracionFade)
        {
            tiempo += Time.deltaTime;
            audio.volume = Mathf.Lerp(0f, volumenObjetivo, tiempo / duracionFade);
            yield return null;
        }
        audio.volume = volumenObjetivo;
    }
}
