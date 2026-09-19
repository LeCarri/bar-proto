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
    public bool cliente2Atendido = false;
    public bool cliente3Atendido = false;
    public bool carlosPidioCerveza = false;
    public bool marielaPidioHoney = false;

    [Header("Evento Celular")]
    public GameObject notificacionCelularUI;
    public AudioSource sonidoVibracionCelular;

    [Header("Quiebre & Transición Cliente 4")]
    public GameObject cajitaDeMusicaObjeto;
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
        if (cajitaDeMusicaObjeto != null) cajitaDeMusicaObjeto.SetActive(false);
        if (textoInstruccionF != null) textoInstruccionF.gameObject.SetActive(false);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
        }

        CambiarIluminacion("Normal");
        IniciarFaseTareas();
    }

    // ==========================================
    // MÉTODOS DE COMPATIBILIDAD (Soportan otros scripts)
    // ==========================================
    public void RegistrarPedidoMarielaHoney() { marielaPidioHoney = true; }
    public bool TienePedidoEntregable() { return tieneObjetoEnMano; }
    public void ClienteCompletado() { clientesAtendidosTotal++; }
    public void HabilitarTriggerCocinaFinal() { /* Método puente para vacíos */ }

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

        if (sillasAcomodadas >= totalSillas && 
            mesasLimpiadas >= totalMesasParaLimpiar &&
            zonasBarridas >= totalZonasParaBarrer &&
            estadoActual == ActoState.Limpieza)
        {
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
        yield return new WaitForSeconds(2f);

        if (sonidoGolpeSuelo != null) sonidoGolpeSuelo.Play();
        yield return new WaitForSeconds(1.5f);
        MostrarDialogo("Lucas: Ufff... Estas cañerías están cada vez peor...");
        yield return new WaitForSeconds(2.5f);

        if (effectoParpadeo != null)
        {
            effectoParpadeo.IniciarParpadeo();
            yield return new WaitForSeconds(1f);
        }

        CambiarIluminacion("Servicio");
        if (grupoClientes != null) grupoClientes.SetActive(true);

        estadoActual = ActoState.Servicio;
        MostrarDialogo("Lucas: ¿Clientes?... ¡Muy bien, a trabajar!");
        ActualizarObjetivo("Atiende a los clientes en el salón");

        if (ambientBar != null && !ambientBar.isPlaying) ambientBar.Play();
        if (musicBar != null && !musicBar.isPlaying) musicBar.Play();
    }

    // ==========================================
    // 2. SERVICIO DE CLIENTES
    // ==========================================
    public void InteractuarCarlos()
{
    if (estadoActual != ActoState.Servicio) return;

    if (!carlosAtendido && !tieneObjetoEnMano)
    {
        carlosPidioCerveza = true;
        MostrarDialogo("Cliente: Hola Lucas, ¿todo bien?\nLucas: ¡Hola Carlos! Sí, todo bien... ¿Lo de siempre?\nCliente: Sí, por favor.");
        if (indicadorCervezas != null) indicadorCervezas.SetActive(true);
    }
    else if (!carlosAtendido && tieneObjetoEnMano)
    {
        // DEBUG: Para ver en Consola qué ítem tenés y cuál requiere
        ItemSO itemEnMano = ControladorMano3D.Instance != null ? ControladorMano3D.Instance.ObtenerItemActual() : null;
        Debug.Log($"Item en mano: {(itemEnMano != null ? itemEnMano.nombreItem : "null")} | Esperado: {(itemCerveza != null ? itemCerveza.nombreItem : "null")}");

        if (TienePedidoValido(itemCerveza))
        {
            carlosAtendido = true;
            EntregarPedido();
            MostrarDialogo("Cliente: Gracias, maestro.");
            if (indicadorCervezas != null) indicadorCervezas.SetActive(false);
            clientesAtendidosTotal++;
        }
        else
        {
            Debug.LogWarning("Carlos no acepta el pedido porque el ítem en mano no coincide con 'itemCerveza'.");
        }
    }
    else if (carlosAtendido)
    {
        MostrarDialogo("Cliente: ¿Mucho laburo?\nLucas: Lo de siempre, la verdad.\nCliente: ¡Te vas a terminar matando!\nLucas: Y... no me queda otra.");
    }
}

    public void InteractuarCliente2()
    {
        if (estadoActual != ActoState.Servicio || !carlosAtendido) return;

        if (!cliente2Atendido && !tieneObjetoEnMano)
        {
            MostrarDialogo("Cliente: Nos das una cerveza y un Whisky, por favor.");
            DispararVibracionCelular();
        }
        else if (!cliente2Atendido && tieneObjetoEnMano && (TienePedidoValido(itemCerveza) || TienePedidoValido(itemWhisky)))
        {
            cliente2Atendido = true;
            EntregarPedido();
            MostrarDialogo("Cliente: Excelente, gracias.");
            clientesAtendidosTotal++;
        }
    }

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

    public void InteractuarCliente3()
    {
        if (estadoActual != ActoState.Servicio || !cliente2Atendido) return;

        if (!cliente3Atendido && !tieneObjetoEnMano)
        {
            MostrarDialogo("Cliente: Hola, ¿Te pido una cerveza?");
            if (indicadorCervezas != null) indicadorCervezas.SetActive(true);
        }
        else if (!cliente3Atendido && tieneObjetoEnMano && TienePedidoValido(itemCerveza))
        {
            cliente3Atendido = true;
            EntregarPedido();
            MostrarDialogo("Cliente: ¿Mariela ya no viene? Hace rato que no la veo.\nLucas: Está en casa...\nCliente: Ah... Pensé que...\nLucas: ¿Qué...?\nCliente: Nada, nada...");
            if (indicadorCervezas != null) indicadorCervezas.SetActive(false);
            clientesAtendidosTotal++;
        }
    }

    public void InteractuarCliente4()
    {
        if (estadoActual != ActoState.Servicio || !cliente3Atendido) return;

        if (!tieneObjetoEnMano)
        {
            MostrarDialogo("Cliente: Dame un Whisky Jack Daniels.");
        }
        else if (tieneObjetoEnMano && TienePedidoValido(itemWhiskySangre))
        {
            EntregarPedido();
            StartCoroutine(SecuenciaQuiebreCajita());
        }
    }

    public void ServirWhiskyEspecial()
    {
        tieneObjetoEnMano = true;
        if (ControladorMano3D.Instance != null && itemWhiskySangre != null)
        {
            ControladorMano3D.Instance.EquiparItem(itemWhiskySangre);
        }
    }

    // ==========================================
    // 3. QUIEBRE Y APARICIÓN DE MARIELA
    // ==========================================
    IEnumerator SecuenciaQuiebreCajita()
    {
        estadoActual = ActoState.Quiebre;

        if (ambientBar != null) ambientBar.Stop();
        if (musicBar != null) musicBar.Stop();

        if (grupoClientes != null) grupoClientes.SetActive(false);
        CambiarIluminacion("Combate"); 

        if (cajitaDeMusicaObjeto != null) cajitaDeMusicaObjeto.SetActive(true);

        if (sonidoCajitaMusica != null) sonidoCajitaMusica.Play();
        yield return new WaitForSeconds(4f);

        if (sonidoGritoNena != null) sonidoGritoNena.Play();
        yield return new WaitForSeconds(1.5f);

        if (sonidoCajitaMusica != null) sonidoCajitaMusica.Stop();

        MostrarDialogo("Lucas: ¿Justo ahora?... Menos mal que tengo la linterna acá en la barra.");
        ActualizarObjetivo("Busca la linterna detrás de la barra");
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
    private bool TienePedidoValido(ItemSO itemRequerido)
    {
        if (ControladorMano3D.Instance == null || itemRequerido == null) return false;
        ItemSO actual = ControladorMano3D.Instance.ObtenerItemActual();
        return actual == itemRequerido || (actual != null && actual.nombreItem.Equals(itemRequerido.nombreItem, System.StringComparison.OrdinalIgnoreCase));
    }

    private void EntregarPedido()
    {
        tieneObjetoEnMano = false;
        if (ControladorMano3D.Instance != null) ControladorMano3D.Instance.VaciarMano();
    }

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
    // SISTEMA DE DIÁLOGOS (REVISADO)
    // ==========================================
    public void MostrarDialogo(string mensaje)
    {
        if (textoSubtitulos == null || canvasGroupDialogo == null)
        {
            Debug.LogWarning("[Act1Manager] Falta asignar textoSubtitulos o canvasGroupDialogo en el Inspector.");
            return;
        }

        // Si hay un diálogo escribiéndose, lo cancelamos para empezar el nuevo
        if (corrutinaActiva != null)
        {
            StopCoroutine(corrutinaActiva);
        }

        corrutinaActiva = StartCoroutine(SecuenciaDialogo(mensaje));
    }

    IEnumerator SecuenciaDialogo(string frase)
    {
        // 1. Activar el Canvas y resetear visibilidad
        canvasGroupDialogo.gameObject.SetActive(true);
        canvasGroupDialogo.alpha = 1f; // Forzamos visibilidad inmediata para evitar fallas de fade
        textoSubtitulos.text = "";

        float speedType = velocidadEscritura > 0 ? velocidadEscritura : 0.03f;

        // 2. Efecto máquina de escribir
        foreach (char letra in frase.ToCharArray())
        {
            textoSubtitulos.text += letra;
            yield return new WaitForSeconds(speedType);
        }

        // 3. Tiempo de lectura visible en pantalla
        yield return new WaitForSeconds(3.5f);

        // 4. Desvanecimiento suave
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