using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// =====================================================================================
//  ACT2MANAGER REESTRUCTURA — NOCHE 2 según el guion "EL ÚLTIMO TURNO - NOCHE 2.3"
//
//  Es una copia INDEPENDIENTE del Act2Manager: la demo sigue usando Act2Manager / Act2ManagerDemo
//  sin ningún cambio. En la escena reestructurada tiene que haber SOLO este manager.
//
//  Flujo:
//   0. INTRO        → (cámara exterior opcional) → diálogos → celular → "- NOCHE 2 - 19:46 hs."
//   1. TAREAS       → luz que parpadea → barrer (hoja que cae, mancha) → barra/exhibidor (susurro)
//                     → baños (cubículo fuera de servicio, sombra en el baño de mujeres)
//      PARPADEO N°1 → trigger en el pasillito de los baños: bar destruido 1 segundo → servicio
//   2. SERVICIO     → clientes normales de a uno → cliente corrupto (glitch)
//   3. PASILLO      → pasillo eterno → zapatos de Pilar → luces tensas → golpes → cámara a la puerta
//   4. SÓTANO       → los golpes se apagan al acercarse → nota "La llave está FUERA DE SERVICIO"
//   5. BAÑO         → bar destruido + luces rojas → el cubículo cede → llave → portazos
//   6. VIGILANTE    → 2 apariciones → combate con sombras → Pilar → sin pilas → correr
//   7. CIERRE       → la puerta se abre, todo vuelve a la normalidad → Lucas se frena → negro → risa
//
//  TIPS: clic derecho sobre el componente →
//   "Auto-buscar referencias"  conecta solo casi todos los campos.
//   "⚠ Verificar escena"        lista en la Consola lo que falta o lo que sobra.
//   "▶ Saltar a ..."            (en Play) salta a cualquier fase para probarla.
// =====================================================================================
public class Act2ManagerReestructura : MonoBehaviour
{
    public static Act2ManagerReestructura Instance { get; private set; }

    public enum Act2State
    {
        Inicio,     // Intro
        Tareas,     // Barrer, barra/exhibidor, baños
        Servicio,   // Clientes normales y el corrupto
        Pasillo,    // Corredor infinito hacia el depósito
        Sotano,     // Golpes y nota en la puerta del sótano
        Bano,       // Regreso al baño: buscar la llave
        Vigilante,  // Apariciones del Vigilante
        Psicosis,   // Combate de sombras + Pilar
        Cierre      // La puerta se abre, todo vuelve a la normalidad
    }

    // ─────────────────────────────────────────────────────────────
    //  CAMPOS (Inspector)
    // ─────────────────────────────────────────────────────────────
    [Header("Estado actual (solo para mirar)")]
    public Act2State estadoActual = Act2State.Inicio;

    [Header("TESTEO")]
    [Tooltip("Desde qué fase arranca la noche al darle Play. Dejar en 'Normal' para el juego final.")]
    public FaseDebugReestructura iniciarEnFase = FaseDebugReestructura.Normal;

    [Header("UI y diálogos")]
    [Tooltip("TextMeshPro - Text (UI) donde se escriben los diálogos.")]
    public TextMeshProUGUI textoSubtitulos;
    [Tooltip("CanvasGroup del cuadro de diálogo (hace el fundido). Si queda vacío, el texto aparece y se borra sin fundido.")]
    public CanvasGroup canvasGroupDialogo;
    public float velocidadEscritura = 0.04f;
    public float velocidadFade = 3f;
    [Tooltip("Segundos extra que queda cada diálogo antes de pasar al siguiente en las secuencias.")]
    public float tiempoLecturaDialogo = 1.8f;
    [Tooltip("TextMeshPro - Text (UI) del objetivo actual.")]
    public TextMeshProUGUI textoObjetivo;
    [Tooltip("Panel negro a pantalla completa con CanvasGroup (fundidos a negro).")]
    public CanvasGroup fadeCanvasGroup;

    [Header("Iluminación (GameObjects padre con las luces)")]
    public GameObject lucesNormales;
    public GameObject lucesServicio;
    public GameObject lucesPsicosis;
    [Tooltip("OPCIONAL. Luces 'tensas y oscuras' después de los zapatos. Vacío = luces de servicio parpadeando.")]
    public GameObject lucesTension;

    [Header("Efectos")]
    [Tooltip("El EffectoParpadeo de la escena (se usa su pantalla negra y su sonido de chispazo).")]
    public EffectoParpadeo efectoParpadeo;
    public EfectoPsicosisReestructura efectoPsicosis;
    public CameraShake sacudidaCamara;
    [Tooltip("OPCIONAL. Imagen negra a pantalla completa para los flashazos. Vacío = usa la del EffectoParpadeo.")]
    public GameObject pantallaNegraFlash;

    [Header("Estados del bar")]
    [Tooltip("OPCIONAL. Grupo con los objetos del bar ORDENADO.")]
    public GameObject barOrdenado;
    [Tooltip("Grupo con el bar DESORDENADO / destruido (sillas tiradas, vidrios...). Empieza desactivado.")]
    public GameObject barDesordenado;
    [Tooltip("Cuánto se ve el bar destruido en el Parpadeo N°1 (segundos).")]
    public float duracionFlashDesordenado = 1f;

    [Header("Audio ambiental")]
    public AudioSource ambientBar;
    public AudioSource musicBar;
    public AudioSource musicaSuspenso;
    public AudioSource audioBasement;
    [Tooltip("Estática general (se calla en el cierre).")]
    public AudioSource sonidoEstatica;

    [Header("0. INTRO")]
    [Tooltip("OPCIONAL. Cámara (CinemachineCamera de prioridad alta) frente al bar, DESACTIVADA. Se acerca a la puerta y hace fundido.")]
    public GameObject camaraIntroExterior;
    [Tooltip("Punto al que se acerca la cámara de la intro (la puerta del bar).")]
    public Transform puntoPuertaIntro;
    public float duracionAcercamientoIntro = 4f;
    public AudioSource sonidoPuertaAbriendo;
    [Tooltip("Si está marcado, la intro pasa con el bar a OSCURAS (el fundido de entrada no va a mostrar nada) y las luces " +
             "se prenden recién en las tareas. Desmarcado (recomendado): se ve el bar desde el fundido y en las tareas las luces parpadean al 'encenderse'.")]
    public bool introConLucesApagadas = false;
    [TextArea] public string dialogoIntro1 = "Lucas: No pude dormir nada ayer... Estoy que me desmayo...";
    [TextArea] public string dialogoIntro2 = "Lucas: Tengo que aguantar un poco…";
    [Tooltip("OPCIONAL del guion: notificación de Mariela + 'Después.'")]
    public bool usarEventoCelular = true;
    [Tooltip("UI con el texto 'Mariela — 1 mensaje nuevo'. Empieza DESACTIVADA.")]
    public GameObject notificacionCelularUI;
    public AudioSource sonidoVibracionCelular;
    [TextArea] public string dialogoCelular = "Lucas: Después.";
    [Tooltip("CanvasGroup con un TextMeshPro hijo para el texto en pantalla de la noche.")]
    public CanvasGroup tituloNoche;
    [TextArea] public string textoTituloNoche = "- NOCHE 2 -\n19:46 hs.";

    [Header("1. TAREAS")]
    public AudioSource sonidoInterruptorLuz;
    [TextArea] public string dialogoTareas = "Lucas: Bueno… Una noche más";
    [Tooltip("Si es true, las tareas se hacen en orden: barrer → barra → baños.")]
    public bool tareasEnOrden = true;

    [Header("2. SERVICIO")]
    [Tooltip("GameObject padre con TODOS los clientes (normales y corrupto). Empieza apagado y aparece con el Parpadeo N°1.")]
    public GameObject grupoClientesCorruptos;
    [Tooltip("Clientes normales EN ORDEN. Vacío = los busca solos (ordenados por nombre).")]
    public ClienteNormalReestructura[] clientesNormalesEnOrden;
    [Tooltip("El cliente corrupto (el último). Vacío = lo busca solo.")]
    public ClienteCorruptoReestructura clienteCorrupto;
    [TextArea] public string dialogoLucasPedido = "Lucas: ¿Qué le sirvo?";
    [TextArea] public string dialogoClienteCorrupto = "Cliente_1: Traeme lo más fuerte que tengas.";
    [Tooltip("Duración del glitch (el guion dice 2/3 segundos).")]
    public float duracionGlitch = 2.5f;
    [Tooltip("OPCIONAL. Panel de UI (distorsión / estática) con CanvasGroup que parpadea durante el glitch.")]
    public CanvasGroup overlayGlitch;
    public AudioSource sonidoEstaticaGlitch;
    public AudioSource sonidoRespiracionGlitch;
    public float intensidadMaxShakeGlitch = 3f;

    [Header("Servicio de cerveza (vaso + canilla)")]
    public ItemSO itemCerveza;
    public ItemSO itemVasoVacio;
    [Tooltip("Vaso que aparece debajo de la canilla mientras se sirve. Empieza desactivado.")]
    public GameObject vasoServicio;
    public ServicioCervezaVisual servicioCervezaVisual;

    [Header("3. PASILLO + ZAPATOS")]
    public PasilloEfectoReestructura pasilloEfecto;
    [TextArea] public string dialogoZapatos = "Lucas: No... No deberían estar acá...";
    [Tooltip("Hacia dónde gira la cámara cuando empiezan los golpes (la puerta del sótano).")]
    public Transform puntoPuertaSotano;
    public float duracionGiroHaciaPuerta = 1.5f;

    [Header("4. SÓTANO")]
    public PuertaSotanoAct2Reestructura puertaSotanoL;
    [Tooltip("OPCIONAL. Segunda hoja de la puerta.")]
    public PuertaSotanoAct2Reestructura puertaSotanoR;
    public NotaPuertaReestructura notaPuerta;
    [Tooltip("Loop de golpes en la puerta del sótano.")]
    public AudioSource sonidoGolpesSotano;

    [Header("5. BAÑO — puertas con física (no se modifican; ver ControlPuertaFisicaReestructura)")]
    public LlaveInteractuableReestructura llaveObjeto;
    [Tooltip("ControlPuertaFisicaReestructura con rol FueraDeServicio que apunta a la puerta del tercer cubículo.")]
    public ControlPuertaFisicaReestructura controlFueraDeServicio;
    [Tooltip("ControlPuertaFisicaReestructura con rol SeCierraDeGolpe de los cubículos de al lado.")]
    public ControlPuertaFisicaReestructura[] controlesQueSeCierranDeGolpe;
    [Tooltip("OPCIONAL. Sonido extra al cerrarse de golpe (Door.cs ya hace sonar las bisagras).")]
    public AudioSource sonidoPortazosCubiculos;
    [Tooltip("Si está marcado, las luces rojas parpadean desde el regreso al baño hasta el cierre.")]
    public bool parpadearLucesRojas = false;

    [Header("6. VIGILANTE + COMBATE")]
    [Tooltip("OPCIONAL. Ruidos de Sombras en el salón que se acercan (loop). Sube de volumen después de la llave.")]
    public AudioSource ruidoSombrasLejos;
    public SombrasCombateReestructura sombrasCombate;
    public FiguraNinoReestructura figuraNino;
    [Tooltip("Vacío = lo busca solo.")]
    public LinternaSinPilasReestructura linternaSinPilas;
    [Tooltip("Segundos desde que Pilar desaparece hasta que la linterna se queda sin pilas.")]
    public float segundosHastaSinPilas = 5f;
    [Tooltip("Por si el jugador nunca se acerca a Pilar: segundos de combate hasta quedarse sin pilas (0 = nunca).")]
    public float segundosMaxCombateSinPilas = 45f;
    [Tooltip("Seguro: si después de una aparición del Vigilante la siguiente no pasa en estos segundos (por ejemplo, el jugador salió corriendo antes), el combate empieza igual. 0 = sin seguro.")]
    public float segundosMaxEsperaVigilante = 20f;

    [Header("7. CIERRE")]
    public AudioSource sonidoAbrirCandado;
    [Tooltip("Cualquier otro audio que tenga que callarse ('Silencio absoluto').")]
    public AudioSource[] audiosASilenciar;
    [TextArea] public string dialogoFinal = "Lucas: No… todavía no puedo… Después.";
    public float distanciaRetroceso = 1.5f;
    public float duracionRetroceso = 1.2f;
    [Tooltip("Seguro: si el jugador no pisa el trigger 'FrenoEscaleraSotano' en estos segundos después de abrir la puerta, Lucas se frena igual. 0 = sin seguro.")]
    public float segundosMaxEsperaEscalera = 15f;
    [Tooltip("Sonido de una niña jugando / pequeña risa, en el negro final.")]
    public AudioSource risaNina;
    public float segundosEnNegroAntesDeCambiar = 5f;
    [Tooltip("Nombre EXACTO de la escena siguiente (tiene que estar en File > Build Profiles > Scene List).")]
    public string escenaSiguiente = "Night_3 Scene";

    [HideInInspector] public bool llaveTenida = false;
    [HideInInspector] public bool vasoEnCanilla = false;

    // ─────────────────────────────────────────────────────────────
    //  ESTADO INTERNO
    // ─────────────────────────────────────────────────────────────
    private bool tieneEscoba, tieneTrapo, tieneCepillo;
    private readonly List<ZonaLimpiezaReestructura> zonasLimpieza = new List<ZonaLimpiezaReestructura>();
    private TareaReestructura tareaActual = TareaReestructura.Barrer;
    private bool tareasTerminadas, parpadeo1Hecho, golpesSilenciados, puertaSotanoAbierta, lucasSeFreno;
    private int indiceClienteNormal;
    private int aparicionesVigilanteHechas;
    private bool combateFinalIniciado, sinPilasProgramado;
    private bool parpadeandoLucesServicio, parpadeandoLucesRojas;
    private bool sirviendoCerveza;
    private PlayerController jugadorCtrl;
    private Coroutine rutinaDialogo;
    private Coroutine rutinaSinPilasPorTiempo;

    // Scripts de la DEMO que no tienen que estar en la escena reestructurada (se buscan por nombre
    // para no depender de ellos).
    static readonly string[] ScriptsDeLaDemo =
    {
        "Act2Manager", "Act2ManagerDemo", "Act2DebugHelper", "ClienteCorrupto", "PasilloEfecto", "EfectoPsicosis",
        "ZapatosNino", "ZapatosNinoDemo", "NotaPuerta", "LlaveInteractuable", "PuertaSotanoAct2", "TriggerCierreSotano",
        "SombrasCombate", "FiguraNino", "TriggerDesaparicion", "TriggerAparicionJumpscare", "VigenteMirror",
        "ParpadeoBarCambio", "PuntoVasosAct2", "PuntoSuministroAct2"
    };

    // ═════════════════════════════════════════════════════════════
    //  ARRANQUE
    // ═════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[Act2ManagerReestructura] Hay DOS Act2ManagerReestructura en la escena. Se destruye el segundo.");
            Destroy(this);
            return;
        }
        Instance = this;

        // Conflictos graves: otro manager corriendo su propio flujo al mismo tiempo
        foreach (MonoBehaviour mb in FindObjectsByType<MonoBehaviour>())
        {
            string n = mb.GetType().Name;
            if (n == "Act1Manager" || n == "Act2Manager" || n == "Act2ManagerDemo")
                Debug.LogError($"[Act2ManagerReestructura] ¡Hay un {n} en '{mb.gameObject.name}'! Eliminá ese componente: " +
                               "corre su propio flujo y choca con la Noche 2 reestructurada.");
            else if (n == "EfectoPsicosis")
                Debug.LogError($"[Act2ManagerReestructura] '{mb.gameObject.name}' tiene EfectoPsicosis (el de la demo): pisa el FOV " +
                               "y ARRUINA el efecto del pasillo. Reemplazalo por EfectoPsicosisReestructura.");
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (Instance != this) return;   // era un duplicado

        if (textoSubtitulos == null) textoSubtitulos = CrearTextoEmergencia();
        if (textoSubtitulos != null) textoSubtitulos.text = "";
        if (canvasGroupDialogo != null) canvasGroupDialogo.alpha = 0f;

        estadoActual = Act2State.Inicio;

        // FADE IN FROM BLACK (como en la demo): arranca todo negro y aclara
        if (fadeCanvasGroup == null)
        {
            GameObject panel = Buscar("PanelNegro", "FadePanel", "FadeCanvas");
            if (panel != null) fadeCanvasGroup = panel.GetComponent<CanvasGroup>();
            if (fadeCanvasGroup != null) Debug.LogWarning($"[Act2ManagerReestructura] 'Fade Canvas Group' estaba vacío: se usa '{fadeCanvasGroup.name}'. Asignalo en el Inspector.");
            else Debug.LogError("[Act2ManagerReestructura] Falta 'Fade Canvas Group' (panel negro con CanvasGroup): no va a haber fundido de entrada.");
        }
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
            StartCoroutine(FundirDesdeNegro());
        }

        CambiarIluminacion("Normal");

        if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(false);
        if (llaveObjeto != null)            llaveObjeto.gameObject.SetActive(false);
        if (notaPuerta != null)             notaPuerta.gameObject.SetActive(false);
        if (figuraNino != null)             figuraNino.gameObject.SetActive(true);
        if (vasoServicio != null)           vasoServicio.SetActive(false);

        IniciarNoche2();
    }

    void IniciarNoche2()
    {
        jugadorCtrl = FindAnyObjectByType<PlayerController>();

        zonasLimpieza.Clear();
        zonasLimpieza.AddRange(FindObjectsByType<ZonaLimpiezaReestructura>(FindObjectsInactive.Include));

        if (linternaSinPilas == null) linternaSinPilas = FindAnyObjectByType<LinternaSinPilasReestructura>(FindObjectsInactive.Include);

        // Clientes
        if (clientesNormalesEnOrden == null || clientesNormalesEnOrden.Length == 0)
            clientesNormalesEnOrden = BuscarClientesOrdenados();
        foreach (ClienteNormalReestructura c in clientesNormalesEnOrden) if (c != null) c.esSuTurno = false;

        if (clienteCorrupto == null) clienteCorrupto = FindAnyObjectByType<ClienteCorruptoReestructura>(FindObjectsInactive.Include);
        if (clienteCorrupto != null) clienteCorrupto.esSuTurno = false;

        // Estado inicial del mundo
        if (barDesordenado != null) barDesordenado.SetActive(false);
        if (barOrdenado != null) barOrdenado.SetActive(true);
        if (notificacionCelularUI != null) notificacionCelularUI.SetActive(false);
        if (tituloNoche != null) tituloNoche.alpha = 0f;
        if (overlayGlitch != null) overlayGlitch.alpha = 0f;
        if (camaraIntroExterior != null) camaraIntroExterior.SetActive(false);
        if (lucesTension != null) lucesTension.SetActive(false);

        Debug.Log($"[Act2ManagerReestructura] Noche 2 reestructurada. Zonas de limpieza: barrer {Total(TareaReestructura.Barrer)}, " +
                  $"barra {Total(TareaReestructura.Barra)}, baños {Total(TareaReestructura.Banos)}. Clientes normales: {clientesNormalesEnOrden.Length}.");

        if (iniciarEnFase == FaseDebugReestructura.Normal)
            StartCoroutine(SecuenciaIntro());
        else
            DebugSaltarA(iniciarEnFase, true);
    }

    ClienteNormalReestructura[] BuscarClientesOrdenados()
    {
        ClienteNormalReestructura[] encontrados = FindObjectsByType<ClienteNormalReestructura>(FindObjectsInactive.Include);
        System.Array.Sort(encontrados, (a, b) => string.CompareOrdinal(a.name, b.name));
        return encontrados;
    }

    // ═════════════════════════════════════════════════════════════
    //  0. INTRO
    // ═════════════════════════════════════════════════════════════
    IEnumerator SecuenciaIntro()
    {
        estadoActual = Act2State.Inicio;
        BloquearJugador(true);

        if (introConLucesApagadas) ApagarTodasLasLuces();

        if (camaraIntroExterior != null)
        {
            // Frente del bar, Lucas llegando... la cámara se acerca a la puerta
            camaraIntroExterior.SetActive(true);
            yield return new WaitForSeconds(2.5f);   // fundido de entrada

            Transform cam = camaraIntroExterior.transform;
            if (puntoPuertaIntro != null)
            {
                Vector3 desde = cam.position;
                float t = 0f;
                while (t < duracionAcercamientoIntro)
                {
                    t += Time.deltaTime;
                    cam.position = Vector3.Lerp(desde, puntoPuertaIntro.position, Mathf.SmoothStep(0f, 1f, t / duracionAcercamientoIntro));
                    yield return null;
                }
            }

            // FUNDIDO A NEGRO, ruido de puerta abriéndose
            yield return Fundir(0f, 1f, 0.8f);
            if (sonidoPuertaAbriendo != null) sonidoPuertaAbriendo.Play();
            camaraIntroExterior.SetActive(false);
            yield return new WaitForSeconds(1.5f);
            // Vuelve la imagen con Lucas ya adentro del bar
            yield return Fundir(1f, 0f, 1.5f);
        }
        else
        {
            yield return new WaitForSeconds(2.5f);
        }

        yield return Decir(dialogoIntro1);
        yield return new WaitForSeconds(1f);          // "Se detiene un segundo."
        yield return Decir(dialogoIntro2);
        SumarParanoia(5f);

        if (usarEventoCelular)
        {
            if (sonidoVibracionCelular != null) sonidoVibracionCelular.Play();
            if (notificacionCelularUI != null) notificacionCelularUI.SetActive(true);
            yield return new WaitForSeconds(2.5f);   // Lucas mira la notificación... bloquea, guarda
            if (notificacionCelularUI != null) notificacionCelularUI.SetActive(false);
            yield return Decir(dialogoCelular);
        }

        // (TEXTO EN PANTALLA) - NOCHE 2 - 19:46 hs.
        if (tituloNoche != null)
        {
            TextMeshProUGUI tmp = tituloNoche.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null) tmp.text = textoTituloNoche;
            tituloNoche.gameObject.SetActive(true);
            yield return FadeCanvas(tituloNoche, 0f, 1f, 1f);
            yield return new WaitForSeconds(3f);
            yield return FadeCanvas(tituloNoche, 1f, 0f, 1f);
        }

        BloquearJugador(false);
        StartCoroutine(SecuenciaTareasInicio());
    }

    // ═════════════════════════════════════════════════════════════
    //  1. TAREAS
    // ═════════════════════════════════════════════════════════════
    IEnumerator SecuenciaTareasInicio()
    {
        estadoActual = Act2State.Tareas;

        // "Lucas enciende la luz (la iluminación tarda un segundo en estabilizarse). Parpadea un poco."
        if (sonidoInterruptorLuz != null) sonidoInterruptorLuz.Play();
        yield return ParpadeoEncendido(lucesNormales, 1.2f);
        CambiarIluminacion("Normal");

        yield return Decir(dialogoTareas);

        tareaActual = PrimeraTareaPendiente();
        ActualizarObjetivoTareas();
    }

    /// <summary>La llama HerramientaLimpiezaReestructura al recoger la escoba / trapo / cepillo.</summary>
    public void DarHerramienta(HerramientaReestructura h)
    {
        switch (h)
        {
            case HerramientaReestructura.Escoba:  tieneEscoba = true;  break;
            case HerramientaReestructura.Trapo:   tieneTrapo = true;   break;
            case HerramientaReestructura.Cepillo: tieneCepillo = true; break;
        }
        if (estadoActual == Act2State.Tareas) ActualizarObjetivoTareas();
    }

    public bool TieneHerramienta(HerramientaReestructura h)
    {
        switch (h)
        {
            case HerramientaReestructura.Escoba:  return tieneEscoba;
            case HerramientaReestructura.Trapo:   return tieneTrapo;
            case HerramientaReestructura.Cepillo: return tieneCepillo;
            default:                              return true;
        }
    }

    public bool PuedeHacerTarea(TareaReestructura t)
    {
        if (estadoActual != Act2State.Tareas) return false;
        return !tareasEnOrden || t == tareaActual;
    }

    /// <summary>La llama ZonaLimpiezaReestructura al terminar de limpiar.</summary>
    public void RegistrarZonaLimpia(ZonaLimpiezaReestructura zona)
    {
        if (!zonasLimpieza.Contains(zona)) zonasLimpieza.Add(zona);

        // Tarea 1: "cuando falte solo un montículo para barrer, se activa la mancha que tiene cerca"
        if (zona.tarea == TareaReestructura.Barrer && Total(TareaReestructura.Barrer) - Hechas(TareaReestructura.Barrer) == 1)
        {
            foreach (ZonaLimpiezaReestructura z in zonasLimpieza)
                if (z != null && z.tarea == TareaReestructura.Barrer && !z.EstaCompletada && z.manchaCercana != null)
                    z.manchaCercana.Activar();
        }

        if (tareasEnOrden && Hechas(tareaActual) >= Total(tareaActual))
            tareaActual = PrimeraTareaPendiente();

        bool todo = Hechas(TareaReestructura.Barrer) >= Total(TareaReestructura.Barrer) &&
                    Hechas(TareaReestructura.Barra)  >= Total(TareaReestructura.Barra) &&
                    Hechas(TareaReestructura.Banos)  >= Total(TareaReestructura.Banos);

        if (todo && !tareasTerminadas) TareasTerminadas(zona);
        else ActualizarObjetivoTareas();
    }

    void TareasTerminadas(ZonaLimpiezaReestructura ultimaZona)
    {
        tareasTerminadas = true;
        if (ControladorMano3D.Instance != null && ControladorMano3D.Instance.TieneManoOcupada())
            ControladorMano3D.Instance.VaciarMano();

        ActualizarObjetivo("Volvé al salón");

        // El parpadeo lo dispara el trigger del pasillito de los baños. Si no hay trigger, o si la última
        // tarea no fue en los baños (el jugador no va a pasar por ahí), pasa solo a los 3 segundos.
        if (!HayTrigger(TipoTriggerReestructura.ParpadeoSalidaBanos))
        {
            Debug.LogWarning("[Act2ManagerReestructura] No hay TriggerZonaReestructura 'ParpadeoSalidaBanos'. El Parpadeo N°1 se dispara solo en 3 s.");
            StartCoroutine(EsperarYParpadeo1());
        }
        else if (ultimaZona != null && ultimaZona.tarea != TareaReestructura.Banos)
        {
            StartCoroutine(EsperarYParpadeo1());
        }
    }

    IEnumerator EsperarYParpadeo1()
    {
        yield return new WaitForSeconds(3f);
        if (!parpadeo1Hecho) StartCoroutine(ParpadeoPrimeraMutacion());
    }

    TareaReestructura PrimeraTareaPendiente()
    {
        if (Hechas(TareaReestructura.Barrer) < Total(TareaReestructura.Barrer)) return TareaReestructura.Barrer;
        if (Hechas(TareaReestructura.Barra)  < Total(TareaReestructura.Barra))  return TareaReestructura.Barra;
        return TareaReestructura.Banos;
    }

    int Total(TareaReestructura t)
    {
        int n = 0;
        foreach (ZonaLimpiezaReestructura z in zonasLimpieza) if (z != null && z.tarea == t) n++;
        return n;
    }

    int Hechas(TareaReestructura t)
    {
        int n = 0;
        foreach (ZonaLimpiezaReestructura z in zonasLimpieza) if (z != null && z.tarea == t && z.EstaCompletada) n++;
        return n;
    }

    bool NecesitaHerramienta(TareaReestructura t, HerramientaReestructura h)
    {
        foreach (ZonaLimpiezaReestructura z in zonasLimpieza)
            if (z != null && z.tarea == t && !z.EstaCompletada && z.HerramientaNecesaria() == h) return true;
        return false;
    }

    void ActualizarObjetivoTareas()
    {
        if (estadoActual != Act2State.Tareas || tareasTerminadas) return;

        if (tareasEnOrden)
            ActualizarObjetivo(TextoTarea(tareaActual));
        else
            ActualizarObjetivo(TextoTarea(TareaReestructura.Barrer) + "\n- " + TextoTarea(TareaReestructura.Barra) + "\n- " + TextoTarea(TareaReestructura.Banos));
    }

    string TextoTarea(TareaReestructura t)
    {
        switch (t)
        {
            case TareaReestructura.Barrer:
                if (NecesitaHerramienta(t, HerramientaReestructura.Escoba) && !tieneEscoba) return "Buscá la escoba en el depósito";
                return $"Barré el bar ({Hechas(t)}/{Total(t)})";
            case TareaReestructura.Barra:
                if (NecesitaHerramienta(t, HerramientaReestructura.Trapo) && !tieneTrapo) return "Buscá el trapo";
                return $"Limpiá la barra y el exhibidor ({Hechas(t)}/{Total(t)})";
            default:
                if (NecesitaHerramienta(t, HerramientaReestructura.Cepillo) && !tieneCepillo) return "Buscá el cepillo para los baños";
                return $"Limpiá los baños ({Hechas(t)}/{Total(t)})";
        }
    }

    // ═════════════════════════════════════════════════════════════
    //  TRIGGERS GENÉRICOS (TriggerZonaReestructura)
    // ═════════════════════════════════════════════════════════════
    /// <summary>Devuelve true si el trigger se usó (y no tiene que volver a dispararse).</summary>
    public bool TriggerZona(TipoTriggerReestructura tipo)
    {
        switch (tipo)
        {
            case TipoTriggerReestructura.ParpadeoSalidaBanos:
                if (estadoActual == Act2State.Tareas && tareasTerminadas && !parpadeo1Hecho)
                {
                    StartCoroutine(ParpadeoPrimeraMutacion());
                    return true;
                }
                return false;

            case TipoTriggerReestructura.SilenciarGolpesSotano:
                if (estadoActual == Act2State.Sotano && !golpesSilenciados)
                {
                    SilenciarGolpes();
                    return true;
                }
                return false;

            case TipoTriggerReestructura.FrenoEscaleraSotano:
                if (estadoActual == Act2State.Cierre && puertaSotanoAbierta && !lucasSeFreno)
                {
                    StartCoroutine(LucasSeFrena());
                    return true;
                }
                return false;
        }
        return false;
    }

    bool HayTrigger(TipoTriggerReestructura tipo)
    {
        foreach (TriggerZonaReestructura t in FindObjectsByType<TriggerZonaReestructura>(FindObjectsInactive.Include))
            if (t.tipo == tipo) return true;
        return false;
    }

    // ═════════════════════════════════════════════════════════════
    //  PARPADEO N°1 — PRIMERA MUTACIÓN TEMPORAL
    // ═════════════════════════════════════════════════════════════
    IEnumerator ParpadeoPrimeraMutacion()
    {
        parpadeo1Hecho = true;

        // Flashazo negro rápido...
        PantallaNegra(true);
        if (efectoParpadeo != null && efectoParpadeo.audioLuces != null) efectoParpadeo.audioLuces.Play();
        yield return new WaitForSeconds(0.08f);

        // ...vemos por un segundo todo el bar absolutamente desordenado
        if (barOrdenado != null) barOrdenado.SetActive(false);
        if (barDesordenado != null) barDesordenado.SetActive(true);
        PantallaNegra(false);
        SumarParanoia(10f);
        yield return new WaitForSeconds(duracionFlashDesordenado);

        PantallaNegra(true);
        yield return new WaitForSeconds(0.12f);

        // ...para luego volver al bar en servicio, con clientes esperando sus pedidos
        if (barDesordenado != null) barDesordenado.SetActive(false);
        if (barOrdenado != null) barOrdenado.SetActive(true);
        CambiarIluminacion("Servicio");
        if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(true);
        if (ambientBar != null && !ambientBar.isPlaying) ambientBar.Play();
        if (musicBar != null && !musicBar.isPlaying) musicBar.Play();
        PantallaNegra(false);

        IniciarServicio();
    }

    // ═════════════════════════════════════════════════════════════
    //  2. SERVICIO
    // ═════════════════════════════════════════════════════════════
    void IniciarServicio()
    {
        estadoActual = Act2State.Servicio;
        indiceClienteNormal = 0;
        foreach (ClienteNormalReestructura c in clientesNormalesEnOrden) if (c != null) c.esSuTurno = false;
        if (clienteCorrupto != null) clienteCorrupto.esSuTurno = false;
        HabilitarSiguienteCliente();
    }

    void HabilitarSiguienteCliente()
    {
        // Saltear vacíos o ya atendidos
        while (indiceClienteNormal < clientesNormalesEnOrden.Length &&
               (clientesNormalesEnOrden[indiceClienteNormal] == null ||
                clientesNormalesEnOrden[indiceClienteNormal].estado == ClienteNormalReestructura.Estado.Atendido))
            indiceClienteNormal++;

        if (indiceClienteNormal < clientesNormalesEnOrden.Length)
        {
            clientesNormalesEnOrden[indiceClienteNormal].esSuTurno = true;
            ActualizarObjetivo($"Atendé a los clientes ({indiceClienteNormal}/{clientesNormalesEnOrden.Length + 1})");
            return;
        }

        // El último es el corrupto
        if (clienteCorrupto != null)
        {
            clienteCorrupto.esSuTurno = true;
            ActualizarObjetivo("Atendé al cliente que está sentado solo");
        }
        else
        {
            Debug.LogError("[Act2ManagerReestructura] No hay ClienteCorruptoReestructura en la escena: el servicio no puede terminar.");
        }
    }

    /// <summary>La llama ClienteNormalReestructura cuando se lo atendió.</summary>
    public void ClienteNormalAtendido(ClienteNormalReestructura cliente)
    {
        indiceClienteNormal++;
        HabilitarSiguienteCliente();
    }

    /// <summary>La llama ClienteCorruptoReestructura al interactuar.</summary>
    public void IniciarSecuenciaClienteCorrupto(ClienteCorruptoReestructura cliente)
    {
        StartCoroutine(SecuenciaClienteCorrupto(cliente));
    }

    IEnumerator SecuenciaClienteCorrupto(ClienteCorruptoReestructura cliente)
    {
        BloquearJugador(true);   // "se bloquea el movimiento de Lucas"
        Transform jugador = jugadorCtrl != null ? jugadorCtrl.transform : null;

        yield return Decir(dialogoLucasPedido);

        // GLITCH: el cliente gira la cabeza, todos los clientes se voltean, estática, respiración, sacudida
        SumarParanoia(15f);
        List<MirarJugadorReestructura> miradores = new List<MirarJugadorReestructura>();
        foreach (MirarJugadorReestructura m in FindObjectsByType<MirarJugadorReestructura>())
        {
            if (cliente != null && m.transform.IsChildOf(cliente.transform)) continue;
            miradores.Add(m);
            m.MirarAlJugador();
        }
        if (cliente != null) StartCoroutine(cliente.GirarHaciaJugador(jugador, duracionGlitch * 0.8f));

        if (sonidoEstaticaGlitch != null) { sonidoEstaticaGlitch.volume = 0f; sonidoEstaticaGlitch.Play(); }
        if (sonidoRespiracionGlitch != null) { sonidoRespiracionGlitch.volume = 0f; sonidoRespiracionGlitch.Play(); }

        float t = 0f, proximoShake = 0f;
        while (t < duracionGlitch)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duracionGlitch);
            if (sonidoEstaticaGlitch != null) sonidoEstaticaGlitch.volume = k;
            if (sonidoRespiracionGlitch != null) sonidoRespiracionGlitch.volume = k;
            if (overlayGlitch != null) overlayGlitch.alpha = Random.Range(0f, 0.6f) * (0.4f + k);
            if (sacudidaCamara != null && t >= proximoShake)
            {
                StartCoroutine(sacudidaCamara.Shake(0.3f, intensidadMaxShakeGlitch * k));
                proximoShake = t + 0.3f;
            }
            yield return null;
        }

        // "Vuelve todo a la normalidad, con el cliente mirándonos"
        if (sonidoEstaticaGlitch != null) sonidoEstaticaGlitch.Stop();
        if (sonidoRespiracionGlitch != null) sonidoRespiracionGlitch.Stop();
        if (overlayGlitch != null) overlayGlitch.alpha = 0f;
        foreach (MirarJugadorReestructura m in miradores) if (m != null) m.VolverAlInstante();

        yield return new WaitForSeconds(0.4f);
        if (cliente != null) cliente.ReproducirVozDistorsionada();
        yield return Decir(dialogoClienteCorrupto);

        BloquearJugador(false);
        if (cliente != null) cliente.esSuTurno = false;
        IrACocina();
    }

    // ═════════════════════════════════════════════════════════════
    //  SERVICIO DE CERVEZA (lo usan PuntoSuministroAct2Reestructura / PuntoVasosAct2Reestructura)
    // ═════════════════════════════════════════════════════════════
    public void ColocarVasoEnCanilla()
    {
        if (estadoActual != Act2State.Servicio) return;
        if (ControladorMano3D.Instance == null) return;
        if (ControladorMano3D.Instance.ObtenerItemActual() != itemVasoVacio) return;

        ControladorMano3D.Instance.VaciarMano();

        if (servicioCervezaVisual != null) servicioCervezaVisual.PrepararVasoVacio();
        if (vasoServicio != null) vasoServicio.SetActive(true);

        vasoEnCanilla = true;
        ServirCerveza();
    }

    public void ServirCerveza()
    {
        if (!vasoEnCanilla || sirviendoCerveza) return;

        if (servicioCervezaVisual == null)
        {
            // Sin animación: la cerveza pasa directo a la mano
            Debug.LogWarning("[Act2ManagerReestructura] Falta 'Servicio Cerveza Visual': la cerveza se sirve sin animación.");
            vasoEnCanilla = false;
            if (vasoServicio != null) vasoServicio.SetActive(false);
            if (ControladorMano3D.Instance != null) ControladorMano3D.Instance.EquiparItem(itemCerveza);
            return;
        }

        sirviendoCerveza = true;
        servicioCervezaVisual.Servir(() =>
        {
            sirviendoCerveza = false;
            vasoEnCanilla = false;
            if (vasoServicio != null) vasoServicio.SetActive(false);
            if (ControladorMano3D.Instance != null) ControladorMano3D.Instance.EquiparItem(itemCerveza);
        });
    }

    // ═════════════════════════════════════════════════════════════
    //  3. PASILLO → ZAPATOS
    // ═════════════════════════════════════════════════════════════
    /// <summary>Empieza la fase Pasillo (después del pedido del cliente corrupto).</summary>
    public void IrACocina()
    {
        estadoActual = Act2State.Pasillo;
        ActualizarObjetivo("Buscá lo que pidió en el depósito");
    }

    /// <summary>La llama ZapatosNinoReestructura.</summary>
    public void ZapatosEncontrados()
    {
        if (estadoActual != Act2State.Pasillo) return;
        StartCoroutine(SecuenciaZapatos());
    }

    IEnumerator SecuenciaZapatos()
    {
        estadoActual = Act2State.Sotano;

        if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(false);
        if (pasilloEfecto != null) pasilloEfecto.DesactivarEfecto();   // apagado para el resto de la noche

        BloquearJugador(true);
        yield return Decir(dialogoZapatos);

        // (Finaliza el diálogo). Cambia la iluminación, todo tenso y oscuro. Efecto general.
        if (ambientBar != null) ambientBar.Stop();
        if (musicBar != null) musicBar.Stop();
        if (musicaSuspenso != null) musicaSuspenso.Stop();
        CambiarIluminacion("Tension");
        if (sacudidaCamara != null) StartCoroutine(sacudidaCamara.Shake(0.6f, 2f));
        SumarParanoia(10f);

        yield return new WaitForSeconds(0.8f);

        // Golpes en la puerta del sótano
        if (sonidoGolpesSotano != null) { sonidoGolpesSotano.loop = true; sonidoGolpesSotano.Play(); }
        if (puertaSotanoL != null) puertaSotanoL.ActivarGolpes();

        // Movimiento de cámara indicando la dirección de la puerta
        yield return GirarJugadorHacia(puntoPuertaSotano != null ? puntoPuertaSotano : (puertaSotanoL != null ? puertaSotanoL.transform : null),
                                       duracionGiroHaciaPuerta);
        BloquearJugador(false);

        golpesSilenciados = false;
        if (notaPuerta != null) notaPuerta.gameObject.SetActive(true);
        ActualizarObjetivo("Investiga la puerta del sótano");
    }

    void SilenciarGolpes()
    {
        golpesSilenciados = true;
        if (sonidoGolpesSotano != null) sonidoGolpesSotano.Stop();
        if (puertaSotanoL != null) puertaSotanoL.DesactivarGolpes();
        if (puertaSotanoR != null) puertaSotanoR.DesactivarGolpes();
    }

    // ═════════════════════════════════════════════════════════════
    //  4/5. NOTA → REGRESO AL BAÑO
    // ═════════════════════════════════════════════════════════════
    /// <summary>La llama NotaPuertaReestructura al cerrar el primer plano de la nota.</summary>
    public void NotaLeida()
    {
        if (estadoActual != Act2State.Sotano) return;
        StartCoroutine(SecuenciaNotaLeida());
    }

    IEnumerator SecuenciaNotaLeida()
    {
        estadoActual = Act2State.Bano;
        if (!golpesSilenciados) SilenciarGolpes();
        SumarParanoia(10f);

        yield return new WaitForSeconds(1f);

        // El salón: completamente desordenado y todo tirado; luces rojas, todo tenso.
        PantallaNegra(true);
        yield return new WaitForSeconds(0.15f);
        parpadeandoLucesServicio = false;
        if (barOrdenado != null) barOrdenado.SetActive(false);
        if (barDesordenado != null) barDesordenado.SetActive(true);
        CambiarIluminacion("Psicosis");
        PantallaNegra(false);

        if (parpadearLucesRojas && !parpadeandoLucesRojas)
        {
            parpadeandoLucesRojas = true;
            StartCoroutine(ParpadeoLucesPsicosis());
        }

        // "Los dos cubículos están abiertos. El tercero sigue cerrado, pero ahora la puerta cede."
        AbrirPuertasDelBano();

        if (llaveObjeto != null) llaveObjeto.gameObject.SetActive(true);
        else Debug.LogError("[Act2ManagerReestructura] Falta 'Llave Objeto': no hay llave para agarrar.");
        ActualizarObjetivo("Buscá la llave en el baño de hombres");
    }

    void AbrirPuertasDelBano()
    {
        if (controlFueraDeServicio != null) controlFueraDeServicio.Habilitar();
        else Debug.LogWarning("[Act2ManagerReestructura] Falta 'Control Fuera De Servicio' (ControlPuertaFisicaReestructura del cubículo fuera de servicio).");

        if (controlesQueSeCierranDeGolpe != null)
            foreach (ControlPuertaFisicaReestructura c in controlesQueSeCierranDeGolpe) if (c != null) c.AbrirYTrabar();
    }

    // ═════════════════════════════════════════════════════════════
    //  5. LLAVE (la llama LlaveInteractuableReestructura)
    // ═════════════════════════════════════════════════════════════
    public void LlaveRecogida()
    {
        llaveTenida = true;
        estadoActual = Act2State.Vigilante;
        aparicionesVigilanteHechas = 0;
        SumarParanoia(15f);

        // "Al agarrarla escuchamos ruidos de las puertas de los cubículos de al lado cerrándose de golpe."
        if (sonidoPortazosCubiculos != null) sonidoPortazosCubiculos.Play();
        if (controlesQueSeCierranDeGolpe != null)
            foreach (ControlPuertaFisicaReestructura c in controlesQueSeCierranDeGolpe) if (c != null) c.CerrarDeGolpe();

        // "Escuchamos de lejos ruidos de Sombras en el salón acercándose."
        if (ruidoSombrasLejos != null) StartCoroutine(FadeInAudio(ruidoSombrasLejos, 15f, 0.7f));

        ActualizarObjetivo("Salí del baño");

        if (FindObjectsByType<AparicionVigilanteReestructura>().Length == 0)
        {
            Debug.LogWarning("[Act2ManagerReestructura] No hay AparicionVigilanteReestructura en la escena: el combate empieza solo en 3 s.");
            StartCoroutine(CombateConDelay(3f));
        }
    }

    public bool TieneLlave() => llaveTenida;

    IEnumerator CombateConDelay(float s)
    {
        yield return new WaitForSeconds(s);
        IniciarCombateFinal();
    }

    // ═════════════════════════════════════════════════════════════
    //  6. VIGILANTE (lo llama AparicionVigilanteReestructura) + COMBATE
    // ═════════════════════════════════════════════════════════════
    public bool PuedeAparecerVigilante(int orden)
    {
        return estadoActual == Act2State.Vigilante && orden == aparicionesVigilanteHechas;
    }

    public void AparicionVigilanteTerminada(int orden, bool esLaUltima)
    {
        aparicionesVigilanteHechas = orden + 1;
        if (esLaUltima) IniciarCombateFinal();
        else if (segundosMaxEsperaVigilante > 0f) StartCoroutine(SeguroVigilante(aparicionesVigilanteHechas));
    }

    IEnumerator SeguroVigilante(int aparicionEsperada)
    {
        yield return new WaitForSeconds(segundosMaxEsperaVigilante);
        if (estadoActual == Act2State.Vigilante && aparicionesVigilanteHechas == aparicionEsperada)
        {
            Debug.Log("[Act2ManagerReestructura] La siguiente aparición del Vigilante no se disparó a tiempo: empieza el combate.");
            IniciarCombateFinal();
        }
    }

    void IniciarCombateFinal()
    {
        if (combateFinalIniciado) return;
        combateFinalIniciado = true;
        estadoActual = Act2State.Psicosis;

        if (ruidoSombrasLejos != null) ruidoSombrasLejos.Stop();
        CambiarIluminacion("Psicosis");
        if (efectoPsicosis != null) efectoPsicosis.ActivarPsicosis();
        SumarParanoia(20f);

        if (sombrasCombate != null) sombrasCombate.IniciarCombate();
        else Debug.LogError("[Act2ManagerReestructura] Falta 'Sombras Combate': no van a aparecer sombras.");

        // Durante el combate el jugador pasa por una mesa: hay una niña sentada
        if (figuraNino != null) figuraNino.Aparecer();

        ActualizarObjetivo("Atravesá el salón y llegá a la puerta del sótano");

        if (segundosMaxCombateSinPilas > 0f)
            rutinaSinPilasPorTiempo = StartCoroutine(SinPilasPorTiempo());
    }

    /// <summary>La llama FiguraNinoReestructura cuando Pilar desaparece.</summary>
    public void PilarDesaparecio()
    {
        if (estadoActual != Act2State.Psicosis || sinPilasProgramado) return;
        StartCoroutine(SinPilasEn(segundosHastaSinPilas));
    }

    IEnumerator SinPilasPorTiempo()
    {
        yield return new WaitForSeconds(segundosMaxCombateSinPilas);
        if (!sinPilasProgramado && estadoActual == Act2State.Psicosis) StartCoroutine(SinPilasEn(0f));
    }

    IEnumerator SinPilasEn(float segundos)
    {
        sinPilasProgramado = true;
        yield return new WaitForSeconds(segundos);
        if (estadoActual != Act2State.Psicosis) yield break;

        if (linternaSinPilas == null) linternaSinPilas = FindAnyObjectByType<LinternaSinPilasReestructura>();
        if (linternaSinPilas != null) linternaSinPilas.AgotarPilas();
        else Debug.LogWarning("[Act2ManagerReestructura] No hay LinternaSinPilasReestructura: la linterna no se va a quedar sin pilas.");

        ActualizarObjetivo("¡Corré hasta la puerta del sótano!");
    }

    // ═════════════════════════════════════════════════════════════
    //  7. REGRESO A LA PUERTA DEL SÓTANO / CIERRE
    // ═════════════════════════════════════════════════════════════
    /// <summary>La llave solo abre la puerta durante el combate (no se puede saltear el Vigilante ni las Sombras).</summary>
    public bool PuedeUsarLlave() => llaveTenida && estadoActual == Act2State.Psicosis;

    /// <summary>La llaman PuertaSotanoAct2Reestructura ([E]) y TriggerCierreSotanoReestructura.</summary>
    public void UsarLlave()
    {
        if (!PuedeUsarLlave()) return;
        StartCoroutine(SecuenciaAperturaSotano());
    }

    IEnumerator SecuenciaAperturaSotano()
    {
        estadoActual = Act2State.Cierre;
        llaveTenida = false;

        // Lucas introduce la llave y la puerta se abre
        if (sonidoAbrirCandado != null) sonidoAbrirCandado.Play();
        yield return new WaitForSeconds(0.6f);

        // En ese instante todo se detiene y "vuelve a la normalidad"
        if (rutinaSinPilasPorTiempo != null) StopCoroutine(rutinaSinPilasPorTiempo);
        parpadeandoLucesServicio = false;
        parpadeandoLucesRojas = false;

        if (sombrasCombate != null) sombrasCombate.DesactivarTodo();     // Las Sombras desaparecen
        if (efectoPsicosis != null) efectoPsicosis.DesactivarPsicosis(); // La distorsión desaparece
        if (sonidoEstatica != null) sonidoEstatica.Stop();               // La estática desaparece

        AudioSource[] callar = { ambientBar, musicBar, musicaSuspenso, audioBasement, sonidoGolpesSotano, ruidoSombrasLejos };
        foreach (AudioSource a in callar) if (a != null) a.Stop();
        if (audiosASilenciar != null) foreach (AudioSource a in audiosASilenciar) if (a != null) a.Stop();

        if (figuraNino != null) figuraNino.OcultarTodo();

        CambiarIluminacion("Normal");                                   // Las luces vuelven
        if (barDesordenado != null) barDesordenado.SetActive(false);    // El bar está impecable
        if (barOrdenado != null) barOrdenado.SetActive(true);
        if (ParanoiaSystem.Instance != null) ParanoiaSystem.Instance.ResetParanoia();
        if (textoObjetivo != null) textoObjetivo.text = "";             // Silencio absoluto

        // La puerta está abierta: unos pocos escalones y oscuridad
        if (puertaSotanoL != null) puertaSotanoL.AbrirSola();
        if (puertaSotanoR != null) puertaSotanoR.AbrirSola();
        puertaSotanoAbierta = true;

        // Lucas se frena al pisar el trigger de la escalera. Si no hay trigger, se frena solo en 4 s;
        // si hay, pero el jugador no avanza, se frena igual después de "segundosMaxEsperaEscalera".
        float espera = segundosMaxEsperaEscalera;
        if (!HayTrigger(TipoTriggerReestructura.FrenoEscaleraSotano))
        {
            Debug.LogWarning("[Act2ManagerReestructura] No hay TriggerZonaReestructura 'FrenoEscaleraSotano': Lucas se frena solo en 4 s.");
            espera = 4f;
        }
        if (espera > 0f)
        {
            yield return new WaitForSeconds(espera);
            if (!lucasSeFreno) StartCoroutine(LucasSeFrena());
        }
    }

    IEnumerator LucasSeFrena()
    {
        lucasSeFreno = true;
        BloquearJugador(true);   // perdemos control sobre el personaje

        // Vuelve unos pasos hacia atrás
        if (jugadorCtrl != null)
        {
            CharacterController cc = jugadorCtrl.GetComponent<CharacterController>();
            Vector3 atras = -jugadorCtrl.transform.forward;
            atras.y = 0f;
            atras.Normalize();
            float velocidad = distanciaRetroceso / Mathf.Max(0.01f, duracionRetroceso);
            float t = 0f;
            while (t < duracionRetroceso)
            {
                t += Time.deltaTime;
                Vector3 paso = (atras * velocidad + Vector3.down * 2f) * Time.deltaTime;
                if (cc != null) cc.Move(paso);
                else jugadorCtrl.transform.position += atras * velocidad * Time.deltaTime;
                yield return null;
            }
        }

        yield return Decir(dialogoFinal);

        // CORTE A NEGRO
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
        }
        if (canvasGroupDialogo != null) canvasGroupDialogo.alpha = 0f;
        if (textoSubtitulos != null) textoSubtitulos.text = "";

        // Se escucha el sonido de una niña jugando. Una pequeña risa.
        if (risaNina != null) risaNina.Play();

        Debug.Log("[Act2ManagerReestructura] Noche 2 finalizada.");
        yield return new WaitForSeconds(segundosEnNegroAntesDeCambiar);

        if (string.IsNullOrEmpty(escenaSiguiente)) yield break;
        if (Application.CanStreamedLevelBeLoaded(escenaSiguiente)) SceneManager.LoadScene(escenaSiguiente);
        else Debug.LogError($"[Act2ManagerReestructura] La escena '{escenaSiguiente}' no está en File > Build Profiles > Scene List (o el nombre está mal escrito).");
    }

    // ═════════════════════════════════════════════════════════════
    //  UTILIDADES
    // ═════════════════════════════════════════════════════════════
    public void ActualizarObjetivo(string nuevoObjetivo)
    {
        if (textoObjetivo != null) textoObjetivo.text = "- " + nuevoObjetivo;
    }

    /// <summary>Muestra un diálogo con efecto máquina de escribir.</summary>
    public void MostrarDialogo(string mensaje)
    {
        if (textoSubtitulos == null)
        {
            Debug.LogError("[Act2ManagerReestructura] ¡'Texto Subtitulos' NO está asignado en el Inspector! Diálogo: " + mensaje);
            return;
        }
        if (rutinaDialogo != null) StopCoroutine(rutinaDialogo);
        rutinaDialogo = StartCoroutine(SecuenciaDialogo(mensaje));
    }

    IEnumerator SecuenciaDialogo(string frase)
    {
        textoSubtitulos.text = "";

        if (canvasGroupDialogo != null)
            while (canvasGroupDialogo.alpha < 1f)
            {
                canvasGroupDialogo.alpha += Time.deltaTime * velocidadFade;
                yield return null;
            }

        foreach (char letra in frase)
        {
            textoSubtitulos.text += letra;
            yield return new WaitForSeconds(velocidadEscritura);
        }

        yield return new WaitForSeconds(3f);

        if (canvasGroupDialogo != null)
            while (canvasGroupDialogo.alpha > 0f)
            {
                canvasGroupDialogo.alpha -= Time.deltaTime * (velocidadFade / 2f);
                yield return null;
            }

        textoSubtitulos.text = "";
        rutinaDialogo = null;
    }

    /// <summary>Muestra un diálogo y espera a que se termine de escribir + un rato de lectura.</summary>
    IEnumerator Decir(string frase)
    {
        if (string.IsNullOrEmpty(frase)) yield break;
        MostrarDialogo(frase);
        yield return new WaitForSeconds(frase.Length * Mathf.Max(0.005f, velocidadEscritura) + tiempoLecturaDialogo);
    }

    /// <summary>Sube (o baja, con valores negativos) la paranoia de forma segura.</summary>
    public void SumarParanoia(float valor)
    {
        if (valor == 0f) return;
        ParanoiaSystem p = ParanoiaSystem.Instance;
        if (p == null) { Debug.LogWarning($"[Act2ManagerReestructura] No hay ParanoiaSystem en la escena. Valor ignorado: {valor}"); return; }

        // AddParanoia ignora los negativos, así que para bajar se usa SetParanoia
        if (valor < 0f) p.SetParanoia(p.paranoiaActual + valor);
        else p.AddParanoia(valor);
    }

    /// <summary>Bloquea / desbloquea el movimiento y la cámara del jugador (PlayerController).</summary>
    public void BloquearJugador(bool bloquear)
    {
        if (jugadorCtrl == null) jugadorCtrl = FindAnyObjectByType<PlayerController>();
        if (jugadorCtrl != null) jugadorCtrl.controlesBloqueados = bloquear;
        else if (bloquear) Debug.LogWarning("[Act2ManagerReestructura] No hay PlayerController en la escena: no se puede bloquear al jugador.");
    }

    public void CambiarIluminacion(string estado)
    {
        if (lucesNormales != null) lucesNormales.SetActive(false);
        if (lucesServicio != null) lucesServicio.SetActive(false);
        if (lucesPsicosis != null) lucesPsicosis.SetActive(false);
        if (lucesTension  != null) lucesTension.SetActive(false);

        switch (estado)
        {
            case "Normal":   if (lucesNormales != null) lucesNormales.SetActive(true); break;
            case "Servicio": if (lucesServicio != null) lucesServicio.SetActive(true); break;
            case "Psicosis": if (lucesPsicosis != null) lucesPsicosis.SetActive(true); break;
            case "Tension":
                // "Todo tenso y oscuro". Sin luces de tensión: las de servicio parpadeando cada 3 s.
                if (lucesTension != null) lucesTension.SetActive(true);
                else
                {
                    if (lucesServicio != null) lucesServicio.SetActive(true);
                    if (!parpadeandoLucesServicio) { parpadeandoLucesServicio = true; StartCoroutine(ParpadeoLucesServicio()); }
                }
                break;
            default: Debug.LogWarning("[Act2ManagerReestructura] Estado de luz desconocido: " + estado); break;
        }
    }

    IEnumerator ParpadeoLucesServicio()
    {
        bool encendidas = true;
        while (parpadeandoLucesServicio)
        {
            yield return new WaitForSeconds(3f);
            if (!parpadeandoLucesServicio) yield break;
            encendidas = !encendidas;
            if (lucesServicio != null) lucesServicio.SetActive(encendidas);
        }
    }

    IEnumerator ParpadeoLucesPsicosis()
    {
        bool encendidas = true;
        while (parpadeandoLucesRojas)
        {
            yield return new WaitForSeconds(1f);
            if (!parpadeandoLucesRojas) yield break;
            encendidas = !encendidas;
            if (lucesPsicosis != null) lucesPsicosis.SetActive(encendidas);
        }
    }

    IEnumerator FundirDesdeNegro()
    {
        fadeCanvasGroup.alpha = 1f;
        yield return new WaitForSeconds(0.5f);
        yield return Fundir(1f, 0f, 2f);
    }

    IEnumerator Fundir(float desde, float hasta, float duracion)
    {
        if (fadeCanvasGroup == null) yield break;
        fadeCanvasGroup.gameObject.SetActive(true);
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        fadeCanvasGroup.alpha = hasta;
        fadeCanvasGroup.blocksRaycasts = hasta > 0.5f;
    }

    IEnumerator FadeCanvas(CanvasGroup cg, float desde, float hasta, float duracion)
    {
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        cg.alpha = hasta;
    }

    IEnumerator FadeInAudio(AudioSource audio, float duracionFade, float volumenObjetivo)
    {
        if (audio == null) yield break;
        audio.volume = 0f;
        audio.loop = true;
        audio.Play();

        float t = 0f;
        while (t < duracionFade)
        {
            t += Time.deltaTime;
            audio.volume = Mathf.Lerp(0f, volumenObjetivo, t / duracionFade);
            yield return null;
        }
        audio.volume = volumenObjetivo;
    }

    void PantallaNegra(bool mostrar)
    {
        GameObject negro = pantallaNegraFlash != null ? pantallaNegraFlash
                         : (efectoParpadeo != null ? efectoParpadeo.pantallaNegra : null);
        if (negro != null) { negro.SetActive(mostrar); return; }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = mostrar ? 1f : 0f;
        }
    }

    void ApagarTodasLasLuces()
    {
        if (lucesNormales != null) lucesNormales.SetActive(false);
        if (lucesServicio != null) lucesServicio.SetActive(false);
        if (lucesPsicosis != null) lucesPsicosis.SetActive(false);
        if (lucesTension != null) lucesTension.SetActive(false);
    }

    IEnumerator ParpadeoEncendido(GameObject luces, float duracion)
    {
        if (luces == null) yield break;
        float t = 0f;
        while (t < duracion)
        {
            luces.SetActive(!luces.activeSelf);
            float paso = Random.Range(0.05f, 0.2f);
            t += paso;
            yield return new WaitForSeconds(paso);
        }
        luces.SetActive(true);
    }

    IEnumerator GirarJugadorHacia(Transform objetivo, float duracion)
    {
        if (objetivo == null || jugadorCtrl == null) yield break;

        Transform j = jugadorCtrl.transform;
        Vector3 dir = objetivo.position - j.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) yield break;

        Quaternion desde = j.rotation;
        Quaternion hasta = Quaternion.LookRotation(dir);
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            j.rotation = Quaternion.Slerp(desde, hasta, Mathf.SmoothStep(0f, 1f, t / duracion));
            yield return null;
        }
        j.rotation = hasta;
    }

    TextMeshProUGUI CrearTextoEmergencia()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[Act2ManagerReestructura] No hay Canvas en la escena. Los diálogos no se van a ver.");
            return null;
        }
        GameObject go = new GameObject("TextoSubtitulos_Auto");
        go.transform.SetParent(canvas.transform, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.05f);
        rect.anchorMax = new Vector2(0.9f, 0.2f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        Debug.LogWarning("[Act2ManagerReestructura] 'Texto Subtitulos' estaba vacío: se creó uno automático. Asignalo en el Inspector.");
        return tmp;
    }

    // ═════════════════════════════════════════════════════════════
    //  DEBUG — saltar a cualquier fase (campo "Iniciar En Fase", menú contextual o Act2DebugHelperReestructura)
    // ═════════════════════════════════════════════════════════════
    public void DebugSaltarA(FaseDebugReestructura fase) => DebugSaltarA(fase, false);

    /// <param name="alArrancar">true = se llama al darle Play (se mantiene el fundido de entrada desde negro).</param>
    void DebugSaltarA(FaseDebugReestructura fase, bool alArrancar)
    {
        StopAllCoroutines();
        rutinaDialogo = null;
        if (jugadorCtrl == null) jugadorCtrl = FindAnyObjectByType<PlayerController>();
        if (clientesNormalesEnOrden == null) clientesNormalesEnOrden = new ClienteNormalReestructura[0];

        if (fadeCanvasGroup != null)
        {
            if (alArrancar) StartCoroutine(FundirDesdeNegro());   // StopAllCoroutines cortó el fundido del Start: se vuelve a lanzar
            else { fadeCanvasGroup.alpha = 0f; fadeCanvasGroup.blocksRaycasts = false; }
        }
        if (camaraIntroExterior != null) camaraIntroExterior.SetActive(false);
        if (tituloNoche != null) tituloNoche.alpha = 0f;
        if (notificacionCelularUI != null) notificacionCelularUI.SetActive(false);
        BloquearJugador(false);

        if (fase >= FaseDebugReestructura.Servicio)
        {
            tareasTerminadas = true;
            parpadeo1Hecho = true;
            tieneEscoba = tieneTrapo = tieneCepillo = true;
        }

        switch (fase)
        {
            case FaseDebugReestructura.Normal:
                StartCoroutine(SecuenciaIntro());
                break;

            case FaseDebugReestructura.Tareas:
                CambiarIluminacion("Normal");
                estadoActual = Act2State.Tareas;
                tareaActual = PrimeraTareaPendiente();
                ActualizarObjetivoTareas();
                break;

            case FaseDebugReestructura.Servicio:
                CambiarIluminacion("Servicio");
                if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(true);
                if (ambientBar != null && !ambientBar.isPlaying) ambientBar.Play();
                IniciarServicio();
                break;

            case FaseDebugReestructura.ClienteCorrupto:
                CambiarIluminacion("Servicio");
                if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(true);
                estadoActual = Act2State.Servicio;
                foreach (ClienteNormalReestructura c in clientesNormalesEnOrden)
                    if (c != null) { c.estado = ClienteNormalReestructura.Estado.Atendido; c.esSuTurno = false; }
                indiceClienteNormal = clientesNormalesEnOrden.Length;
                HabilitarSiguienteCliente();
                break;

            case FaseDebugReestructura.Pasillo:
                CambiarIluminacion("Servicio");
                if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(true);
                if (clienteCorrupto != null) clienteCorrupto.esSuTurno = false;
                IrACocina();
                break;

            case FaseDebugReestructura.PuertaSotano:
                estadoActual = Act2State.Pasillo;
                ZapatosEncontrados();
                break;

            case FaseDebugReestructura.RegresoBano:
                CambiarIluminacion("Tension");
                estadoActual = Act2State.Sotano;
                if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(false);
                if (pasilloEfecto != null) pasilloEfecto.DesactivarEfecto();
                NotaLeida();
                break;

            case FaseDebugReestructura.Vigilante:
            case FaseDebugReestructura.Combate:
                if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(false);
                if (pasilloEfecto != null) pasilloEfecto.DesactivarEfecto();
                golpesSilenciados = true;
                if (barOrdenado != null) barOrdenado.SetActive(false);
                if (barDesordenado != null) barDesordenado.SetActive(true);
                CambiarIluminacion("Psicosis");
                if (controlFueraDeServicio != null) controlFueraDeServicio.Habilitar();
                if (llaveObjeto != null) llaveObjeto.gameObject.SetActive(false);
                LlaveRecogida();
                if (fase == FaseDebugReestructura.Combate) IniciarCombateFinal();
                break;

            case FaseDebugReestructura.Cierre:
                if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(false);
                if (pasilloEfecto != null) pasilloEfecto.DesactivarEfecto();
                llaveTenida = true;
                estadoActual = Act2State.Psicosis;
                UsarLlave();
                break;
        }

        Debug.Log($"[Act2ManagerReestructura] DEBUG: saltando a la fase {fase}.");
    }

    [ContextMenu("▶ Saltar a TAREAS (en Play)")]           void N2_Tareas()    => SaltarDesdeMenu(FaseDebugReestructura.Tareas);
    [ContextMenu("▶ Saltar a SERVICIO (en Play)")]         void N2_Servicio()  => SaltarDesdeMenu(FaseDebugReestructura.Servicio);
    [ContextMenu("▶ Saltar a CLIENTE CORRUPTO (en Play)")] void N2_Corrupto()  => SaltarDesdeMenu(FaseDebugReestructura.ClienteCorrupto);
    [ContextMenu("▶ Saltar a PASILLO (en Play)")]          void N2_Pasillo()   => SaltarDesdeMenu(FaseDebugReestructura.Pasillo);
    [ContextMenu("▶ Saltar a PUERTA SÓTANO (en Play)")]    void N2_Puerta()    => SaltarDesdeMenu(FaseDebugReestructura.PuertaSotano);
    [ContextMenu("▶ Saltar a REGRESO AL BAÑO (en Play)")]  void N2_Bano()      => SaltarDesdeMenu(FaseDebugReestructura.RegresoBano);
    [ContextMenu("▶ Saltar a VIGILANTE (en Play)")]        void N2_Vigilante() => SaltarDesdeMenu(FaseDebugReestructura.Vigilante);
    [ContextMenu("▶ Saltar a COMBATE (en Play)")]          void N2_Combate()   => SaltarDesdeMenu(FaseDebugReestructura.Combate);
    [ContextMenu("▶ Saltar a CIERRE (en Play)")]           void N2_Cierre()    => SaltarDesdeMenu(FaseDebugReestructura.Cierre);

    void SaltarDesdeMenu(FaseDebugReestructura fase)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[Act2ManagerReestructura] Esto funciona solo con el juego en Play. Para arrancar en una fase sin Play, usá el campo 'Iniciar En Fase'.");
            return;
        }
        DebugSaltarA(fase);
    }

    // ═════════════════════════════════════════════════════════════
    //  VERIFICAR Y AUTO-CONFIGURAR (clic derecho en el componente)
    // ═════════════════════════════════════════════════════════════
    [ContextMenu("⚠ Verificar escena")]
    void VerificarEscena()
    {
        int errores = 0, avisos = 0;
        void Error(bool falta, string msg) { if (falta) { Debug.LogError("[Noche 2] FALTA: " + msg); errores++; } }
        void Aviso(bool falta, string msg) { if (falta) { Debug.LogWarning("[Noche 2] Opcional / revisar: " + msg); avisos++; } }

        // 1) Scripts de la demo que NO tienen que estar
        foreach (MonoBehaviour mb in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
        {
            if (mb == null || !mb.gameObject.scene.IsValid()) continue;
            string n = mb.GetType().Name;
            if (System.Array.IndexOf(ScriptsDeLaDemo, n) >= 0)
            {
                Debug.LogError($"[Noche 2] SOBRA: '{mb.gameObject.name}' tiene {n} (script de la DEMO). " +
                               (n == "TriggerDesaparicion" || n == "TriggerAparicionJumpscare" || n == "VigenteMirror" || n == "ParpadeoBarCambio"
                                   ? "En la reestructura no se usa: quitá el componente."
                                   : $"Reemplazalo por {n}Reestructura."), mb.gameObject);
                errores++;
            }
        }

        // 2) Referencias obligatorias
        Error(textoSubtitulos == null, "Texto Subtitulos (sin esto NO se ven los diálogos)");
        Aviso(canvasGroupDialogo == null, "Canvas Group Dialogo (sin esto los diálogos aparecen sin fundido)");
        Error(textoObjetivo == null, "Texto Objetivo");
        Error(fadeCanvasGroup == null, "Fade Canvas Group (panel negro)");
        Error(lucesNormales == null || lucesServicio == null || lucesPsicosis == null, "Luces Normales / Luces Servicio / Luces Psicosis");
        Error(FindAnyObjectByType<PlayerController>() == null, "PlayerController en el jugador");
        Error(ParanoiaSystem.Instance == null && FindAnyObjectByType<ParanoiaSystem>() == null, "ParanoiaSystem en la escena");
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        Error(jugador == null, "un objeto con el tag 'Player' (el jugador)");

        ZonaLimpiezaReestructura[] zonas = FindObjectsByType<ZonaLimpiezaReestructura>(FindObjectsInactive.Include);
        int b = 0, ba = 0, bn = 0;
        foreach (ZonaLimpiezaReestructura z in zonas)
        {
            if (z.tarea == TareaReestructura.Barrer) b++; else if (z.tarea == TareaReestructura.Barra) ba++; else bn++;
            Aviso(z.GetComponent<Collider>() == null, $"la zona '{z.name}' no tiene Collider (no se va a poder limpiar)");
        }
        Error(zonas.Length == 0, "ZonaLimpiezaReestructura (montículos, barra, exhibidor, inodoros)");
        Debug.Log($"[Noche 2] Zonas de limpieza: barrer={b} (guion: 3), barra/exhibidor={ba}, baños={bn}");
        Aviso(FindObjectsByType<HerramientaLimpiezaReestructura>(FindObjectsInactive.Include).Length == 0,
              "HerramientaLimpiezaReestructura (escoba/trapo/cepillo). Si no hay, poné 'Herramienta Requerida = Ninguna' en las zonas");
        Aviso(!HayTrigger(TipoTriggerReestructura.ParpadeoSalidaBanos), "TriggerZonaReestructura 'ParpadeoSalidaBanos' (si falta, el parpadeo pasa solo)");
        Aviso(!HayTrigger(TipoTriggerReestructura.SilenciarGolpesSotano), "TriggerZonaReestructura 'SilenciarGolpesSotano'");
        Aviso(!HayTrigger(TipoTriggerReestructura.FrenoEscaleraSotano), "TriggerZonaReestructura 'FrenoEscaleraSotano' (si falta, Lucas se frena solo)");
        Aviso(barDesordenado == null, "Bar Desordenado (bar destruido del Parpadeo N°1 y del regreso al baño)");
        Aviso(efectoParpadeo == null && pantallaNegraFlash == null, "Efecto Parpadeo o Pantalla Negra Flash (flashazos negros)");

        Error(grupoClientesCorruptos == null, "Grupo Clientes Corruptos (padre de todos los clientes)");
        Error(FindAnyObjectByType<ClienteCorruptoReestructura>(FindObjectsInactive.Include) == null, "ClienteCorruptoReestructura");
        Aviso(FindObjectsByType<ClienteNormalReestructura>(FindObjectsInactive.Include).Length == 0, "ClienteNormalReestructura (clientes normales antes del corrupto)");
        Aviso(FindObjectsByType<MirarJugadorReestructura>(FindObjectsInactive.Include).Length == 0, "MirarJugadorReestructura en los clientes (se dan vuelta en el glitch)");
        Aviso(itemVasoVacio == null || itemCerveza == null, "Item Vaso Vacio / Item Cerveza (servicio de cerveza)");
        Error(FindAnyObjectByType<ZapatosNinoReestructura>(FindObjectsInactive.Include) == null, "ZapatosNinoReestructura en el depósito");
        Error(pasilloEfecto == null, "Pasillo Efecto (PasilloEfectoReestructura)");
        Error(pasilloEfecto != null && pasilloEfecto.cinemachineCam == null, "en PasilloEfectoReestructura: 'Cinemachine Cam'");
        Error(notaPuerta == null, "Nota Puerta (NotaPuertaReestructura)");
        Aviso(notaPuerta != null && notaPuerta.imagenNota == null, "en NotaPuertaReestructura: 'Imagen Nota' (el primer plano de la nota)");
        Error(puertaSotanoL == null, "Puerta Sotano L (PuertaSotanoAct2Reestructura)");
        Aviso(puntoPuertaSotano == null, "Punto Puerta Sotano (giro de cámara hacia la puerta; si falta usa la Puerta Sotano L)");
        Error(llaveObjeto == null, "Llave Objeto (LlaveInteractuableReestructura)");
        Aviso(controlFueraDeServicio == null, "Control Fuera De Servicio (ControlPuertaFisicaReestructura del cubículo fuera de servicio)");
        Aviso(controlesQueSeCierranDeGolpe == null || controlesQueSeCierranDeGolpe.Length == 0, "Controles Que Se Cierran De Golpe (puertas de al lado)");
        foreach (ControlPuertaFisicaReestructura c in FindObjectsByType<ControlPuertaFisicaReestructura>(FindObjectsInactive.Include))
            Error(c.puerta == null || c.puerta.GetComponent<HingeJoint>() == null, $"en el control '{c.name}': la 'Puerta' tiene que tener Rigidbody + Hinge Joint");

        AparicionVigilanteReestructura[] apariciones = FindObjectsByType<AparicionVigilanteReestructura>(FindObjectsInactive.Include);
        Aviso(apariciones.Length < 2, $"AparicionVigilanteReestructura (hay {apariciones.Length}, el guion pide 2)");
        bool hayUltima = false;
        foreach (AparicionVigilanteReestructura a in apariciones)
        {
            if (a.esLaUltima) hayUltima = true;
            Error(a.modeloVigilante == null || a.puntoAparicion == null, $"en '{a.name}': 'Modelo Vigilante' o 'Punto Aparicion'");
        }
        Error(apariciones.Length > 0 && !hayUltima, "marcar 'Es La Ultima' en la última AparicionVigilanteReestructura");
        Error(sombrasCombate == null, "Sombras Combate (SombrasCombateReestructura)");
        Error(sombrasCombate != null && sombrasCombate.prefabSombra == null, "en SombrasCombateReestructura: 'Prefab Sombra'");
        Error(figuraNino == null, "Figura Nino (FiguraNinoReestructura — Pilar)");
        Aviso(efectoPsicosis == null, "Efecto Psicosis (EfectoPsicosisReestructura)");
        Aviso(FindAnyObjectByType<LinternaSinPilasReestructura>(FindObjectsInactive.Include) == null, "LinternaSinPilasReestructura (la linterna se queda sin pilas)");
        Aviso(FindAnyObjectByType<TriggerCierreSotanoReestructura>(FindObjectsInactive.Include) == null, "TriggerCierreSotanoReestructura frente a la puerta del sótano (si falta, hay que apretar [E] en la puerta)");
        Aviso(!string.IsNullOrEmpty(escenaSiguiente) && Application.isPlaying && !Application.CanStreamedLevelBeLoaded(escenaSiguiente),
              $"la escena '{escenaSiguiente}' no está en la Scene List del Build Profile");

        Debug.Log(errores == 0
            ? $"[Noche 2] ✓ Escena lista ({avisos} avisos opcionales)."
            : $"[Noche 2] ✗ {errores} cosas para arreglar, {avisos} avisos. Hacé clic en cada mensaje rojo para ver qué objeto es.");
    }

    [ContextMenu("Auto-buscar referencias")]
    void AutoBuscarReferencias()
    {
        int antes = ContarVacios();

        // UI
        if (textoSubtitulos == null) { GameObject g = Buscar("TextoSubtitulos", "Dialogue", "Subtitulos", "DialogoText"); if (g != null) textoSubtitulos = g.GetComponent<TextMeshProUGUI>(); }
        if (canvasGroupDialogo == null) { GameObject g = Buscar("FondoDialogo"); if (g != null) canvasGroupDialogo = g.GetComponent<CanvasGroup>(); }
        if (canvasGroupDialogo == null && textoSubtitulos != null) canvasGroupDialogo = textoSubtitulos.GetComponentInParent<CanvasGroup>(true);
        if (textoObjetivo == null) { GameObject g = Buscar("Objetivos", "TextoObjetivo", "Objetivo"); if (g != null) textoObjetivo = g.GetComponent<TextMeshProUGUI>(); }
        if (fadeCanvasGroup == null) { GameObject g = Buscar("FadePanel", "PanelNegro", "FadeCanvas"); if (g != null) fadeCanvasGroup = g.GetComponent<CanvasGroup>(); }
        if (tituloNoche == null) { GameObject g = Buscar("TituloNoche"); if (g != null) tituloNoche = g.GetComponent<CanvasGroup>(); }
        if (overlayGlitch == null) { GameObject g = Buscar("OverlayGlitch"); if (g != null) overlayGlitch = g.GetComponent<CanvasGroup>(); }
        if (notificacionCelularUI == null) notificacionCelularUI = Buscar("NotificacionCelular");

        // Luces y estados del bar
        if (lucesNormales == null) lucesNormales = Buscar("LucesNormales", "Luces Normales");
        if (lucesServicio == null) lucesServicio = Buscar("LucesServicio", "Luces Servicio", "BarLight");
        if (lucesPsicosis == null) lucesPsicosis = Buscar("LucesPsicosis", "Luces Psicosis", "LucesCombate");
        if (lucesTension == null) lucesTension = Buscar("LucesTension", "Luces Tension");
        if (barOrdenado == null) barOrdenado = Buscar("BarOrdenado");
        if (barDesordenado == null) barDesordenado = Buscar("BarDesordenado");
        if (grupoClientesCorruptos == null) grupoClientesCorruptos = Buscar("--ClientGroup--", "ClientesCorruptos", "GrupoClientes", "Clientes");

        // Componentes
        if (efectoParpadeo == null) efectoParpadeo = FindAnyObjectByType<EffectoParpadeo>(FindObjectsInactive.Include);
        if (efectoPsicosis == null) efectoPsicosis = FindAnyObjectByType<EfectoPsicosisReestructura>(FindObjectsInactive.Include);
        if (sacudidaCamara == null) sacudidaCamara = FindAnyObjectByType<CameraShake>(FindObjectsInactive.Include);
        if (servicioCervezaVisual == null) servicioCervezaVisual = FindAnyObjectByType<ServicioCervezaVisual>(FindObjectsInactive.Include);
        if (pasilloEfecto == null) pasilloEfecto = FindAnyObjectByType<PasilloEfectoReestructura>(FindObjectsInactive.Include);
        if (notaPuerta == null) notaPuerta = FindAnyObjectByType<NotaPuertaReestructura>(FindObjectsInactive.Include);
        if (llaveObjeto == null) llaveObjeto = FindAnyObjectByType<LlaveInteractuableReestructura>(FindObjectsInactive.Include);
        if (sombrasCombate == null) sombrasCombate = FindAnyObjectByType<SombrasCombateReestructura>(FindObjectsInactive.Include);
        if (figuraNino == null) figuraNino = FindAnyObjectByType<FiguraNinoReestructura>(FindObjectsInactive.Include);
        if (linternaSinPilas == null) linternaSinPilas = FindAnyObjectByType<LinternaSinPilasReestructura>(FindObjectsInactive.Include);
        if (clienteCorrupto == null) clienteCorrupto = FindAnyObjectByType<ClienteCorruptoReestructura>(FindObjectsInactive.Include);
        if (clientesNormalesEnOrden == null || clientesNormalesEnOrden.Length == 0) clientesNormalesEnOrden = BuscarClientesOrdenados();

        if (puertaSotanoL == null)
        {
            PuertaSotanoAct2Reestructura[] puertas = FindObjectsByType<PuertaSotanoAct2Reestructura>(FindObjectsInactive.Include);
            System.Array.Sort(puertas, (x, y) => string.CompareOrdinal(x.name, y.name));
            if (puertas.Length > 0) puertaSotanoL = puertas[0];
            if (puertas.Length > 1 && puertaSotanoR == null) puertaSotanoR = puertas[1];
        }
        if (puntoPuertaSotano == null && puertaSotanoL != null) puntoPuertaSotano = puertaSotanoL.transform;

        // Controles de las puertas del baño (por rol)
        List<ControlPuertaFisicaReestructura> golpe = new List<ControlPuertaFisicaReestructura>();
        foreach (ControlPuertaFisicaReestructura c in FindObjectsByType<ControlPuertaFisicaReestructura>(FindObjectsInactive.Include))
        {
            if (c.rol == RolPuertaReestructura.FueraDeServicio) { if (controlFueraDeServicio == null) controlFueraDeServicio = c; }
            else golpe.Add(c);
        }
        if ((controlesQueSeCierranDeGolpe == null || controlesQueSeCierranDeGolpe.Length == 0) && golpe.Count > 0)
            controlesQueSeCierranDeGolpe = golpe.ToArray();

        // Audio: hijos de "--AudioSource--" (o "AudioSources" / "Sounds") según su nombre
        GameObject padreAudio = Buscar("--AudioSource--", "AudioSources", "Sounds");
        if (padreAudio != null)
        {
            foreach (AudioSource a in padreAudio.GetComponentsInChildren<AudioSource>(true))
            {
                string n = a.gameObject.name.ToLower();
                if (ambientBar == null && n.Contains("ambient")) ambientBar = a;
                else if (musicaSuspenso == null && n.Contains("suspenso")) musicaSuspenso = a;
                else if (audioBasement == null && (n.Contains("sotano") || n.Contains("basement"))) audioBasement = a;
                else if (musicBar == null && n.Contains("music")) musicBar = a;
                else if (sonidoEstatica == null && n.Contains("static")) sonidoEstatica = a;
                else if (sonidoGolpesSotano == null && (n.Contains("golpe") || n.Contains("knock"))) sonidoGolpesSotano = a;
                else if (sonidoAbrirCandado == null && (n.Contains("candado") || n.Contains("unlock") || n.Contains("llave"))) sonidoAbrirCandado = a;
                else if (risaNina == null && (n.Contains("risa") || n.Contains("laugh"))) risaNina = a;
            }
        }

        if (musicaSuspenso == null) { GameObject g = Buscar("MusicaSuspenso"); if (g != null) musicaSuspenso = g.GetComponent<AudioSource>(); }

        int despues = ContarVacios();
        Debug.Log($"[Act2ManagerReestructura] Auto-buscar: se completaron {antes - despues} campos. " +
                  "Después corré '⚠ Verificar escena' para ver qué falta asignar a mano.");
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    int ContarVacios()
    {
        Object[] campos =
        {
            textoSubtitulos, canvasGroupDialogo, textoObjetivo, fadeCanvasGroup, tituloNoche, overlayGlitch, notificacionCelularUI,
            lucesNormales, lucesServicio, lucesPsicosis, lucesTension, barOrdenado, barDesordenado, grupoClientesCorruptos,
            efectoParpadeo, efectoPsicosis, sacudidaCamara, servicioCervezaVisual, pasilloEfecto, notaPuerta, llaveObjeto,
            sombrasCombate, figuraNino, linternaSinPilas, clienteCorrupto, puertaSotanoL, puertaSotanoR, puntoPuertaSotano,
            controlFueraDeServicio, ambientBar, musicBar, sonidoEstatica, sonidoGolpesSotano, sonidoAbrirCandado, risaNina
        };
        int n = 0;
        foreach (Object o in campos) if (o == null) n++;
        if (clientesNormalesEnOrden == null || clientesNormalesEnOrden.Length == 0) n++;
        if (controlesQueSeCierranDeGolpe == null || controlesQueSeCierranDeGolpe.Length == 0) n++;
        return n;
    }

    /// <summary>Busca un GameObject de la escena por nombre (también los desactivados).</summary>
    static GameObject Buscar(params string[] nombres)
    {
        Transform[] todos = FindObjectsByType<Transform>(FindObjectsInactive.Include);
        foreach (string nombre in nombres)
            foreach (Transform t in todos)
                if (t.name == nombre && t.gameObject.scene.IsValid()) return t.gameObject;
        return null;
    }
}
