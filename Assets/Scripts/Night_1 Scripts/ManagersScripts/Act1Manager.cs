using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class Act1Manager : MonoBehaviour
{
    public enum ActoState { Limpieza, Servicio, Quiebre, AparicionBarra, Combate, Transicion }

    [Header("Estado General")]
    public ActoState estadoActual = ActoState.Limpieza;
    private Coroutine corrutinaActiva;

    [Header("UI y Diálogos")]
    public TextMeshProUGUI textoSubtitulos;
    public CanvasGroup canvasGroupDialogo; 
    public TextMeshProUGUI textoObjetivo;
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

    [Header("Items & Pistas Narrativas")]
    public GameObject dibujoMesa;
    public GameObject jugueteOso;
    public GameObject fotoFamiliar;

    [Header("Efectos")]
    public EffectoParpadeo effectoParpadeo;

    [Header("Sistemas de Iluminación")]
    public GameObject lucesNormales;   
    public GameObject lucesServicio;   
    public GameObject lucesCreepy;     
    public GameObject lucesCombate;    

    [Header("Audio")]
    public AudioSource ambientBar;
    public AudioSource musicBar;
    public AudioSource sonidoCajitaMusica;
    public AudioSource sonidoGritoNena;

    [Header("Progreso de Servicio & Banderas de Clientes")]
    public bool carlosPidioCerveza = false;
    public bool marielaPidioHoney = false;
    public int clientesAtendidosTotal = 0;
    public bool carlosAtendido = false;
    public bool cliente2Atendido = false;
    public bool cliente3Atendido = false;

    [Header("Secuencia Final y Objetos")]
    public GameObject linternaObjeto;
    public GameObject botellaEspecial; 
    public GameObject objetoMujer;
    public GameObject puertaDeposito;

    [Header("Combate")]
    public GameObject[] enemigos;
    public AudioSource sonidoMutacion;
    public CameraShake sacudidaCamara;
    public int enemigosDerrotados = 0;
    public int totalEnemigos = 3;

    [Header("Referencias de Cierre")] 
    public CanvasGroup fadeCanvasGroup; 
    public TextMeshProUGUI interactionText;

    [Header("Indicadores")]
    public GameObject indicadorCervezas;
    public GameObject indicadorDeposito;

    [Header("Items ScriptableObjects")]
    public ItemSO itemCerveza;
    public ItemSO itemWhisky;
    public ItemSO itemVasoVacio;
    public ItemSO itemHoney;
    
    [Header("Servicio de Cerveza")]
    public GameObject vasoServicio;
    public ServicioCervezaVisual servicioCervezaVisual;

    public bool tieneObjetoEnMano = false;
    public bool vasoEnCanilla = false;
    private bool sirviendoCerveza = false;

    public static Act1Manager Instance {get; private set;}

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }   
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Time.timeScale = 1f;
        tieneObjetoEnMano = false;
        clientesAtendidosTotal = 0;

        if (indicadorCervezas != null) indicadorCervezas.SetActive(false);
        if (indicadorDeposito != null) indicadorDeposito.SetActive(false);
        if (botellaEspecial != null) botellaEspecial.SetActive(false);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
            StartCoroutine(SecuenciaInicioNoche());
        }

        CambiarIluminacion("Normal");
        IniciarFaseTareas();
    }

    private IEnumerator SecuenciaInicioNoche()
    {
        // Lógica inicial de la Noche 1 (fades, audios, etc.)
        yield return null;
    }

    public void IniciarFaseTareas()
    {
        estadoActual = ActoState.Limpieza;
        if (lucesNormales != null) lucesNormales.SetActive(true); 
        if (lucesServicio != null) lucesServicio.SetActive(false); 
        if (grupoClientes != null) grupoClientes.SetActive(false); 

        MostrarDialogo("Lucas: Hay que dejar todo listo antes de abrir...");
        ActualizarProgresoObjetivo(); 
    }

    //===========================
    // TAREAS INICIALES
    //===========================
    public void RegistrarZonaBarrida()
    {
        zonasBarridas++;
        VerificarFinTareas();
    }

    public void RegistrarMesasLimpias()
    {
        mesasLimpiadas++;
        VerificarFinTareas();
    }

    public void SillaCompletada()
    {
        sillasAcomodadas++;
        VerificarFinTareas();
    }

    public void InteractuarDibujo()
    {
        MostrarDialogo("Lucas: Un dibujo infantil... sin firma.");
    }

    public void InteractuarOsoJuguete()
    {
        MostrarDialogo("Lucas: ¿Y esto?... Siempre pierde estas cosas; después se lo llevo.");
    }

    public void InteractuarFotoFamiliar()
    {
        MostrarDialogo("Lucas: Mi familia... qué contenta estaba Pili ese día... qué lástima.");
    }

    private void VerificarFinTareas()
    {
        ActualizarProgresoObjetivo();

        if (sillasAcomodadas >= totalSillas && 
            mesasLimpiadas >= totalMesasParaLimpiar &&
            zonasBarridas >= totalZonasParaBarrer &&
            estadoActual == ActoState.Limpieza)
        {
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
        yield return new WaitForSeconds(1f);
        
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

    //===========================
    // SERVICIO Y DIÁLOGOS
    //===========================
    public void RegistrarPedidoMarielaHoney()
    {
        marielaPidioHoney = true;
    }

    public bool TienePedidoEntregable()
    {
        return tieneObjetoEnMano;
    }

    public void ClienteCompletado()
    {
        clientesAtendidosTotal++;
        tieneObjetoEnMano = false;

        if (ControladorMano3D.Instance != null)
        {
            ControladorMano3D.Instance.VaciarMano();
        }

        if (clientesAtendidosTotal >= 3)
        {
            StartCoroutine(SecuenciaQuiebreCajita());
        }
    }

    public void ServirCerveza()
    {
        sirviendoCerveza = true;
        tieneObjetoEnMano = true;
    }

    public void ColocarVasoEnCanilla()
    {
        vasoEnCanilla = true;
    }

   public void RecogerObjeto()
{
    tieneObjetoEnMano = true;
}

public void RecogerObjeto(bool estado)
{
    tieneObjetoEnMano = estado;
}

public void RecogerObjeto(ItemSO item)
{
    tieneObjetoEnMano = true;
}
    public void HabilitarTriggerCocinaFinal()
    {
        // Método de soporte para triggers de retorno al final del nivel
    }

    public void EndNight()
    {
        FinalizarNoche();
    }

    public void InteractuarCarlos()
    {
        if (estadoActual != ActoState.Servicio) return;

        if (!carlosAtendido)
        {
            carlosAtendido = true;
            carlosPidioCerveza = true;
            MostrarDialogo("Carlos: Hola Lucas, ¿todo bien?\nLucas: ¡Hola Carlos! Sí, todo bien... ¿Lo de siempre?\nCarlos: Sí, por favor.");
            if (indicadorCervezas != null) indicadorCervezas.SetActive(true);
        }
        else if (tieneObjetoEnMano)
        {
            EntregarPedidoACliente();
            MostrarDialogo("Carlos: Gracias, maestro.");
            ClienteCompletado();
            if (indicadorCervezas != null) indicadorCervezas.SetActive(false);
        }
        else
        {
            MostrarDialogo("Carlos: Te vas a terminar matando de tanto laburo.");
        }
    }

    public void InteractuarCliente2()
    {
        if (estadoActual != ActoState.Servicio || !carlosAtendido) return;

        if (!cliente2Atendido)
        {
            cliente2Atendido = true;
            MostrarDialogo("Cliente: Nos das una cerveza y un Whisky, por favor.");
        }
        else if (tieneObjetoEnMano)
        {
            EntregarPedidoACliente();
            MostrarDialogo("Cliente: Excelente, gracias.");
            ClienteCompletado();
        }
    }

    public void InteractuarCliente3()
    {
        if (estadoActual != ActoState.Servicio || !cliente2Atendido) return;

        if (!cliente3Atendido)
        {
            cliente3Atendido = true;
            MostrarDialogo("Cliente: Hola, ¿Te pido una cerveza?\nCliente: ¿Mariela ya no viene? Hace rato que no la veo.\nLucas: Está en casa...");
        }
        else if (tieneObjetoEnMano)
        {
            EntregarPedidoACliente();
            ClienteCompletado();
        }
    }

    private void EntregarPedidoACliente()
    {
        tieneObjetoEnMano = false;
        if (ControladorMano3D.Instance != null)
        {
            ControladorMano3D.Instance.VaciarMano();
        }
    }

    //===========================
    // QUIEBRE DE REALIDAD
    //===========================
    IEnumerator SecuenciaQuiebreCajita()
    {
        estadoActual = ActoState.Quiebre;
        yield return new WaitForSeconds(1f);

        if (ambientBar != null) ambientBar.Stop();
        if (musicBar != null) musicBar.Stop();

        if (grupoClientes != null) grupoClientes.SetActive(false);
        CambiarIluminacion("Combate"); 

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

        StartCoroutine(SecuenciaAparicionMariela());
    }

    IEnumerator SecuenciaAparicionMariela()
    {
        yield return new WaitForSeconds(1f);
        CambiarIluminacion("Creepy");

        if (objetoMujer != null) objetoMujer.SetActive(true);

        MostrarDialogo("Mujer: ¿Seguís sirviendo lo mismo de siempre?... Cerveza. Tragos... Excusas. ¿No tenés algo mejor para ofrecer?");
        yield return new WaitForSeconds(5f);

        MostrarDialogo("Lucas: Sí... Tengo algo especial en el depósito. Esperame. Ya vuelvo.");
        ActualizarObjetivo("Busca la botella especial en el depósito");

        if (indicadorDeposito != null) indicadorDeposito.SetActive(true);
        if (botellaEspecial != null) botellaEspecial.SetActive(true);

        estadoActual = ActoState.AparicionBarra;
    }

    public void AlRecogerBotellaEspecial()
    {
        if (estadoActual != ActoState.AparicionBarra) return;

        if (botellaEspecial != null) botellaEspecial.SetActive(false);
        if (indicadorDeposito != null) indicadorDeposito.SetActive(false);

        CambiarIluminacion("Combate"); 
        if (objetoMujer != null) objetoMujer.SetActive(false);

        StartCoroutine(SecuenciaInicioCombate());
    }

    IEnumerator SecuenciaInicioCombate()
    {
        yield return new WaitForSeconds(1.5f);
        MostrarDialogo("Presiona [F] para usar la linterna.");
        
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.F));

        if (sacudidaCamara != null) StartCoroutine(sacudidaCamara.Shake(0.8f, 0.2f));
        if (sonidoMutacion != null) sonidoMutacion.Play();

        MostrarDialogo("Lucas: ¿¡Qué carajos!?");
        yield return new WaitForSeconds(2f);

        estadoActual = ActoState.Combate;
        ActualizarObjetivo("¡SOBREVIVE! Disipa a las sombras con tu linterna");

        if (puertaDeposito != null) puertaDeposito.SetActive(false);

        StartCoroutine(ActivarEnemigosSecuencial());
    }

    IEnumerator ActivarEnemigosSecuencial()
    {
        foreach (GameObject enemigo in enemigos)
        {
            if (enemigo != null)
            {
                enemigo.SetActive(true);
                AudioSource snd = enemigo.GetComponent<AudioSource>();
                if (snd != null) snd.Play();
            }
            yield return new WaitForSeconds(1.5f);
        }
    }

    public void EnemigoEliminado()
    {
        enemigosDerrotados++;

        if (enemigosDerrotados >= totalEnemigos)
        {
            FinalizarNoche();
        }
    }

    void FinalizarNoche()
    {
        StartCoroutine(SecuenciaCierreNoche());
    }

    IEnumerator SecuenciaCierreNoche()
    {
        CambiarIluminacion("Servicio");
        if (ambientBar != null) ambientBar.Play();

        MostrarDialogo("Lucas: ¿Qué fue todo eso...?... Estoy cansado... nada más.");
        yield return new WaitForSeconds(3.5f);

        MostrarDialogo("Lucas: Mejor guardo esto y mañana sigo...");
        ActualizarObjetivo("Guarda el vaso en la barra y retírate");

        yield return new WaitForSeconds(4f);
        MostrarDialogo("Lucas: ¡Qué día!... ¿¡Qué hora es ya!?");

        yield return new WaitForSeconds(3f);
        MostrarDialogo("Lucas: [Mensaje de Mariela]: Después...");

        yield return new WaitForSeconds(3f);

        float duracionFade = 2.5f;
        float tiempoFade = 0;
        while (tiempoFade < duracionFade)
        {
            tiempoFade += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, tiempoFade / duracionFade);
            yield return null;
        }

        SceneManager.LoadScene("Night_2 Scene"); 
    }

    //===========================
    // SISTEMAS DE SOPORTE
    //===========================
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

    public void MostrarDialogo(string mensaje)
    {
        Debug.Log($"[Act1Manager] MostrarDialogo llamado con mensaje: '{mensaje}'");

        if (textoSubtitulos == null)
        {
            Debug.LogError("[Act1Manager] ERROR: 'textosSubtitulos' es NULL en el Inspector!");
            return;
        }

        if (canvasGroupDialogo == null)
        {
            Debug.LogError("[Act1Manager] ERROR: 'canvasGroupDialogo' es NULL en el Inspector!");
            return;
        }

        if (corrutinaActiva != null) 
        {
            StopCoroutine(corrutinaActiva);
        }

        corrutinaActiva = StartCoroutine(SecuenciaDialogo(mensaje));
    }
        IEnumerator SecuenciaDialogo(string frase)
    {
        // 1. Nos aseguramos de que la pantalla negra del script de parpadeo esté APAGADA mientras leemos
        if (effectoParpadeo != null && effectoParpadeo.pantallaNegra != null)
        {
            effectoParpadeo.pantallaNegra.SetActive(false);
        }

        // 2. Activamos el contenedor de UI de diálogo y reseteamos el alpha
        if (canvasGroupDialogo != null)
        {
            if (!canvasGroupDialogo.gameObject.activeSelf)
            {
                canvasGroupDialogo.gameObject.SetActive(true);
            }
            canvasGroupDialogo.alpha = 0f;
        }

        if (textoSubtitulos != null)
        {
            textoSubtitulos.text = "";
        }

        float speedFade = velocidadFade > 0 ? velocidadFade : 2f;
        float speedType = velocidadEscritura > 0 ? velocidadEscritura : 0.03f;

        // 3. Fade In
        while (canvasGroupDialogo != null && canvasGroupDialogo.alpha < 1f)
        {
            canvasGroupDialogo.alpha += Time.deltaTime * speedFade;
            yield return null;
        }      

        // 4. Efecto de tipeo
        if (textoSubtitulos != null)
        {
            foreach (char letra in frase.ToCharArray())
            {
                textoSubtitulos.text += letra;
                yield return new WaitForSeconds(speedType);
            }
        }

        yield return new WaitForSeconds(3f);

        // 5. Fade Out
        while (canvasGroupDialogo != null && canvasGroupDialogo.alpha > 0f)
        {
            canvasGroupDialogo.alpha -= Time.deltaTime * (speedFade / 2f);
            yield return null;
        }

    // 6. Ocultamos el panel de diálogo
        if (canvasGroupDialogo != null)
        {
            canvasGroupDialogo.alpha = 0f;
            canvasGroupDialogo.gameObject.SetActive(false);
        }

        corrutinaActiva = null;
    }
}