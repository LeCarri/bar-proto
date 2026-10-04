using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// =====================================================================================
//  ACT2MANAGER — REESTRUCTURA NOCHE 2 (guion "EL ÚLTIMO TURNO - NOCHE 2.3")
//
//  Este archivo es la SEGUNDA MITAD del mismo componente Act2Manager (clase "partial").
//  NO es un script aparte: no se agrega a ningún GameObject. Todos estos campos aparecen
//  en el Inspector del Act2Manager que ya está en la escena, debajo de los viejos.
//
//  Flujo nuevo:
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
// =====================================================================================
public partial class Act2Manager
{
    // ─────────────────────────────────────────────────────────────
    //  CAMPOS (Inspector)
    // ─────────────────────────────────────────────────────────────
    [Header("═════════ REESTRUCTURA NOCHE 2 ═════════")]
    [Tooltip("Para testear: desde qué fase arranca la noche al darle Play. Dejar en 'Normal' para el juego final.")]
    public FaseDebugAct2 iniciarEnFase = FaseDebugAct2.Normal;
    [Tooltip("Segundos extra que queda cada diálogo en pantalla antes de pasar al siguiente en las secuencias.")]
    public float tiempoLecturaDialogo = 1.8f;

    [Header("0. INTRO")]
    [Tooltip("OPCIONAL. Cámara (Camera o CinemachineCamera de prioridad alta) frente al bar, DESACTIVADA. Se acerca a la puerta y hace fundido.")]
    public GameObject camaraIntroExterior;
    [Tooltip("Punto al que se acerca la cámara de la intro (la puerta del bar).")]
    public Transform puntoPuertaIntro;
    public float duracionAcercamientoIntro = 4f;
    public AudioSource sonidoPuertaAbriendo;
    [Tooltip("Lucas llega con el bar a oscuras y en las tareas 'enciende la luz'.")]
    public bool empezarConLucesApagadas = true;
    [TextArea] public string dialogoIntro1 = "Lucas: No pude dormir nada ayer... Estoy que me desmayo...";
    [TextArea] public string dialogoIntro2 = "Lucas: Tengo que aguantar un poco…";
    [Tooltip("OPCIONAL del guion: notificación de Mariela + 'Después.'")]
    public bool usarEventoCelular = true;
    [Tooltip("UI con el texto 'Mariela — 1 mensaje nuevo'. Empieza DESACTIVADA.")]
    public GameObject notificacionCelularUI;
    public AudioSource sonidoVibracionCelular;
    [TextArea] public string dialogoCelular = "Lucas: Después.";
    [Tooltip("CanvasGroup con un TextMeshPro para el texto en pantalla de la noche.")]
    public CanvasGroup tituloNoche;
    [TextArea] public string textoTituloNoche = "- NOCHE 2 -\n19:46 hs.";

    [Header("1. TAREAS")]
    public AudioSource sonidoInterruptorLuz;
    [TextArea] public string dialogoTareas = "Lucas: Bueno… Una noche más";
    [Tooltip("Si es true, las tareas se hacen en orden: barrer → barra → baños.")]
    public bool tareasEnOrden = true;

    [Header("Estados del bar")]
    [Tooltip("Grupo con los objetos del bar ORDENADO (opcional).")]
    public GameObject barOrdenado;
    [Tooltip("Grupo con el bar DESORDENADO / destruido (sillas tiradas, vidrios...). Empieza desactivado.")]
    public GameObject barDesordenado;
    [Tooltip("Cuánto se ve el bar destruido en el Parpadeo N°1.")]
    public float duracionFlashDesordenado = 1f;
    [Tooltip("Imagen negra a pantalla completa para los flashazos. Vacío = usa la del EffectoParpadeo.")]
    public GameObject pantallaNegraFlash;

    [Header("2. SERVICIO")]
    [Tooltip("Clientes normales EN ORDEN. Vacío = los busca solos (ordenados por nombre).")]
    public ClienteAct2[] clientesNormalesEnOrden;
    [Tooltip("El cliente corrupto (el último). Vacío = busca el primer ClienteCorrupto con 'Hace Pedido'.")]
    public ClienteCorrupto clienteCorrupto;
    [TextArea] public string dialogoLucasPedido = "Lucas: ¿Qué le sirvo?";
    [TextArea] public string dialogoClienteCorrupto = "Cliente_1: Traeme lo más fuerte que tengas.";
    [Tooltip("Duración del glitch (el guion dice 2/3 segundos).")]
    public float duracionGlitch = 2.5f;
    [Tooltip("Panel de UI (distorsión / estática) que parpadea durante el glitch. Opcional.")]
    public CanvasGroup overlayGlitch;
    public AudioSource sonidoEstaticaGlitch;
    public AudioSource sonidoRespiracionGlitch;
    public float intensidadMaxShakeGlitch = 3f;

    [Header("3. PASILLO + ZAPATOS")]
    [TextArea] public string dialogoZapatos = "Lucas: No... No deberían estar acá...";
    [Tooltip("Luces 'tensas y oscuras' después de los zapatos. Vacío = luces de servicio parpadeando.")]
    public GameObject lucesTension;
    [Tooltip("Hacia dónde gira la cámara cuando empiezan los golpes (la puerta del sótano).")]
    public Transform puntoPuertaSotano;
    public float duracionGiroHaciaPuerta = 1.5f;

    [Header("4. BAÑO — puertas con física (no se modifican; ver ControlPuertaFisicaAct2)")]
    [Tooltip("ControlPuertaFisicaAct2 con rol FueraDeServicio que apunta a la puerta del tercer cubículo.")]
    public ControlPuertaFisicaAct2 controlFueraDeServicio;
    [Tooltip("ControlPuertaFisicaAct2 con rol SeCierraDeGolpe de los cubículos de al lado.")]
    public ControlPuertaFisicaAct2[] controlesQueSeCierranDeGolpe;
    [Tooltip("Sonido extra al cerrarse de golpe (opcional: Door.cs ya hace sonar las bisagras).")]
    public AudioSource sonidoPortazosCubiculos;

    [Header("5. VIGILANTE + COMBATE")]
    [Tooltip("Ruidos de Sombras en el salón, que se acercan (loop). Sube de volumen después de la llave.")]
    public AudioSource ruidoSombrasLejos;
    public bool parpadearLucesRojas = false;
    [Tooltip("Vacío = lo busca solo.")]
    public LinternaSinPilasAct2 linternaSinPilas;
    [Tooltip("Segundos desde que Pilar desaparece hasta que la linterna se queda sin pilas.")]
    public float segundosHastaSinPilas = 5f;
    [Tooltip("Por si el jugador nunca se acerca a Pilar: segundos de combate hasta quedarse sin pilas (0 = nunca).")]
    public float segundosMaxCombateSinPilas = 45f;

    [Header("6. CIERRE")]
    public AudioSource sonidoAbrirCandado;
    [Tooltip("Cualquier otro audio que tenga que callarse ('Silencio absoluto').")]
    public AudioSource[] audiosASilenciar;
    [TextArea] public string dialogoFinal = "Lucas: No… todavía no puedo… Después.";
    public float distanciaRetroceso = 1.5f;
    public float duracionRetroceso = 1.2f;
    [Tooltip("Sonido de una niña jugando / pequeña risa, en el negro final.")]
    public AudioSource risaNina;
    public float segundosEnNegroAntesDeCambiar = 5f;
    public string escenaSiguiente = "Night_3 Scene";

    // ─────────────────────────────────────────────────────────────
    //  ESTADO INTERNO
    // ─────────────────────────────────────────────────────────────
    private bool tieneEscoba, tieneTrapo, tieneCepillo;
    private readonly List<ZonaLimpiezaAct2> zonasLimpieza = new List<ZonaLimpiezaAct2>();
    private TareaAct2 tareaActual = TareaAct2.Barrer;
    private bool tareasTerminadas, parpadeo1Hecho, golpesSilenciados, puertaSotanoAbierta, lucasSeFreno;
    private int indiceClienteNormal;
    private int aparicionesVigilanteHechas;
    private bool combateFinalIniciado, sinPilasProgramado;
    private PlayerController jugadorCtrl;
    private Coroutine rutinaSinPilasPorTiempo;

    // ═════════════════════════════════════════════════════════════
    //  ARRANQUE (lo llama Start() en Act2Manager.cs)
    // ═════════════════════════════════════════════════════════════
    void IniciarNoche2()
    {
        jugadorCtrl = FindAnyObjectByType<PlayerController>();

        zonasLimpieza.Clear();
        zonasLimpieza.AddRange(FindObjectsByType<ZonaLimpiezaAct2>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        if (linternaSinPilas == null) linternaSinPilas = FindAnyObjectByType<LinternaSinPilasAct2>();

        // Clientes
        if (clientesNormalesEnOrden == null || clientesNormalesEnOrden.Length == 0)
        {
            ClienteAct2[] encontrados = FindObjectsByType<ClienteAct2>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            System.Array.Sort(encontrados, (a, b) => string.CompareOrdinal(a.name, b.name));
            clientesNormalesEnOrden = encontrados;
        }
        foreach (ClienteAct2 c in clientesNormalesEnOrden) if (c != null) c.esSuTurno = false;

        ClienteCorrupto[] corruptos = FindObjectsByType<ClienteCorrupto>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (ClienteCorrupto c in corruptos)
        {
            c.esSuTurno = false;   // nadie se atiende antes de tiempo
            if (clienteCorrupto == null && c.hacePedido) clienteCorrupto = c;
        }

        // Estado inicial del mundo
        if (barDesordenado != null) barDesordenado.SetActive(false);
        if (barOrdenado != null) barOrdenado.SetActive(true);
        if (notificacionCelularUI != null) notificacionCelularUI.SetActive(false);
        if (tituloNoche != null) tituloNoche.alpha = 0f;
        if (overlayGlitch != null) overlayGlitch.alpha = 0f;
        if (camaraIntroExterior != null) camaraIntroExterior.SetActive(false);
        if (lucesTension != null) lucesTension.SetActive(false);

        Debug.Log($"[Act2Manager] Noche 2 (reestructura). Zonas de limpieza: barrer {Total(TareaAct2.Barrer)}, " +
                  $"barra {Total(TareaAct2.Barra)}, baños {Total(TareaAct2.Banos)}. Clientes normales: {clientesNormalesEnOrden.Length}.");

        if (iniciarEnFase == FaseDebugAct2.Normal)
            StartCoroutine(SecuenciaIntroNueva());
        else
            DebugSaltarA(iniciarEnFase);
    }

    // ═════════════════════════════════════════════════════════════
    //  0. INTRO
    // ═════════════════════════════════════════════════════════════
    IEnumerator SecuenciaIntroNueva()
    {
        estadoActual = Act2State.Inicio;
        BloquearJugador(true);

        if (empezarConLucesApagadas) ApagarTodasLasLuces();

        if (camaraIntroExterior != null)
        {
            // Frente del bar, Lucas llegando... la cámara se acerca a la puerta
            camaraIntroExterior.SetActive(true);
            yield return new WaitForSeconds(2.5f);   // fundido de entrada del Start

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

    public void DarHerramienta(HerramientaAct2 h)
    {
        switch (h)
        {
            case HerramientaAct2.Escoba:  tieneEscoba = true;  break;
            case HerramientaAct2.Trapo:   tieneTrapo = true;   break;
            case HerramientaAct2.Cepillo: tieneCepillo = true; break;
        }
        if (estadoActual == Act2State.Tareas) ActualizarObjetivoTareas();
    }

    public bool TieneHerramienta(HerramientaAct2 h)
    {
        switch (h)
        {
            case HerramientaAct2.Escoba:  return tieneEscoba;
            case HerramientaAct2.Trapo:   return tieneTrapo;
            case HerramientaAct2.Cepillo: return tieneCepillo;
            default:                      return true;
        }
    }

    public bool PuedeHacerTarea(TareaAct2 t)
    {
        if (estadoActual != Act2State.Tareas) return false;
        return !tareasEnOrden || t == tareaActual;
    }

    public void RegistrarZonaLimpia(ZonaLimpiezaAct2 zona)
    {
        if (!zonasLimpieza.Contains(zona)) zonasLimpieza.Add(zona);

        // Tarea 1: "cuando falte solo un montículo para barrer, se activa la mancha que tiene cerca"
        if (zona.tarea == TareaAct2.Barrer && Total(TareaAct2.Barrer) - Hechas(TareaAct2.Barrer) == 1)
        {
            foreach (ZonaLimpiezaAct2 z in zonasLimpieza)
                if (z != null && z.tarea == TareaAct2.Barrer && !z.EstaCompletada && z.manchaCercana != null)
                    z.manchaCercana.Activar();
        }

        // ¿Terminó la tarea actual?
        if (tareasEnOrden)
        {
            if (Hechas(tareaActual) >= Total(tareaActual))
                tareaActual = PrimeraTareaPendiente();
        }

        bool todo = Hechas(TareaAct2.Barrer) >= Total(TareaAct2.Barrer) &&
                    Hechas(TareaAct2.Barra)  >= Total(TareaAct2.Barra) &&
                    Hechas(TareaAct2.Banos)  >= Total(TareaAct2.Banos);

        if (todo && !tareasTerminadas) TareasTerminadas();
        else ActualizarObjetivoTareas();
    }

    void TareasTerminadas()
    {
        tareasTerminadas = true;
        if (ControladorMano3D.Instance != null && ControladorMano3D.Instance.TieneManoOcupada())
            ControladorMano3D.Instance.VaciarMano();

        ActualizarObjetivo("Volvé al salón");

        // Si no hay trigger en el pasillito de los baños, el parpadeo pasa solo
        if (!ExisteTrigger(TipoTriggerAct2.ParpadeoSalidaBanos))
        {
            Debug.LogWarning("[Act2Manager] No hay TriggerZonaAct2 'ParpadeoSalidaBanos'. El Parpadeo N°1 se dispara solo en 3 s.");
            StartCoroutine(EsperarYParpadeo1());
        }
    }

    IEnumerator EsperarYParpadeo1()
    {
        yield return new WaitForSeconds(3f);
        if (!parpadeo1Hecho) StartCoroutine(ParpadeoPrimeraMutacion());
    }

    TareaAct2 PrimeraTareaPendiente()
    {
        if (Hechas(TareaAct2.Barrer) < Total(TareaAct2.Barrer)) return TareaAct2.Barrer;
        if (Hechas(TareaAct2.Barra)  < Total(TareaAct2.Barra))  return TareaAct2.Barra;
        return TareaAct2.Banos;
    }

    int Total(TareaAct2 t)
    {
        int n = 0;
        foreach (ZonaLimpiezaAct2 z in zonasLimpieza) if (z != null && z.tarea == t) n++;
        return n;
    }

    int Hechas(TareaAct2 t)
    {
        int n = 0;
        foreach (ZonaLimpiezaAct2 z in zonasLimpieza) if (z != null && z.tarea == t && z.EstaCompletada) n++;
        return n;
    }

    bool NecesitaHerramienta(TareaAct2 t, HerramientaAct2 h)
    {
        foreach (ZonaLimpiezaAct2 z in zonasLimpieza)
            if (z != null && z.tarea == t && !z.EstaCompletada && z.HerramientaNecesaria() == h) return true;
        return false;
    }

    void ActualizarObjetivoTareas()
    {
        if (estadoActual != Act2State.Tareas || tareasTerminadas) return;

        if (tareasEnOrden)
        {
            ActualizarObjetivo(TextoTarea(tareaActual));
        }
        else
        {
            ActualizarObjetivo(TextoTarea(TareaAct2.Barrer) + "\n- " + TextoTarea(TareaAct2.Barra) + "\n- " + TextoTarea(TareaAct2.Banos));
        }
    }

    string TextoTarea(TareaAct2 t)
    {
        switch (t)
        {
            case TareaAct2.Barrer:
                if (NecesitaHerramienta(t, HerramientaAct2.Escoba) && !tieneEscoba) return "Buscá la escoba en el depósito";
                return $"Barré el bar ({Hechas(t)}/{Total(t)})";
            case TareaAct2.Barra:
                if (NecesitaHerramienta(t, HerramientaAct2.Trapo) && !tieneTrapo) return "Buscá el trapo";
                return $"Limpiá la barra y el exhibidor ({Hechas(t)}/{Total(t)})";
            default:
                if (NecesitaHerramienta(t, HerramientaAct2.Cepillo) && !tieneCepillo) return "Buscá el cepillo para los baños";
                return $"Limpiá los baños ({Hechas(t)}/{Total(t)})";
        }
    }

    // ═════════════════════════════════════════════════════════════
    //  TRIGGERS GENÉRICOS (TriggerZonaAct2)
    // ═════════════════════════════════════════════════════════════
    /// <summary>Devuelve true si el trigger se usó (y no debe volver a dispararse).</summary>
    public bool TriggerZona(TipoTriggerAct2 tipo)
    {
        switch (tipo)
        {
            case TipoTriggerAct2.ParpadeoSalidaBanos:
                if (estadoActual == Act2State.Tareas && tareasTerminadas && !parpadeo1Hecho)
                {
                    StartCoroutine(ParpadeoPrimeraMutacion());
                    return true;
                }
                return false;

            case TipoTriggerAct2.SilenciarGolpesSotano:
                if (estadoActual == Act2State.Sotano && !golpesSilenciados)
                {
                    SilenciarGolpes();
                    return true;
                }
                return false;

            case TipoTriggerAct2.FrenoEscaleraSotano:
                if (estadoActual == Act2State.Cierre && puertaSotanoAbierta && !lucasSeFreno)
                {
                    StartCoroutine(LucasSeFrena());
                    return true;
                }
                return false;
        }
        return false;
    }

    bool ExisteTrigger(TipoTriggerAct2 tipo)
    {
        foreach (TriggerZonaAct2 t in FindObjectsByType<TriggerZonaAct2>(FindObjectsSortMode.None))
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

        IniciarServicioNuevo();
    }

    // ═════════════════════════════════════════════════════════════
    //  2. SERVICIO
    // ═════════════════════════════════════════════════════════════
    void IniciarServicioNuevo()
    {
        estadoActual = Act2State.Servicio;
        indiceClienteNormal = 0;
        foreach (ClienteAct2 c in clientesNormalesEnOrden) if (c != null) c.esSuTurno = false;
        if (clienteCorrupto != null) clienteCorrupto.esSuTurno = false;
        HabilitarSiguienteCliente();
    }

    void HabilitarSiguienteCliente()
    {
        // Saltear vacíos o ya atendidos
        while (indiceClienteNormal < clientesNormalesEnOrden.Length &&
               (clientesNormalesEnOrden[indiceClienteNormal] == null ||
                clientesNormalesEnOrden[indiceClienteNormal].estado == ClienteAct2.Estado.Atendido))
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
            Debug.LogError("[Act2Manager] No hay ClienteCorrupto asignado: el servicio no puede terminar.");
        }
    }

    public void ClienteNormalAtendido(ClienteAct2 cliente)
    {
        indiceClienteNormal++;
        HabilitarSiguienteCliente();
    }

    public void IniciarSecuenciaClienteCorrupto(ClienteCorrupto cliente)
    {
        StartCoroutine(SecuenciaClienteCorrupto(cliente));
    }

    IEnumerator SecuenciaClienteCorrupto(ClienteCorrupto cliente)
    {
        BloquearJugador(true);   // "se bloquea el movimiento de Lucas"
        Transform jugador = jugadorCtrl != null ? jugadorCtrl.transform : null;

        yield return Decir(dialogoLucasPedido);

        // GLITCH: el cliente gira la cabeza, todos los clientes se voltean, estática, respiración, sacudida
        SumarParanoia(15f);
        List<MirarJugadorAct2> miradores = new List<MirarJugadorAct2>();
        foreach (MirarJugadorAct2 m in FindObjectsByType<MirarJugadorAct2>(FindObjectsSortMode.None))
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
        foreach (MirarJugadorAct2 m in miradores) if (m != null) m.VolverAlInstante();

        yield return new WaitForSeconds(0.4f);
        if (cliente != null) cliente.ReproducirVozDistorsionada();
        yield return Decir(dialogoClienteCorrupto);

        BloquearJugador(false);
        if (cliente != null) cliente.esSuTurno = false;
        IrACocina();
    }

    // ═════════════════════════════════════════════════════════════
    //  3. PASILLO → ZAPATOS (lo llama ZapatosEncontrados() en Act2Manager.cs)
    // ═════════════════════════════════════════════════════════════
    IEnumerator SecuenciaZapatosNueva()
    {
        estadoActual = Act2State.Sotano;

        if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(false);
        if (pasilloEfecto != null) pasilloEfecto.DesactivarEfecto();

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
        if (sonidoGolpesSotano != null) sonidoGolpesSotano.Play();
        if (puertaSotanoL != null) puertaSotanoL.ActivarGolpes();

        // Movimiento de cámara indicando la dirección de la puerta
        yield return GirarJugadorHacia(puntoPuertaSotano, duracionGiroHaciaPuerta);
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
    //  4/5. NOTA → REGRESO AL BAÑO (lo llama NotaLeida() en Act2Manager.cs)
    // ═════════════════════════════════════════════════════════════
    IEnumerator SecuenciaNotaLeidaNueva()
    {
        estadoActual = Act2State.Bano;
        if (!golpesSilenciados) SilenciarGolpes();
        SumarParanoia(10f);

        yield return new WaitForSeconds(1f);

        // El salón: completamente desordenado y todo tirado; luces rojas, todo tenso.
        PantallaNegra(true);
        yield return new WaitForSeconds(0.15f);
        parpadeandoLuces = false;
        if (barOrdenado != null) barOrdenado.SetActive(false);
        if (barDesordenado != null) barDesordenado.SetActive(true);
        CambiarIluminacion("Psicosis");
        PantallaNegra(false);

        if (parpadearLucesRojas)
        {
            parpadeandoLuces2 = true;
            StartCoroutine(ParpadeoLucesPsicosis());
        }

        // El tercer cubículo sigue cerrado, pero ahora la puerta cede y se abre lentamente.
        // "Los dos cubículos están abiertos": los de al lado se abren y quedan trabados.
        AbrirPuertasDelBano();

        if (llaveObjeto != null) llaveObjeto.gameObject.SetActive(true);
        ActualizarObjetivo("Buscá la llave en el baño de hombres");
    }

    // ═════════════════════════════════════════════════════════════
    //  5. LLAVE (la llama LlaveInteractuable)
    // ═════════════════════════════════════════════════════════════
    public void LlaveTomadaEnCubiculo()
    {
        llaveTenida = true;
        estadoActual = Act2State.Vigilante;
        aparicionesVigilanteHechas = 0;
        SumarParanoia(15f);

        // "Al agarrarla escuchamos ruidos de las puertas de los cubículos de al lado cerrándose de golpe."
        if (sonidoPortazosCubiculos != null) sonidoPortazosCubiculos.Play();
        if (controlesQueSeCierranDeGolpe != null)
            foreach (ControlPuertaFisicaAct2 c in controlesQueSeCierranDeGolpe) if (c != null) c.CerrarDeGolpe();

        // "Escuchamos de lejos ruidos de Sombras en el salón acercándose."
        if (ruidoSombrasLejos != null) StartCoroutine(FadeInAudio(ruidoSombrasLejos, 15f, 0.7f));

        ActualizarObjetivo("Salí del baño");

        if (FindObjectsByType<AparicionVigilanteAct2>(FindObjectsSortMode.None).Length == 0)
        {
            Debug.LogWarning("[Act2Manager] No hay AparicionVigilanteAct2 en la escena: el combate empieza solo en 3 s.");
            StartCoroutine(CombateConDelay(3f));
        }
    }

    void AbrirPuertasDelBano()
    {
        if (controlFueraDeServicio != null) controlFueraDeServicio.Habilitar();
        else Debug.LogWarning("[Act2Manager] Falta 'controlFueraDeServicio' (ControlPuertaFisicaAct2 del cubículo fuera de servicio).");

        if (controlesQueSeCierranDeGolpe != null)
            foreach (ControlPuertaFisicaAct2 c in controlesQueSeCierranDeGolpe) if (c != null) c.AbrirYTrabar();
    }

    IEnumerator CombateConDelay(float s)
    {
        yield return new WaitForSeconds(s);
        IniciarCombateFinal();
    }

    // ═════════════════════════════════════════════════════════════
    //  6. VIGILANTE (lo llama AparicionVigilanteAct2)
    // ═════════════════════════════════════════════════════════════
    public bool PuedeAparecerVigilante(int orden)
    {
        return estadoActual == Act2State.Vigilante && orden == aparicionesVigilanteHechas;
    }

    public void AparicionVigilanteTerminada(int orden, bool esLaUltima)
    {
        aparicionesVigilanteHechas = orden + 1;
        if (esLaUltima) IniciarCombateFinal();
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
        else Debug.LogError("[Act2Manager] Falta 'sombrasCombate': no van a aparecer sombras.");

        // Durante el combate el jugador pasa por una mesa: hay una niña sentada
        if (figuraNino != null) figuraNino.Aparecer();

        ActualizarObjetivo("Atravesá el salón y llegá a la puerta del sótano");

        if (segundosMaxCombateSinPilas > 0f)
            rutinaSinPilasPorTiempo = StartCoroutine(SinPilasPorTiempo());
    }

    /// <summary>Lo llama FiguraNino cuando la niña desaparece.</summary>
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

        if (linternaSinPilas == null) linternaSinPilas = FindAnyObjectByType<LinternaSinPilasAct2>();
        if (linternaSinPilas != null) linternaSinPilas.AgotarPilas();
        else Debug.LogWarning("[Act2Manager] No hay LinternaSinPilasAct2: la linterna no se va a quedar sin pilas.");

        ActualizarObjetivo("¡Corré hasta la puerta del sótano!");
    }

    // ═════════════════════════════════════════════════════════════
    //  7. REGRESO A LA PUERTA DEL SÓTANO / CIERRE (lo llama UsarLlave() en Act2Manager.cs)
    // ═════════════════════════════════════════════════════════════
    IEnumerator SecuenciaAperturaSotano()
    {
        estadoActual = Act2State.Cierre;
        llaveTenida = false;

        // Lucas introduce la llave y la puerta se abre
        if (sonidoAbrirCandado != null) sonidoAbrirCandado.Play();
        yield return new WaitForSeconds(0.6f);

        // En ese instante todo se detiene y "vuelve a la normalidad"
        if (rutinaSinPilasPorTiempo != null) StopCoroutine(rutinaSinPilasPorTiempo);
        parpadeandoLuces = false;
        parpadeandoLuces2 = false;

        if (sombrasCombate != null) sombrasCombate.DesactivarTodo();     // Las Sombras desaparecen
        if (efectoPsicosis != null) efectoPsicosis.DesactivarPsicosis(); // La distorsión desaparece
        if (sonidoEstatica != null) sonidoEstatica.Stop();               // La estática desaparece

        AudioSource[] callar = { ambientBar, musicBar, musicaSuspenso, audioBasement, sonidoGolpesSotano, ruidoSombrasLejos };
        foreach (AudioSource a in callar) if (a != null) a.Stop();
        if (audiosASilenciar != null) foreach (AudioSource a in audiosASilenciar) if (a != null) a.Stop();

        if (figuraNino != null)
        {
            if (figuraNino.modeloNino != null) figuraNino.modeloNino.SetActive(false);
            if (figuraNino.sombrasAlrededor != null) figuraNino.sombrasAlrededor.SetActive(false);
            if (figuraNino.dibujoQueQueda != null) figuraNino.dibujoQueQueda.SetActive(false);
            if (figuraNino.vozNino != null) figuraNino.vozNino.Stop();
        }

        CambiarIluminacion("Normal");                                   // Las luces vuelven
        if (barDesordenado != null) barDesordenado.SetActive(false);    // El bar está impecable
        if (barOrdenado != null) barOrdenado.SetActive(true);
        if (ParanoiaSystem.Instance != null) ParanoiaSystem.Instance.ResetParanoia();
        if (textoObjetivo != null) textoObjetivo.text = "";             // Silencio absoluto

        // La puerta está abierta: unos pocos escalones y oscuridad
        if (puertaSotanoL != null) puertaSotanoL.AbrirSola();
        if (puertaSotanoR != null) puertaSotanoR.AbrirSola();
        puertaSotanoAbierta = true;

        // Si no hay trigger en la escalera, Lucas se frena solo
        if (!ExisteTrigger(TipoTriggerAct2.FrenoEscaleraSotano))
        {
            Debug.LogWarning("[Act2Manager] No hay TriggerZonaAct2 'FrenoEscaleraSotano': Lucas se frena solo en 4 s.");
            yield return new WaitForSeconds(4f);
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

        // Se escucha el sonido de una niña jugando. Una pequeña risa.
        if (risaNina != null) risaNina.Play();

        Debug.Log("[Act2Manager] Noche 2 finalizada.");
        yield return new WaitForSeconds(segundosEnNegroAntesDeCambiar);
        if (!string.IsNullOrEmpty(escenaSiguiente)) SceneManager.LoadScene(escenaSiguiente);
    }

    // ═════════════════════════════════════════════════════════════
    //  UTILIDADES NUEVAS
    // ═════════════════════════════════════════════════════════════
    /// <summary>Sube (o baja, con negativo) la paranoia de forma segura.</summary>
    public void SumarParanoia(float valor)
    {
        if (valor != 0f) Paranoia(valor);
    }

    /// <summary>Bloquea / desbloquea el movimiento y la cámara del jugador (PlayerController).</summary>
    public void BloquearJugador(bool bloquear)
    {
        if (jugadorCtrl == null) jugadorCtrl = FindAnyObjectByType<PlayerController>();
        if (jugadorCtrl != null) jugadorCtrl.controlesBloqueados = bloquear;
        else if (bloquear) Debug.LogWarning("[Act2Manager] No hay PlayerController en la escena: no se puede bloquear al jugador.");
    }

    /// <summary>Muestra un diálogo y espera a que se termine de escribir + un rato de lectura.</summary>
    IEnumerator Decir(string frase)
    {
        if (string.IsNullOrEmpty(frase)) yield break;
        MostrarDialogo(frase);
        yield return new WaitForSeconds(frase.Length * Mathf.Max(0.005f, velocidadEscritura) + tiempoLecturaDialogo);
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

    // ═════════════════════════════════════════════════════════════
    //  DEBUG — saltar a cualquier fase (Inspector "Iniciar En Fase", menú contextual o Act2DebugHelper)
    // ═════════════════════════════════════════════════════════════
    public void DebugSaltarA(FaseDebugAct2 fase)
    {
        StopAllCoroutines();
        corrutinaActiva = null;
        if (jugadorCtrl == null) jugadorCtrl = FindAnyObjectByType<PlayerController>();

        if (fadeCanvasGroup != null) { fadeCanvasGroup.alpha = 0f; fadeCanvasGroup.blocksRaycasts = false; }
        if (camaraIntroExterior != null) camaraIntroExterior.SetActive(false);
        if (tituloNoche != null) tituloNoche.alpha = 0f;
        if (notificacionCelularUI != null) notificacionCelularUI.SetActive(false);
        BloquearJugador(false);

        if (fase >= FaseDebugAct2.Servicio)
        {
            tareasTerminadas = true;
            parpadeo1Hecho = true;
            tieneEscoba = tieneTrapo = tieneCepillo = true;
        }

        switch (fase)
        {
            case FaseDebugAct2.Normal:
                StartCoroutine(SecuenciaIntroNueva());
                break;

            case FaseDebugAct2.Tareas:
                CambiarIluminacion("Normal");
                estadoActual = Act2State.Tareas;
                tareaActual = PrimeraTareaPendiente();
                ActualizarObjetivoTareas();
                break;

            case FaseDebugAct2.Servicio:
                CambiarIluminacion("Servicio");
                if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(true);
                if (ambientBar != null && !ambientBar.isPlaying) ambientBar.Play();
                IniciarServicioNuevo();
                break;

            case FaseDebugAct2.ClienteCorrupto:
                CambiarIluminacion("Servicio");
                if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(true);
                estadoActual = Act2State.Servicio;
                foreach (ClienteAct2 c in clientesNormalesEnOrden)
                    if (c != null) { c.estado = ClienteAct2.Estado.Atendido; c.esSuTurno = false; }
                indiceClienteNormal = clientesNormalesEnOrden.Length;
                HabilitarSiguienteCliente();
                break;

            case FaseDebugAct2.Pasillo:
                CambiarIluminacion("Servicio");
                if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(true);
                if (clienteCorrupto != null) clienteCorrupto.esSuTurno = false;
                IrACocina();
                break;

            case FaseDebugAct2.PuertaSotano:
                estadoActual = Act2State.Pasillo;
                ZapatosEncontrados();
                break;

            case FaseDebugAct2.RegresoBano:
                estadoActual = Act2State.Sotano;
                if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(false);
                NotaLeida();
                break;

            case FaseDebugAct2.Vigilante:
            case FaseDebugAct2.Combate:
                if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(false);
                golpesSilenciados = true;
                if (barOrdenado != null) barOrdenado.SetActive(false);
                if (barDesordenado != null) barDesordenado.SetActive(true);
                CambiarIluminacion("Psicosis");
                if (controlFueraDeServicio != null) controlFueraDeServicio.Habilitar();
                if (llaveObjeto != null) llaveObjeto.gameObject.SetActive(false);
                LlaveTomadaEnCubiculo();
                if (fase == FaseDebugAct2.Combate) IniciarCombateFinal();
                break;

            case FaseDebugAct2.Cierre:
                if (grupoClientesCorruptos != null) grupoClientesCorruptos.SetActive(false);
                llaveTenida = true;
                estadoActual = Act2State.Psicosis;
                UsarLlave();
                break;
        }

        Debug.Log($"[Act2Manager] DEBUG: saltando a la fase {fase}.");
    }

    [ContextMenu("▶ NOCHE 2: Saltar a TAREAS")]          void N2_Tareas()       => DebugSaltarA(FaseDebugAct2.Tareas);
    [ContextMenu("▶ NOCHE 2: Saltar a SERVICIO")]        void N2_Servicio()     => DebugSaltarA(FaseDebugAct2.Servicio);
    [ContextMenu("▶ NOCHE 2: Saltar a CLIENTE CORRUPTO")] void N2_Corrupto()    => DebugSaltarA(FaseDebugAct2.ClienteCorrupto);
    [ContextMenu("▶ NOCHE 2: Saltar a PASILLO")]         void N2_Pasillo()      => DebugSaltarA(FaseDebugAct2.Pasillo);
    [ContextMenu("▶ NOCHE 2: Saltar a PUERTA SÓTANO")]   void N2_Puerta()       => DebugSaltarA(FaseDebugAct2.PuertaSotano);
    [ContextMenu("▶ NOCHE 2: Saltar a REGRESO AL BAÑO")] void N2_Bano()         => DebugSaltarA(FaseDebugAct2.RegresoBano);
    [ContextMenu("▶ NOCHE 2: Saltar a VIGILANTE")]       void N2_Vigilante()    => DebugSaltarA(FaseDebugAct2.Vigilante);
    [ContextMenu("▶ NOCHE 2: Saltar a COMBATE")]         void N2_Combate()      => DebugSaltarA(FaseDebugAct2.Combate);
    [ContextMenu("▶ NOCHE 2: Saltar a CIERRE")]          void N2_Cierre()       => DebugSaltarA(FaseDebugAct2.Cierre);

    // ═════════════════════════════════════════════════════════════
    //  VERIFICAR Y AUTO-CONFIGURAR (clic derecho en el componente)
    // ═════════════════════════════════════════════════════════════
    [ContextMenu("⚠ NOCHE 2: Verificar escena (reestructura)")]
    void VerificarEscenaNoche2()
    {
        int errores = 0, avisos = 0;
        void Error(bool falta, string msg) { if (falta) { Debug.LogError("[Noche 2] FALTA: " + msg); errores++; } }
        void Aviso(bool falta, string msg) { if (falta) { Debug.LogWarning("[Noche 2] Opcional: " + msg); avisos++; } }

        Error(textoSubtitulos == null || canvasGroupDialogo == null, "textoSubtitulos y canvasGroupDialogo (sin esto NO se ven los diálogos)");
        Error(textoObjetivo == null, "textoObjetivo (texto de objetivos)");
        Error(fadeCanvasGroup == null, "fadeCanvasGroup (panel negro)");
        Error(lucesNormales == null || lucesServicio == null || lucesPsicosis == null, "lucesNormales / lucesServicio / lucesPsicosis");
        Error(FindAnyObjectByType<PlayerController>() == null, "PlayerController en el jugador");

        var zonas = FindObjectsByType<ZonaLimpiezaAct2>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int b = 0, ba = 0, bn = 0;
        foreach (var z in zonas) { if (z.tarea == TareaAct2.Barrer) b++; else if (z.tarea == TareaAct2.Barra) ba++; else bn++; }
        Error(zonas.Length == 0, "ZonaLimpiezaAct2 (montículos, barra, exhibidor, inodoros)");
        Debug.Log($"[Noche 2] Zonas: barrer={b} (guion: 3), barra/exhibidor={ba}, baños={bn}");
        Aviso(FindObjectsByType<HerramientaLimpiezaAct2>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0, "HerramientaLimpiezaAct2 (escoba/trapo/cepillo) — si no hay, poné 'Herramienta Requerida = Ninguna' en las zonas");
        Aviso(!HayTrigger(TipoTriggerAct2.ParpadeoSalidaBanos), "TriggerZonaAct2 'ParpadeoSalidaBanos' (si falta, el parpadeo pasa solo)");
        Aviso(!HayTrigger(TipoTriggerAct2.SilenciarGolpesSotano), "TriggerZonaAct2 'SilenciarGolpesSotano'");
        Aviso(!HayTrigger(TipoTriggerAct2.FrenoEscaleraSotano), "TriggerZonaAct2 'FrenoEscaleraSotano' (si falta, Lucas se frena solo)");
        Aviso(barDesordenado == null, "barDesordenado (bar destruido del Parpadeo N°1 y del regreso al baño)");

        Error(clienteCorrupto == null && FindAnyObjectByType<ClienteCorrupto>() == null, "ClienteCorrupto");
        Aviso(FindObjectsByType<ClienteAct2>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0, "ClienteAct2 (clientes normales antes del corrupto)");
        Aviso(FindObjectsByType<MirarJugadorAct2>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0, "MirarJugadorAct2 en los clientes (se dan vuelta en el glitch)");
        Error(FindAnyObjectByType<ZapatosNino>(FindObjectsInactive.Include) == null, "ZapatosNino en el depósito");
        Error(pasilloEfecto == null, "pasilloEfecto");
        Error(notaPuerta == null, "notaPuerta");
        Error(puertaSotanoL == null, "puertaSotanoL");
        Aviso(puntoPuertaSotano == null, "puntoPuertaSotano (giro de cámara hacia la puerta)");
        Error(llaveObjeto == null, "llaveObjeto");
        Aviso(controlFueraDeServicio == null, "controlFueraDeServicio (ControlPuertaFisicaAct2 del cubículo fuera de servicio)");
        Aviso(controlesQueSeCierranDeGolpe == null || controlesQueSeCierranDeGolpe.Length == 0, "controlesQueSeCierranDeGolpe (puertas de al lado que se cierran de golpe)");
        var apariciones = FindObjectsByType<AparicionVigilanteAct2>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Aviso(apariciones.Length < 2, $"AparicionVigilanteAct2 (hay {apariciones.Length}, el guion pide 2)");
        bool hayUltima = false; foreach (var a in apariciones) if (a.esLaUltima) hayUltima = true;
        Error(apariciones.Length > 0 && !hayUltima, "marcar 'Es La Ultima' en la última AparicionVigilanteAct2");
        Error(sombrasCombate == null, "sombrasCombate");
        Error(figuraNino == null, "figuraNino (Pilar)");
        Aviso(FindAnyObjectByType<LinternaSinPilasAct2>() == null, "LinternaSinPilasAct2 (la linterna se queda sin pilas)");
        Aviso(!HayTriggerCierre(), "TriggerCierreSotano frente a la puerta del sótano");

        Debug.Log(errores == 0
            ? $"[Noche 2] ✓ Escena lista ({avisos} avisos opcionales)."
            : $"[Noche 2] ✗ {errores} cosas obligatorias sin configurar, {avisos} avisos.");
    }

    bool HayTrigger(TipoTriggerAct2 tipo)
    {
        foreach (var t in FindObjectsByType<TriggerZonaAct2>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.tipo == tipo) return true;
        return false;
    }

    bool HayTriggerCierre() => FindAnyObjectByType<TriggerCierreSotano>(FindObjectsInactive.Include) != null;

    [ContextMenu("Auto-buscar referencias NOCHE 2 (reestructura)")]
    void AutoBuscarReferenciasNoche2()
    {
        AutoBuscarReferencias();   // primero lo de siempre

        if (clienteCorrupto == null)
            foreach (var c in FindObjectsByType<ClienteCorrupto>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (c.hacePedido) { clienteCorrupto = c; break; }

        if (clientesNormalesEnOrden == null || clientesNormalesEnOrden.Length == 0)
        {
            var cl = FindObjectsByType<ClienteAct2>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            System.Array.Sort(cl, (a, b) => string.CompareOrdinal(a.name, b.name));
            clientesNormalesEnOrden = cl;
        }

        if (linternaSinPilas == null) linternaSinPilas = FindAnyObjectByType<LinternaSinPilasAct2>(FindObjectsInactive.Include);

        // Controles de las puertas del baño (por rol)
        var golpe = new List<ControlPuertaFisicaAct2>();
        foreach (var c in FindObjectsByType<ControlPuertaFisicaAct2>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c.rol == RolPuertaAct2.FueraDeServicio) { if (controlFueraDeServicio == null) controlFueraDeServicio = c; }
            else golpe.Add(c);
        }
        if ((controlesQueSeCierranDeGolpe == null || controlesQueSeCierranDeGolpe.Length == 0) && golpe.Count > 0)
            controlesQueSeCierranDeGolpe = golpe.ToArray();

        if (barOrdenado == null) barOrdenado = GameObject.Find("BarOrdenado");
        if (barDesordenado == null) barDesordenado = BuscarIncluyendoInactivos("BarDesordenado");
        if (lucesTension == null) lucesTension = BuscarIncluyendoInactivos("LucesTension");
        if (notificacionCelularUI == null) notificacionCelularUI = BuscarIncluyendoInactivos("NotificacionCelular");
        if (overlayGlitch == null) { var g = BuscarIncluyendoInactivos("OverlayGlitch"); if (g != null) overlayGlitch = g.GetComponent<CanvasGroup>(); }
        if (tituloNoche == null) { var g = BuscarIncluyendoInactivos("TituloNoche"); if (g != null) tituloNoche = g.GetComponent<CanvasGroup>(); }
        if (puntoPuertaSotano == null && puertaSotanoL != null) puntoPuertaSotano = puertaSotanoL.transform;

        Debug.Log("[Act2Manager] Auto-buscar NOCHE 2 terminado. Corré '⚠ NOCHE 2: Verificar escena' para ver qué falta.");
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    static GameObject BuscarIncluyendoInactivos(string nombre)
    {
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name == nombre && t.gameObject.scene.IsValid()) return t.gameObject;
        return null;
    }
}
