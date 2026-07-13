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
public class Act2Manager : MonoBehaviour
{
    public static Act2Manager Instance { get; private set; }

    public enum Act2State
    {
        Inicio,     // Parpadeos + cansancio del barman
        Servicio,   // Clientes corruptos entran al bar
        Pasillo,    // Efecto de corredor infinito hacia la cocina
        Sotano,     // Puerta del sótano: golpes y nota
        Bano,       // Baño: buscar la llave
        Psicosis,   // Combate de sombras + figura del niño
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

    [Header("Psicosis y Combate")]
    public SombrasCombate sombrasCombate;
    public FiguraNino figuraNino;
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

    [Header("Objetivos")]
    public TextMeshProUGUI textoObjetivo;

    void Awake()
    {
        Instance = this;

        // Aviso si hay un Act1Manager en la escena
        if (Object.FindAnyObjectByType<Act1Manager>() != null)
            Debug.LogError("[Act2Manager] ¡ACT1MANAGER DETECTADO EN LA ESCENA! Eliminalo del hierarchy para evitar conflictos.");
    }

    void Start()
    {
        // Si el texto de subtítulos no está asignado, lo crea automáticamente
        if (textoSubtitulos == null) textoSubtitulos = CrearTextoEmergencia();
        if (textoSubtitulos != null) textoSubtitulos.text = "";

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
        if (figuraNino != null)             figuraNino.gameObject.SetActive(true);
        if (notaPuerta != null)             notaPuerta.gameObject.SetActive(false);

        StartCoroutine(SecuenciaInicio());


        ActualizarObjetivo("Preparate para el servicio...");
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
        if (ambientBar != null && !ambientBar.isPlaying) ambientBar.Play();

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

        // Activar golpes rítmicos y nota en la puerta
        if (sonidoGolpesSotano != null) sonidoGolpesSotano.Play();
        if (puertaSotanoL != null)       puertaSotanoL.ActivarGolpes();
        if (notaPuerta != null)         notaPuerta.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        ActualizarObjetivo("Investiga los ruidos del sotano");
    }

    // =========================================================
    // ESTADO 4: SÓTANO — La nota da la pista del baño
    // =========================================================
    public void NotaLeida()
    {
    
    MostrarDialogo("Lucas: \"La guardé donde nadie limpia\"... el baño.");
        estadoActual = Act2State.Bano;
        ActualizarObjetivo("Ir a buscar la llave del sotano al baño");
        // Activar la llave en el baño
        if (llaveObjeto != null) llaveObjeto.gameObject.SetActive(true);
    }

    // =========================================================
    // ESTADO 5: BAÑO — La llave y El Vigilante en el espejo
    // =========================================================
    public void LlaveRecogida()
    {
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
    // ESTADO 6: PSICOSIS — Brote de sombras y figura del niño
    // =========================================================
    IEnumerator SecuenciaPsicosis()
    {
        yield return new WaitForSeconds(1.5f);

        estadoActual = Act2State.Psicosis;

        CambiarIluminacion("Psicosis");
        if (efectoPsicosis != null) efectoPsicosis.ActivarPsicosis();

        if (sacudidaCamara != null)
            StartCoroutine(sacudidaCamara.Shake(1.5f, 25f));

        Paranoia(40f);

        yield return new WaitForSeconds(3f);

        ActualizarObjetivo("SOBREVIVE Y LLEGA AL SOTANO");

        yield return new WaitForSeconds(2f);

        if (sombrasCombate != null)
            sombrasCombate.IniciarCombate();
        else
            Debug.LogError("[Act2Manager] sombrasCombate no asignado — las sombras no aparecerán. Asignalo en el Inspector o ejecutá Auto-buscar referencias.");

        // La figura del niño aparece después de un rato de combate
        yield return new WaitForSeconds(3f);

        if (figuraNino != null) figuraNino.Aparecer();
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

    IEnumerator SecuenciaCierre()
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

        if (sombrasCombate != null) sombrasCombate.DesactivarTodo();
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
    // UTILIDADES
    // =========================================================

    public void MostrarDialogo(string mensaje)
    {
        if (textoSubtitulos != null && canvasGroupDialogo != null)
        {
            // Si ya hay algo escribiéndose, lo matamos de raíz
            if (corrutinaActiva != null)
            {
                StopCoroutine(corrutinaActiva);
            }

            // Guardamos la nueva corrutina en la variable
            corrutinaActiva = StartCoroutine(SecuenciaDialogo(mensaje));
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
        Chk(sombrasCombate,        "sombrasCombate");
        Chk(figuraNino,            "figuraNino");
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

    [ContextMenu("▶ TEST: Saltar a PSICOSIS (sombras)")]
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

        // --- Psicosis ---
        if (sombrasCombate == null)
        {
            sombrasCombate = FindAnyObjectByType<SombrasCombate>();
            if (sombrasCombate != null) encontrados++;
        }
        if (figuraNino == null)
        {
            figuraNino = FindAnyObjectByType<FiguraNino>();
            if (figuraNino != null) encontrados++;
        }

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
}
