using UnityEngine;
using TMPro;
using System.Collections;
using JetBrains.Annotations;
using Unity.VisualScripting; // Necesario para usar Corrutinas (IEnumerator)
using UnityEngine.SceneManagement;
using UnityEngine.Categorization;

public class Act1Manager : MonoBehaviour
{
    public enum ActoState { Limpieza, Servicio, AparicionBarra, ElQuiebre }
    public ActoState estadoActual = ActoState.Limpieza;
    private Coroutine corrutinaActiva;

   [Header("UI y Diálogos")]
    public TextMeshProUGUI textoSubtitulos;
    public CanvasGroup canvasGroupDialogo; // Asegúrate de que el fondo tenga este componente
    public float velocidadEscritura = 0.04f;
    public float velocidadFade = 3f; // Nueva variable para controlar la suavidad

    [Header("Referencias de Escena")]
    public GameObject grupoClientes; 
    public AudioSource sonidoGolpeSuelo; 

    [Header("Configuración")]
    public int totalSillas = 3; 
    private int sillasAcomodadas = 0;

    [Header("Efectos")]

    public EffectoParpadeo effectoParpadeo; //arrastra el objeto con el sccript 

    [Header("Sistemas de Iluminación")]
    public GameObject lucesNormales;   // Amarillas (Bar vacío / Limpieza)
    public GameObject lucesServicio;   // Violetas (Modo servicio general)
    public GameObject lucesCreepy;     // NUEVO: Foco tétrico/parpadeo para la mujer en la barra
    public GameObject lucesCombate;    // Apagón total del combate (Boca de lobo)

    [Header("Audio")]
    public AudioSource ambientBar;
    public AudioSource musicBar;

    [Header("Progreso de servicio")]
    public int ClientesParaAtender = 2;
    private int ClientesAtendidos = 0;
    public GameObject puertaSotano; //referencia a la puerta para bloquearla

    [Header("Lógica de Pedidos")]
    public bool tieneObjetoEnMano = false;
    public int clientesAtendidosTotal = 0;

    [Header("Final de Acto")]
    public GameObject objetoMujer;      
    public GameObject triggerRegresoBarra; 

    [Header("Secuencia de la Botella")]
    public GameObject botellaEspecial;     // El cilindro en la cocina

    [Header("Combate")]
    public GameObject linternaObjeto;      // El objeto de la linterna para recoger
    public GameObject[] enemigos;
    public AudioSource sonidoMutacion;
    public CameraShake sacudidaCamara;

    public int enemigosDerrotados = 0;
    public int totalEnemigos = 3;

    [Header("Referencias de Cierre")] // Las luces originales del bar
    public CanvasGroup fadeCanvasGroup; // Un Panel negro que cubra toda la pantalla
    public TextMeshProUGUI interactionText;

    [Header("Objetivos")]
    public TextMeshProUGUI textoObjetivo;

    [Header("Indicadores de Objetivos")]
    public GameObject indicadorNevera;
    public GameObject indicadorCervezas;



    public void ClienteAtendido()
    {
        ClientesAtendidos++;
    
        // Si ya atendió a los suficientes, podemos habilitar el trigger del olor
        if (ClientesParaAtender >= ClientesAtendidos)
        {
            Debug.Log("Lucas terminó el servicio. Camino a la cocina habilitado.");
            // Aquí podrías activar el objeto del Trigger del Olor si lo tenías desactivado
        }
    }

    public bool ListoParaSotano()
    {
        return ClientesAtendidos >= ClientesParaAtender;
    }
    
    void Start()
    {
        Time.timeScale = 1f;
        
        estadoActual = ActoState.Limpieza;

        if (fadeCanvasGroup != null)
        {
            // Forzamos que el objeto esté activo y sea negro al 100%
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 1f;
        
            Debug.Log("Iniciando fundido de entrada...");
            StartCoroutine(SecuenciaInicioNoche());
        }
        else 
        {
            Debug.LogError("¡Ojo! No asignaste el fadeCanvasGroup en el Inspector.");
        }

        CambiarIluminacion("Normal");
        if (grupoClientes != null) grupoClientes.SetActive(false);
        MostrarDialogo("Hay que dejar todo listo antes de abrir...");
        ActualizarObjetivo("Acomodá las sillas (" + sillasAcomodadas + "/" + totalSillas + ")");
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

    IEnumerator SecuenciaInicioNoche()
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

    public void SillaCompletada()
    {
        sillasAcomodadas++;
        Debug.Log("Sillas: " + sillasAcomodadas + "/" + totalSillas);

        if (sillasAcomodadas >= totalSillas && estadoActual == ActoState.Limpieza)
        {
            StartCoroutine(SecuenciaTransicionSuelo());
        }
        ActualizarObjetivo("Acomodá las sillas (" + sillasAcomodadas + "/" + totalSillas + ")");
    }

    // Usamos un IEnumerator para manejar los tiempos de forma secuencial
    IEnumerator SecuenciaTransicionSuelo()
    {
        yield return new WaitForSeconds(4f);
        // 1. Empieza el sonido del golpe sordo
        if (sonidoGolpeSuelo != null)
        {
            sonidoGolpeSuelo.Play();
            ParanoiaSystem.Instance.AddParanoia(15f);

            yield return new WaitForSeconds(2f);

            // Lucas reacciona
            MostrarDialogo("¿Qué fue eso? Estas cañerías están cada vez peor...");

            // Esperamos un segundo de silencio tenso después del golpe
            yield return new WaitForSeconds(2f);

        }   

        yield return new WaitForSeconds(4f);

        // 2. Lanzamos el efecto de parpadeo (que dura X segundos)
        if (effectoParpadeo != null)
        {
            effectoParpadeo.IniciarParpadeo();
        
            yield return new WaitForSeconds(1f);
            CambiarIluminacion("Servicio");
        }

        // Esperamos a que el parpadeo esté por terminar (por ejemplo, antes del apagón final)
        // Si el parpadeo dura 1.5s en total, esperamos 1s

        // 3. Activamos a los clientes MIENTRAS la luz está parpadeando
        if (grupoClientes != null) grupoClientes.SetActive(true);

        // Esperamos un poquito más para asegurar que el parpadeo terminó
        yield return new WaitForSeconds(0.5f);

        // 4. Cambiamos de estado y Lucas habla
        estadoActual = ActoState.Servicio;
        MostrarDialogo("Clientes ?... a trabajar.");
        ActualizarObjetivo("Atende a los clientes, busca las bebidas detras de la barra.");

        if (indicadorNevera != null) 
        {
            indicadorNevera.SetActive(true);
            Debug.Log("Indicador de nevera activado.");
        }

        if (indicadorCervezas != null)
        {
            indicadorCervezas.SetActive(true);
        }

        ParanoiaSystem.Instance.AddParanoia(25f);//sube la paranoia en un 25%

        if (ambientBar != null && !ambientBar.isPlaying)
        {
            ambientBar.Play();
        }
        if (musicBar != null && !musicBar.isPlaying)
        {
                musicBar.Play();
        }
    }

    void AparecerClientes()
    {
        estadoActual = ActoState.Servicio;
        if (grupoClientes != null) grupoClientes.SetActive(true);
        
        // Diálogo de inicio de jornada
        MostrarDialogo("Clientes ?... a trabajar.");

    }

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

    public void RecogerObjeto(bool esEnBarra)
    {
        if (esEnBarra && clientesAtendidosTotal == 1) // El segundo cliente
        {
            MostrarDialogo("Lucas: No queda nada acá... voy a tener que ir a buscar a la cocina.");
            // Aquí habilitamos el Trigger de la cocina
        }
        else
        {
            tieneObjetoEnMano = true;
            MostrarDialogo("Lucas: Ya tengo el pedido. A entregarlo.");
        }
    }

    public bool TienePedidoEntregable() => tieneObjetoEnMano;

    public void ClienteCompletado()
    {
        clientesAtendidosTotal++;

        // Si es el segundo cliente (el que nos mandó a la cocina)
        if (clientesAtendidosTotal == 2)
        {
            StartCoroutine(SecuenciaQuiebreRealidad());
        }
    }

    IEnumerator SecuenciaQuiebreRealidad()
    {   
        // 1. Segundo parpadeo (el que limpia el bar)
        if (effectoParpadeo != null) effectoParpadeo.IniciarParpadeo();
    
        // Esperamos un momento en medio del parpadeo
        yield return new WaitForSeconds(1f);

        // 2. Apagamos la música y desaparecemos a los clientes
        if (ambientBar != null) ambientBar.Stop();
        if (musicBar != null) musicBar.Stop();
        CambiarIluminacion("Normal");
    
        // Desactivamos el grupo de clientes (el GameObject que los contiene a todos)
        if (grupoClientes != null) grupoClientes.SetActive(false);

        yield return new WaitForSeconds(2f);
        ParanoiaSystem.Instance.AddParanoia(15f);
        ActualizarObjetivo("??? explora el bar en busca de los clientes");

        // 3. Lucas reacciona
        MostrarDialogo("Lucas: ¿Qué...? ¿A dónde se fueron todos? No hace ninguna gracia...");

        // 4. Habilitamos el trigger de regreso a la barra para la aparición
        if (triggerRegresoBarra != null) triggerRegresoBarra.SetActive(true);
    }

    public void CambiarIluminacion(string estado)
    {
        Debug.Log("Cambiando iluminación a: " + estado);

        // Apagamos absolutamente todo primero para que no se pisen
        if (lucesNormales != null) lucesNormales.SetActive(false);
        if (lucesServicio != null) lucesServicio.SetActive(false);
        if (lucesCreepy != null) lucesCreepy.SetActive(false);
        if (lucesCombate != null) lucesCombate.SetActive(false);

        switch (estado)
        {
            case "Normal":
                if (lucesNormales != null) lucesNormales.SetActive(true);
                RenderSettings.ambientLight = new Color(0.22f, 0.22f, 0.22f); // Claridad normal de base
                break;

            case "Servicio":
                if (lucesServicio != null) lucesServicio.SetActive(true);
                RenderSettings.ambientLight = new Color(0.15f, 0.15f, 0.15f); // Penumbra ambiente violeta
                break;

            case "Creepy":
                if (lucesCreepy != null) lucesCreepy.SetActive(true);
                RenderSettings.ambientLight = new Color(0.05f, 0.05f, 0.05f); // Casi oscuras, resalta la barra
                break;

            case "Combate":
                if (lucesCombate != null) lucesCombate.SetActive(true); // Tus luces rojas/combate
                RenderSettings.ambientLight = Color.black; // Oscuridad absoluta en la cocina/salón
                break;

            default:
                Debug.LogWarning("El estado de luz '" + estado + "' no existe.");
                break;
        }
    }

    public void HabilitarTriggerCocinaFinal()
    {
        if (botellaEspecial != null)
        {
            // Activamos el objeto para que el jugador pueda interactuar con él
            botellaEspecial.SetActive(true);
            ActualizarObjetivo("Buscá la botella especial en la cocina");
        
            // Opcional: Podés poner una flecha violeta sutil sobre la puerta 
            // de la cocina para guiar al jugador en este momento de confusión.
            Debug.Log("Misión: Ir a buscar la botella especial.");
        }
    }

    public void AlRecogerBotellaEspecial()
    {
        estadoActual = ActoState.ElQuiebre;
        CambiarIluminacion("Combate"); // Se pone todo en negro/rojo
        ActualizarObjetivo("¡SOBREVIVE! derrota a las sombras con tu linterna (click derecho para hacerles daño)");

        if (objetoMujer != null) objetoMujer.SetActive(false);
        if (linternaObjeto != null) linternaObjeto.SetActive(true); // Se activa la linterna física para recoger

        // Lanzamos el temblor y el sonido
        if (sacudidaCamara != null) StartCoroutine(sacudidaCamara.Shake(0.8f, 0.2f));
        if (sonidoMutacion != null) sonidoMutacion.Play();
        ParanoiaSystem.Instance.AddParanoia(100f);

        // ACTIVACIÓN DE ENEMIGOS
        StartCoroutine(ActivarEnemigosSecuencial());

        // Reacción inicial de Lucas
        MostrarDialogo("Lucas: ¡¿Qué carajo fue eso?! ¡No veo nada!");

        // 🔥 NUEVO: Lanzamos la secuencia del tutorial de la linterna
        StartCoroutine(SecuenciaTutorialLinterna());
    }

    // 🔥 NUEVA CORRUTINA: Espera 5 segundos y te enseña a usar la F
    IEnumerator SecuenciaTutorialLinterna()
    {
        // Espera los 5 segundos que me pediste desde que empezó el caos
        yield return new WaitForSeconds(5f);

        // Lanza el diálogo instructivo en pantalla
        MostrarDialogo("Puedes presionar la [F] para encender la linterna.");
    }

    IEnumerator ActivarEnemigosSecuencial()
    {
        // Esperamos un segundo después del estruendo para que aparezcan
        yield return new WaitForSeconds(1f);

        foreach (GameObject enemigo in enemigos)
        {
            if (enemigo != null)
            {
                enemigo.SetActive(true);
                // Si tienen un AudioSource con un rugido o sonido metálico, tiralo acá
                AudioSource snd = enemigo.GetComponent<AudioSource>();
                if(snd != null) snd.Play();
            }
            
            // Opcional: que aparezcan de a uno con un pequeño delay
            yield return new WaitForSeconds(0.5f);
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
        // 1. Volver a la normalidad
        lucesCombate.SetActive(false);
        lucesNormales.SetActive(true);
        ParanoiaSystem.Instance.AddParanoia(-60f);

        // 2. Lanzar el diálogo (Si tenés un sistema de subtítulos)
        Debug.Log("Lucas: 'Uff... qué carajo fue eso... estoy agotado...'");
        
        // 3. Empezar el fundido a negro
        StartCoroutine(SecuenciaCierreNoche());
    }

    IEnumerator SecuenciaCierreNoche()
    {
        // 1. 🔥 EL NUEVO PARPADEO POST-COMBATE
        // Usamos tu script especializado para hacer titilar las luces antes del apagón
        if (effectoParpadeo != null)
        {
            Debug.Log("Iniciando parpadeo de luces post-combate...");
            effectoParpadeo.IniciarParpadeo();
        }

        // Esperamos un momento tenso mientras las luces titilan descontroladas
        yield return new WaitForSeconds(2f);

        // 2. OSCURIDAD TOTAL
        // Apagamos las luces de combate definitivamente para dejar el bar a oscuras
        if (lucesCombate != null) lucesCombate.SetActive(false);
        RenderSettings.ambientLight = Color.black; // Boca de lobo

        yield return new WaitForSeconds(2f);

        // 3. VUELTA A LA NORMALIDAD (A medias... Lucas está exhausto)
        if (lucesNormales != null) lucesNormales.SetActive(true);
        ActualizarObjetivo("Lograste sobrevivir");
        
        if (interactionText != null) 
        {
            MostrarDialogo("Lucas: ¿Qué mierda fue eso?");
            yield return new WaitForSeconds(3f);
            MostrarDialogo("Lucas: No puedo más... Necesito descansar...");
        }

        // 4. ESPERA ANTES DEL FIN 
        yield return new WaitForSeconds(8f);

        // 5. FUNDIDO A NEGRO FINAL (Transición directa a la Noche 2)
        float duracionFade = 2.5f;
        float tiempoFade = 0;
        while (tiempoFade < duracionFade)
        {
            tiempoFade += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, tiempoFade / duracionFade);
            yield return null;
        }

        Debug.Log("Noche 1 terminada con éxito. Cargando Noche 2...");
        SceneManager.LoadScene("Night_2"); 
    }
}