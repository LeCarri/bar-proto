using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class Act1Manager : MonoBehaviour
{
    public enum ActoState { Introduccion, Limpieza, Servicio, Quiebre, AparicionBarra, Combate, Cierre, Transicion }

    [Header("Estado General")]
    public ActoState estadoActual = ActoState.Limpieza;
    private Coroutine corrutinaActiva;

    [Header("DEBUG / TEST")]
    [SerializeField] private bool saltarLimpieza = false;

    [Header("UI y Diálogos")]
    public TextMeshProUGUI textoSubtitulos;
    public CanvasGroup canvasGroupDialogo; 
    public TextMeshProUGUI textoObjetivo;
    public TextMeshProUGUI textoInstruccionF; 
    public float velocidadEscritura = 0.04f;
    public float velocidadFade = 3f;

    [Header("Referencias de Escena")]
    public GameObject grupoClientes; 
    public AudioSource sonidoGolpeSuelo; 

    [Header("Progreso de Tareas")]
    public int totalSillas = 4;
    private int sillasAcomodadas = 0;
    public int totalMesasParaLimpiar = 2;
    private int mesasLimpiadas = 0;
    public int totalZonasParaBarrer = 2;
    private int zonasBarridas = 0;

    [Header("Herramientas de Limpieza")]
    public bool tieneEscoba = false;
    public bool tieneTrapo = false;

    [Header("Items & Pistas Narrativas")]
    public GameObject dibujoMesa;
    public GameObject jugueteOso;
    public GameObject fotoFamiliar;
    private bool tieneDibujoGuardado = false;

    [Header("Efectos")]
    public EffectoParpadeo effectoParpadeo;

    [Header("Sistemas de Iluminación")]
    public GameObject lucesNormales;   
    public GameObject lucesServicio;   
    public GameObject lucesCreepy;     
    public GameObject lucesCombate;    

    [Header("Audio General")]
    public AudioSource ambientBar;
    public AudioSource musicBar;
    public AudioSource sonidoCajitaMusica;
    public AudioSource sonidoGritoNena;

    [Header("Progreso de Servicio & Clientes")]
    public int clientesAtendidosTotal = 0;
    public bool carlosAtendido = false;
    public bool carlosPidioCerveza = false;

    [Header("Control de Turnos de Clientes")]
    [Tooltip("Arrastrá los GameObjects de los clientes en orden: Index 0 = Carlos, Index 1 = Cliente 2, etc.")]
    public ClienteInteractuable[] clientesEnOrden;
    private int indiceClienteActual = 0;

    [Header("Evento Celular")]
    public GameObject notificacionCelularUI;
    public AudioSource sonidoVibracionCelular;

    [Header("Referencias del Quiebre / Caja Musical")]
    public GameObject cajaMusicalInteractuable; 
    public GameObject vasoWhiskyServido;       
    public GameObject grupoClientesBar;         
    public GameObject lucesNormalesBar;
    public GameObject lucesEmergenciaBar;

    [Header("Audio e Impacto del Quiebre")]
    public AudioSource sonidoCajaMusical;
    public AudioSource sonidoGritoNina;
    public AudioSource sonidoExplosionElectrica; 
    public AudioSource sonidoCorteDeLuz;

    [Header("PostQuiebre")]
    public GameObject linternaObjeto;
    public GameObject botellaEspecial; 
    public GameObject objetoMujer;
    public GameObject puertaDeposito;
    public AudioSource sonidoCierrePuerta;

    [Header("Combate")]
    public GameObject primeraSombra;
    public GameObject[] sombrasSalon;
    public AudioSource sonidoMutacion;
    public CameraShake sacudidaCamara;
    public int enemigosDerrotados = 0;
    public int totalEnemigos = 4;

    [Header("Cierre de Noche")]
    public GameObject vasoHoneySobreMesa;
    public bool vasoRecogidoCierre = false;
    public bool vasoDejadoEnBarra = false;
    public CanvasGroup fadeCanvasGroup; 

    [Header("Indicadores & Scriptables")]
    public GameObject indicadorCervezas;
    public GameObject indicadorDeposito;
    public ItemSO itemCerveza;
    public ItemSO itemWhisky;
    public ItemSO itemWhiskySangre; 
    public ItemSO itemVasoVacio;
    public ItemSO itemVasoHoney;
    
    [Header("Estado del Jugador")]
    public bool tieneObjetoEnMano = false;

    public static Act1Manager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f;
        tieneObjetoEnMano = false;
        clientesAtendidosTotal = 0;

        if (indicadorCervezas != null) indicadorCervezas.SetActive(false);
        if (indicadorDeposito != null) indicadorDeposito.SetActive(false);
        if (botellaEspecial != null) botellaEspecial.SetActive(false);
        if (notificacionCelularUI != null) notificacionCelularUI.SetActive(false);
        if (cajaMusicalInteractuable != null) cajaMusicalInteractuable.SetActive(false);
        if (textoInstruccionF != null) textoInstruccionF.gameObject.SetActive(false);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
        }

        if (saltarLimpieza)
{
            IniciarServicioDirecto();
        }
        else
        {
            CambiarIluminacion("Normal");
            IniciarFaseTareas();
}
    }

    private void IniciarServicioDirecto()
    {
        Debug.Log("[DEBUG] Saltando limpieza. Iniciando servicio directamente.");

        CambiarIluminacion("Servicio");

        if (grupoClientes != null)
            grupoClientes.SetActive(true);

        IniciarServicioClientes();

        ActualizarObjetivo("Atiende a los clientes en el salón");

        if (ambientBar != null && !ambientBar.isPlaying)
            ambientBar.Play();

        if (musicBar != null && !musicBar.isPlaying)
            musicBar.Play();
    }

    // ==========================================
    // MÉTODOS AUXILIARES Y COMPATIBILIDAD
    // ==========================================
    public bool TienePedidoEntregable() { return tieneObjetoEnMano; }
    public void ClienteCompletado() { clientesAtendidosTotal++; }

    // ==========================================
    // 1. TAREAS INICIALES
    // ==========================================
    public void IniciarFaseTareas()
    {
        estadoActual = ActoState.Limpieza;
        if (lucesNormales != null) lucesNormales.SetActive(true); 
        if (lucesServicio != null) lucesServicio.SetActive(false); 
        if (grupoClientes != null) grupoClientes.SetActive(false); 

        MostrarDialogo("Lucas: Hay que dejar todo listo antes de abrir...");
        ActualizarProgresoObjetivo(); 

        if (fadeCanvasGroup != null) StartCoroutine(FadeInInicial());
    }

    IEnumerator FadeInInicial()
    {
        float t = 0;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, t / 1.5f);
            yield return null;
        }
        fadeCanvasGroup.gameObject.SetActive(false);
    }

    public void RegistrarZonaBarrida() { zonasBarridas++; VerificarFinTareas(); }
    public void RegistrarMesasLimpias() { mesasLimpiadas++; VerificarFinTareas(); }
    public void SillaCompletada() { sillasAcomodadas++; VerificarFinTareas(); }

    public void InteractuarDibujo()
    {
        tieneDibujoGuardado = true;
        if (dibujoMesa != null) dibujoMesa.SetActive(false);
        MostrarDialogo("Lucas: Un dibujo infantil... sin firma.");
    }

    public void InteractuarOsoJuguete()
    {
        MostrarDialogo("Lucas: ¿Y esto?... Siempre pierde estas cosas; después se lo llevo.");
    }

    public void InteractuarFotoFamiliar()
    {
        MostrarDialogo("Lucas: Qué contenta estaba Pili ese día...");
    }

    private void VerificarFinTareas()
    {
        ActualizarProgresoObjetivo();

        Debug.Log($"[Progreso Tareas] Sillas: {sillasAcomodadas}/{totalSillas} | Mesas: {mesasLimpiadas}/{totalMesasParaLimpiar} | Zonas: {zonasBarridas}/{totalZonasParaBarrer}");

        if (sillasAcomodadas >= totalSillas && 
            mesasLimpiadas >= totalMesasParaLimpiar &&
            zonasBarridas >= totalZonasParaBarrer &&
            estadoActual == ActoState.Limpieza)
        {
            estadoActual = ActoState.Transicion;
            if (!tieneDibujoGuardado && dibujoMesa != null) dibujoMesa.SetActive(false);
            StartCoroutine(SecuenciaTransicionServicio());
        }
    }

    public void ActualizarProgresoObjetivo()
    {
        if (estadoActual == ActoState.Limpieza)
        {
            ActualizarObjetivo($"Prepara el bar: Sillas ({sillasAcomodadas}/{totalSillas}), Mesas ({mesasLimpiadas}/{totalMesasParaLimpiar}), Barrer ({zonasBarridas}/{totalZonasParaBarrer})");
        }
    }

    IEnumerator SecuenciaTransicionServicio()
    {
        MostrarDialogo("Lucas: Listo. Guardo la escoba...");
        yield return new WaitForSeconds(1.5f);

        if (sonidoGolpeSuelo != null) sonidoGolpeSuelo.Play();
        yield return new WaitForSeconds(1f);

        MostrarDialogo("Lucas: Ufff... Estas cañerías están cada vez peor...");

        if (effectoParpadeo != null)
        {
            Debug.Log("[Act1Manager] Ejecutando efecto de parpadeo.");
            effectoParpadeo.IniciarParpadeo();
            yield return new WaitForSeconds(1.2f);
        }
        else
        {
            Debug.LogWarning("[Act1Manager] 'effectoParpadeo' no está asignado en el Inspector.");
        }

        CambiarIluminacion("Servicio");
        if (grupoClientes != null) grupoClientes.SetActive(true);

        IniciarServicioClientes();

        MostrarDialogo("Lucas: ¿Clientes?... ¡Muy bien, a trabajar!");
        ActualizarObjetivo("Atiende a los clientes en el salón");

        if (ambientBar != null && !ambientBar.isPlaying) ambientBar.Play();
        if (musicBar != null && !musicBar.isPlaying) musicBar.Play();
    }

    // ==========================================
    // 2. GESTIÓN DE TURNOS DE CLIENTES
    // ==========================================
    public void IniciarServicioClientes()
    {
        estadoActual = ActoState.Servicio;
        indiceClienteActual = 0;

        for (int i = 0; i < clientesEnOrden.Length; i++)
        {
            if (clientesEnOrden[i] != null)
            {
                clientesEnOrden[i].esSuTurno = false;
            }
        }

        HabilitarClienteActual();
    }

    public void AvanzarSiguienteCliente()
    {
        if (indiceClienteActual < clientesEnOrden.Length && clientesEnOrden[indiceClienteActual] != null)
        {
            clientesEnOrden[indiceClienteActual].esSuTurno = false;
        }

        indiceClienteActual++;

        if (indiceClienteActual < clientesEnOrden.Length)
        {
            HabilitarClienteActual();
        }
        else
        {
            Debug.Log("[Act1Manager] Todos los clientes fueron atendidos.");
        }
    }

    private void HabilitarClienteActual()
    {
        if (indiceClienteActual < clientesEnOrden.Length && clientesEnOrden[indiceClienteActual] != null)
        {
            clientesEnOrden[indiceClienteActual].esSuTurno = true;
            Debug.Log($"[Act1Manager] Turno actual asignado a: {clientesEnOrden[indiceClienteActual].nombreCliente}");
        }
    }

    public ClienteInteractuable ObtenerClienteActual()
    {
        if (indiceClienteActual < clientesEnOrden.Length)
            return clientesEnOrden[indiceClienteActual];
        return null;
    }

    // Eventos Adicionales de Servicio
    public void DispararVibracionCelular()
    {
        if (notificacionCelularUI != null) notificacionCelularUI.SetActive(true);
        if (sonidoVibracionCelular != null) sonidoVibracionCelular.Play();
    }

    public void InteractuarCelular()
    {
        MostrarDialogo("Mariela: ¿Vas a volver tarde?");
        if (notificacionCelularUI != null) notificacionCelularUI.SetActive(false);
    }

    public void DispararVibracionCelularConRetraso(float retrasoSegundos = 2f)
    {
        StartCoroutine(RutinaVibracionCelular(retrasoSegundos));
    }

    private IEnumerator RutinaVibracionCelular(float retraso)
    {
        yield return new WaitForSeconds(retraso);

        if (sonidoVibracionCelular != null) sonidoVibracionCelular.Play();

        if (SistemaCelular.Instance != null)
        {
            SistemaCelular.Instance.RecibirMensaje("Mariela", "¿Vas a volver tarde?");
        }

        MostrarDialogo("Lucas: (Mensaje de Mariela... Presiona [T] para ver)");
    }

    public void MensajeCelularLeido()
    {
        MostrarDialogo("Lucas: Me acuerdo de cuando se preocupaba de verdad...");
        Debug.Log("[Act1Manager] Mensaje leído. Avanzando narrativa.");
    }

    public void ServirWhiskyEspecial()
    {
        tieneObjetoEnMano = true;
        if (ControladorMano3D.Instance != null && itemWhiskySangre != null)
        {
            ControladorMano3D.Instance.EquiparItem(itemWhiskySangre);
        }
    }

    public void HabilitarTriggerCocinaFinal()
    {
        Debug.Log("[Act1Manager] Trigger Cocina Final habilitado.");
    }

    // ==========================================
    // 3. QUIEBRE Y APARICIÓN DE MARIELA
    // ==========================================
    public void IniciarSecuenciaQuiebre()
    {
        Debug.Log("[Act1Manager] INICIANDO SECUENCIA DE QUIEBRE");

        estadoActual = ActoState.Quiebre; // Cambiamos el estado formalmente a Quiebre

        if (grupoClientesBar != null) grupoClientesBar.SetActive(false);
        if (vasoWhiskyServido != null) vasoWhiskyServido.SetActive(true);
        
        // Habilitamos la caja en escena para ser interactuada
        if (cajaMusicalInteractuable != null) 
        {
            cajaMusicalInteractuable.SetActive(true);
            Debug.Log("[Act1Manager] Caja musical activada en la escena.");
        }

        if (lucesNormalesBar != null) lucesNormalesBar.SetActive(true);
        if (lucesEmergenciaBar != null) lucesEmergenciaBar.SetActive(false);

        MostrarDialogo("Lucas: ¿Pará... a dónde se fueron todos?");
        ActualizarObjetivo("Investiga la caja musical sobre la barra");
    }

    // Llamada de interacción directa para la Caja Musical
    public void InteractuarCajaMusical()
    {
        if (estadoActual == ActoState.Quiebre)
        {
            ActivarSecuenciaCajaMusical();
        }
    }

    public void ActivarSecuenciaCajaMusical()
    {
        StartCoroutine(RutinaCajaMusicalYSusto());
    }

    private IEnumerator RutinaCajaMusicalYSusto()
    {
        if (sonidoCajaMusical != null)
        {
            sonidoCajaMusical.pitch = 1.0f;
            sonidoCajaMusical.Play();
        }

        yield return new WaitForSeconds(3.0f);

        float tiempoAcelerando = 0f;
        float duracionAceleracion = 2.5f;

        while (tiempoAcelerando < duracionAceleracion)
        {
            tiempoAcelerando += Time.deltaTime;
            if (sonidoCajaMusical != null)
            {
                sonidoCajaMusical.pitch = Mathf.Lerp(1.0f, 2.2f, tiempoAcelerando / duracionAceleracion);
            }
            yield return null;
        }

        if (sonidoCajaMusical != null) sonidoCajaMusical.Stop();

        float duracionParpadeo = 1.2f;
        float tiempoParpadeo = 0f;

        while (tiempoParpadeo < duracionParpadeo)
        {
            if (lucesNormalesBar != null)
            {
                lucesNormalesBar.SetActive(!lucesNormalesBar.activeSelf);
            }
        
            float intervaloAleatorio = Random.Range(0.04f, 0.12f);
            tiempoParpadeo += intervaloAleatorio;
            yield return new WaitForSeconds(intervaloAleatorio);
        }

        if (sonidoExplosionElectrica != null) sonidoExplosionElectrica.Play();
        if (sonidoGritoNina != null) sonidoGritoNina.Play();
        if (sonidoCorteDeLuz != null) sonidoCorteDeLuz.Play();

        if (lucesNormalesBar != null) lucesNormalesBar.SetActive(false);
        if (lucesEmergenciaBar != null) lucesEmergenciaBar.SetActive(true);

        yield return new WaitForSeconds(1.8f);

        MostrarDialogo("Lucas: ¡Hijos de puta! Se reventó todo... Necesito la linterna ya.");
        HabilitarTriggerCocinaFinal();
    }

    public void RecogerLinterna()
    {
        if (estadoActual != ActoState.Quiebre) return;

        if (linternaObjeto != null) linternaObjeto.SetActive(false);
        MostrarDialogo("Lucas: Seguro saltó la térmica de nuevo... Tengo que revisar los tapones en el sótano...");
    }

    public void TriggerSalidaBarraAparicion()
    {
        if (estadoActual == ActoState.Quiebre)
        {
            StartCoroutine(SecuenciaAparicionMariela());
        }
    }

    IEnumerator SecuenciaAparicionMariela()
    {
        yield return new WaitForSeconds(0.5f);
        CambiarIluminacion("Creepy");

        if (objetoMujer != null) objetoMujer.SetActive(true);

        MostrarDialogo("Mujer: ¿Seguís sirviendo lo mismo de siempre?... Cerveza. Tragos... Excusas.");
        yield return new WaitForSeconds(4f);

        MostrarDialogo("Lucas: Esa voz... ¿Te conozco?");
        yield return new WaitForSeconds(3f);

        MostrarDialogo("Mujer: Quizás el tiempo borró más que los nombres. ¿No tenés algo mejor para ofrecer?");
        yield return new WaitForSeconds(4.5f);

        MostrarDialogo("Lucas: Tengo algo especial en el depósito. Es... de la casa. Ya vuelvo.");
        ActualizarObjetivo("Busca la botella especial en el depósito");

        if (indicadorDeposito != null) indicadorDeposito.SetActive(true);
        if (botellaEspecial != null) botellaEspecial.SetActive(true);

        estadoActual = ActoState.AparicionBarra;
    }

    // ==========================================
    // 4. COMBATE
    // ==========================================
    public void AlRecogerBotellaEspecial()
    {
        if (estadoActual != ActoState.AparicionBarra) return;

        if (botellaEspecial != null) botellaEspecial.SetActive(false);
        if (indicadorDeposito != null) indicadorDeposito.SetActive(false);

        StartCoroutine(SecuenciaOscuridadYLinterna());
    }

    IEnumerator SecuenciaOscuridadYLinterna()
    {
        CambiarIluminacion("Combate"); 
        if (objetoMujer != null) objetoMujer.SetActive(false);

        yield return new WaitForSeconds(1.5f);

        if (textoInstruccionF != null)
        {
            textoInstruccionF.text = "PRESIONA [F] PARA USAR LA LINTERNA";
            textoInstruccionF.gameObject.SetActive(true);
        }

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));

        if (textoInstruccionF != null) textoInstruccionF.gameObject.SetActive(false);

        if (primeraSombra != null) primeraSombra.SetActive(true);
        if (sonidoMutacion != null) sonidoMutacion.Play();
        if (sacudidaCamara != null) StartCoroutine(sacudidaCamara.Shake(0.6f, 0.2f));

        MostrarDialogo("Lucas: ¿¡Qué carajos!?");
    }

    public void PrimeraSombraDerrotada()
    {
        StartCoroutine(SecuenciaTransicionSalonCombate());
    }

    IEnumerator SecuenciaTransicionSalonCombate()
    {
        MostrarDialogo("Lucas: ¿Qué fue eso? ¡No entiendo nada!");
        yield return new WaitForSeconds(2.5f);

        MostrarDialogo("Lucas: No.... no.... no... ¿Qué está pasando?");

        if (puertaDeposito != null) puertaDeposito.SetActive(true); 
        if (sonidoCierrePuerta != null) sonidoCierrePuerta.Play();

        estadoActual = ActoState.Combate;
        ActualizarObjetivo("¡SOBREVIVE! Disipa a las Sombras con tu linterna");

        foreach (GameObject sombra in sombrasSalon)
        {
            if (sombra != null) sombra.SetActive(true);
            yield return new WaitForSeconds(1.2f);
        }
    }

    public void EnemigoEliminado()
    {
        enemigosDerrotados++;
        if (enemigosDerrotados >= totalEnemigos)
        {
            IniciarCierreNoche();
        }
    }

    // ==========================================
    // 5. CIERRE Y SALIDA
    // ==========================================
    void IniciarCierreNoche()
    {
        estadoActual = ActoState.Cierre;
        CambiarIluminacion("Servicio");
        if (ambientBar != null) ambientBar.Play();

        if (vasoHoneySobreMesa != null) vasoHoneySobreMesa.SetActive(true);

        StartCoroutine(SecuenciaPostCombate());
    }

    IEnumerator SecuenciaPostCombate()
    {
        MostrarDialogo("Lucas: ¿Qué fue todo eso...?... Estoy cansado... nada más.");
        yield return new WaitForSeconds(4f);

        MostrarDialogo("Lucas: Mejor guardo esto y mañana sigo...");
        ActualizarObjetivo("Guarda el vaso en la barra y retírate");
    }

    public void InteractuarVasoHoneyCierre()
    {
        if (estadoActual != ActoState.Cierre || vasoRecogidoCierre) return;

        VisitarVasoHoney();
    }

    private void VisitarVasoHoney()
    {
        vasoRecogidoCierre = true;
        if (vasoHoneySobreMesa != null) vasoHoneySobreMesa.SetActive(false);
        if (ControladorMano3D.Instance != null && itemVasoHoney != null)
        {
            ControladorMano3D.Instance.EquiparItem(itemVasoHoney);
        }
        ActualizarObjetivo("Deja el vaso detrás de la barra");
    }

    public void DejaVasoEnBarraCierre()
    {
        if (!vasoRecogidoCierre || vasoDejadoEnBarra) return;

        vasoDejadoEnBarra = true;
        if (ControladorMano3D.Instance != null) ControladorMano3D.Instance.VaciarMano();

        StartCoroutine(SecuenciaFinalPantalla());
    }

    IEnumerator SecuenciaFinalPantalla()
    {
        MostrarDialogo("Lucas: ¡Qué día!... ¿¡Qué hora es ya!?");
        yield return new WaitForSeconds(3f);

        DispararVibracionCelular();
        yield return new WaitForSeconds(1f);

        MostrarDialogo("Notificación: Mariela — 1 mensaje nuevo");
        yield return new WaitForSeconds(3f);

        MostrarDialogo("Lucas: Después.");
        yield return new WaitForSeconds(2.5f);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            float t = 0;
            while (t < 2.5f)
            {
                t += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, t / 2.5f);
                yield return null;
            }
        }

        SceneManager.LoadScene("Night_2 Scene");
    }

    // ==========================================
    // UTILIDADES GENERALES
    // ==========================================
    public void CambiarIluminacion(string estado)
    {
        if (lucesNormales != null) lucesNormales.SetActive(false);
        if (lucesServicio != null) lucesServicio.SetActive(false);
        if (lucesCreepy != null) lucesCreepy.SetActive(false);
        if (lucesCombate != null) lucesCombate.SetActive(false);

        switch (estado)
        {
            case "Normal":
                if (lucesNormales != null) lucesNormales.SetActive(true);
                RenderSettings.ambientLight = new Color(0.22f, 0.22f, 0.22f);
                break;
            case "Servicio":
                if (lucesServicio != null) lucesServicio.SetActive(true);
                RenderSettings.ambientLight = new Color(0.15f, 0.15f, 0.15f);
                break;
            case "Creepy":
                if (lucesCreepy != null) lucesCreepy.SetActive(true);
                RenderSettings.ambientLight = new Color(0.05f, 0.05f, 0.05f);
                break;
            case "Combate":
                if (lucesCombate != null) lucesCombate.SetActive(true);
                RenderSettings.ambientLight = Color.black;
                break;
        }
    }

    public void ActualizarObjetivo(string nuevoObjetivo)
    {
        if (textoObjetivo != null) textoObjetivo.text = "- " + nuevoObjetivo;
    }

    // ==========================================
    // SISTEMA DE DIÁLOGOS
    // ==========================================
    public void MostrarDialogo(string mensaje)
    {
        if (textoSubtitulos == null || canvasGroupDialogo == null) return;

        if (corrutinaActiva != null) StopCoroutine(corrutinaActiva);
        corrutinaActiva = StartCoroutine(SecuenciaDialogo(mensaje));
    }

    IEnumerator SecuenciaDialogo(string frase)
    {
        canvasGroupDialogo.gameObject.SetActive(true);
        canvasGroupDialogo.alpha = 1f;
        textoSubtitulos.text = "";

        float speedType = velocidadEscritura > 0 ? velocidadEscritura : 0.03f;

        foreach (char letra in frase.ToCharArray())
        {
            textoSubtitulos.text += letra;
            yield return new WaitForSeconds(speedType);
        }

        yield return new WaitForSeconds(3.5f);

        float speedFade = velocidadFade > 0 ? velocidadFade : 2f;
        while (canvasGroupDialogo != null && canvasGroupDialogo.alpha > 0f)
        {
            canvasGroupDialogo.alpha -= Time.deltaTime * speedFade;
            yield return null;
        }

        if (canvasGroupDialogo != null)
        {
            canvasGroupDialogo.alpha = 0f;
            canvasGroupDialogo.gameObject.SetActive(false);
        }

        corrutinaActiva = null;
    }
}