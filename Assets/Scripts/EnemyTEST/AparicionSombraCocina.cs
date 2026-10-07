using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using TMPro;


public class AparicionSombraCocina : MonoBehaviour
{
    [Header("Jugador")]
    public Camera camaraJugador;

    [Header("Linterna")]
    public Light luzLinterna;

    [Tooltip("Cantidad de apagados/prendidos antes del apagón final.")]
    public int cantidadTitileos = 4;

    [Tooltip("Tiempo entre cada cambio de estado de la linterna.")]
    public float intervaloTitileo = 0.12f;

    [Tooltip("Tiempo que queda completamente a oscuras.")]
    public float tiempoOscuridad = 1f;

    [Header("Audio linterna")]
    public AudioSource audioTitileo;

    [Header("Criatura")]
    public GameObject criatura;

    [Header("Sincronización aparición")]
    public float retrasoSonidoAparicion = 0.08f;

    [Tooltip("Punto que el jugador debe mirar para activar el susto.")]
    public Transform puntoMiradaCriatura;

    [Range(0.01f, 0.5f)]
    public float toleranciaMirada = 0.18f;

    [Tooltip("Cuánto tiempo debe mantener la mirada. 0 = instantáneo.")]
    public float tiempoMiradaNecesario = 0f;

    [Header("Audio aparición")]
    public AudioSource sonidoAparicion;
    [Header("Audio - Combate")]
    public AudioSource baseEnemy1;
    public AudioSource baseEnemy2;
    public AudioSource enemyVoices;
    public AudioSource deadCreature;

    [Header("Persecución")]
    public SombraWalkTest scriptPersecucion;
    public NavMeshAgent navMeshAgent;
    [Header("FX - Combate")]
    [Header("Tutorial primer Espectro")]
    public bool activarTutorial = true;

    public Volume volumenTutorial;

    [Range(0.05f, 1f)]
    public float escalaTiempoTutorial = 0.25f;
    [Range(0.05f, 1f)]
    public float escalaTiempoTutorialAtaque = 0.45f;

    public float velocidadEntradaTutorial = 4f;
    public float velocidadSalidaTutorial = 2.5f;

    [Header("UI Tutorial")]
    public TMP_Text textoTutorial;

    private bool tutorialActivo = false;
    public Volume volumenCombate;
    [Header("FX - Sacudida")]
    public Transform objetivoSacudida;

    public float duracionSacudida = 0.25f;

    public float fuerzaSacudida = 0.08f;

    [Range(0f, 1f)]
    public float pesoVolumeCombate = 1f;

    public float velocidadEntradaVolumeCombate = 8f;

    [Header("Radio - Señuelo")]
    public AudioSource audioRadio;
    public AudioSource audioEstaticaRadio;

    [Tooltip("Cuánto suena la radio si el jugador ya estaba cerca.")]
    public float tiempoRadioCorta = 1.5f;

    [Tooltip("Si la radio ya venía llamándolo desde lejos, cuánto esperamos al entrar en la zona antes de que falle.")]
    public float esperaAntesInterferencia = 0.3f;

    [Header("Radio - Interferencia")]
    public int cantidadCortesRadio = 5;

    public float intervaloCorteMin = 0.04f;
    public float intervaloCorteMax = 0.11f;

    [Header("Radio - Recordatorio")]
    [Tooltip("Si no mira después de que la radio se apaga, cada cuánto hacemos un pequeño ruido para volver a llamar su atención.")]
    public float tiempoRecordatorioMirada = 3.5f;

    [Header("Punto mirada - Obstáculos")]
    public LayerMask capasBloqueoMirada;

    private Coroutine rutinaVolumeCombate;

    private bool secuenciaIniciada;
    private bool esperandoMirada;
    private bool revelada;

    private float tiempoMirando;

    private bool eventoRadioHabilitado;
    private bool radioLlamando;
    private bool zonaEncuentroActivada;

    private Coroutine rutinaRadio;
    private Coroutine rutinaRecordatorioRadio;

    private float volumenOriginalRadio = 1f;

    private void Start()
    {

            if (audioRadio != null)
    {
        audioRadio.playOnAwake = false;
        audioRadio.loop = true;

        volumenOriginalRadio =
            audioRadio.volume;

        audioRadio.Stop();
    }

    if (audioEstaticaRadio != null)
    {
        audioEstaticaRadio.playOnAwake = false;
        audioEstaticaRadio.loop = false;
        audioEstaticaRadio.Stop();
    }


    // Importante:
    // cuando la criatura finalmente se active,
    // todavía no queremos que empiece a caminar sola.

    if (scriptPersecucion != null)
        scriptPersecucion.enabled = false;

    if (navMeshAgent != null)
        navMeshAgent.enabled = false;
        // La criatura empieza escondida.
        if (criatura != null)
            criatura.SetActive(false);

        if (audioTitileo != null)
            audioTitileo.playOnAwake = false;

        if (sonidoAparicion != null)
            sonidoAparicion.playOnAwake = false;

        // El efecto de combate comienza apagado.
        if (volumenCombate != null)
            volumenCombate.weight = 0f;

        if (baseEnemy1 != null)
        {
            baseEnemy1.playOnAwake = false;
            baseEnemy1.loop = true;
        }

        if (baseEnemy2 != null)
        {
            baseEnemy2.playOnAwake = false;
            baseEnemy2.loop = true;
        }

        if (enemyVoices != null)
        {
            enemyVoices.playOnAwake = false;
            enemyVoices.loop = true;
        }

        if (deadCreature != null)
        {
            deadCreature.playOnAwake = false;
            deadCreature.loop = false;
        }

        if (volumenTutorial != null)
            volumenTutorial.weight = 0f;

        if (textoTutorial != null)
            textoTutorial.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!esperandoMirada || revelada)
            return;

        DetectarMiradaCriatura();
    }

    // Este método se llama desde alLlegarAC.
    public void IniciarAparicion()
    {
        if (secuenciaIniciada)
            return;

        secuenciaIniciada = true;

        // Desde ahora las zonas pueden reaccionar.
        eventoRadioHabilitado = true;

        Debug.Log(
            "[AparicionSombra] Encuentro habilitado. " +
            "Esperando ZonaHuida o ZonaEncuentro."
        );
    }



    private void DetectarMiradaCriatura()
    {
        if (camaraJugador == null ||
            puntoMiradaCriatura == null)
        {
            return;
        }


        Vector3 viewport =
            camaraJugador.WorldToViewportPoint(
                puntoMiradaCriatura.position
            );


        bool mirando =
            viewport.z > 0f &&
            Mathf.Abs(viewport.x - 0.5f) <= toleranciaMirada &&
            Mathf.Abs(viewport.y - 0.5f) <= toleranciaMirada;


        // ==========================================
        // COMPROBAR SI HAY UNA PARED EN EL MEDIO
        // ==========================================

        if (mirando &&
            capasBloqueoMirada.value != 0)
        {
            Vector3 origen =
                camaraJugador.transform.position;

            Vector3 haciaPunto =
                puntoMiradaCriatura.position -
                origen;

            float distancia =
                haciaPunto.magnitude;


            bool bloqueado =
                Physics.Raycast(
                    origen,
                    haciaPunto.normalized,
                    distancia,
                    capasBloqueoMirada
                );


            if (bloqueado)
            {
                mirando = false;
            }
        }


        // ==========================================
        // DETECCIÓN REAL DE LA MIRADA
        // ==========================================

        if (mirando)
        {
            tiempoMirando += Time.deltaTime;

            if (tiempoMirando >=
                tiempoMiradaNecesario)
            {
                RevelarCriatura();
            }
        }
        else
        {
            tiempoMirando = 0f;
        }
    }

    public void EntrarZonaHuida()
    {
        if (!eventoRadioHabilitado ||
            zonaEncuentroActivada ||
            revelada)
        {
            return;
        }

        if (radioLlamando)
            return;

        radioLlamando = true;

        EncenderRadio();

        Debug.Log(
            "[AparicionSombra] Jugador intenta alejarse. Radio encendida."
        );
    }


        public void EntrarZonaEncuentro()
        {
            if (!eventoRadioHabilitado ||
                zonaEncuentroActivada ||
                revelada)
            {
                return;
            }

            zonaEncuentroActivada = true;

            if (rutinaRadio != null)
                StopCoroutine(rutinaRadio);

            rutinaRadio =
                StartCoroutine(
                    PrepararEncuentroConRadio()
                );

            Debug.Log(
                "[AparicionSombra] Jugador llegó a ZonaEncuentro."
            );
        }

        private void EncenderRadio()
        {
            if (audioRadio == null)
                return;

            audioRadio.Stop();

            audioRadio.volume =
                volumenOriginalRadio;

            audioRadio.loop = true;

            audioRadio.Play();
        }

        private IEnumerator PrepararEncuentroConRadio()
        {
            bool radioYaEstabaSonando =
                audioRadio != null &&
                audioRadio.isPlaying;


            // ==========================================
            // CASO 1:
            // YA ESTABA CERCA
            // ==========================================

            if (!radioYaEstabaSonando)
            {
                EncenderRadio();

                yield return new WaitForSeconds(
                    Mathf.Max(0f, tiempoRadioCorta)
                );
            }


            // ==========================================
            // CASO 2:
            // VENÍA SIGUIENDO LA RADIO DESDE LA PUERTA
            // ==========================================

            else
            {
                yield return new WaitForSeconds(
                    Mathf.Max(
                        0f,
                        esperaAntesInterferencia
                    )
                );
            }


            // ==========================================
            // LA RADIO EMPIEZA A FALLAR
            // ==========================================

            yield return StartCoroutine(
                InterferenciaRadio()
            );


            // ==========================================
            // SILENCIO.
            // AHORA SÍ PUEDE ACTIVARSE CON LA MIRADA.
            // ==========================================

            esperandoMirada = true;
            tiempoMirando = 0f;


            Debug.Log(
                "[AparicionSombra] Radio muerta. Esperando mirada a la criatura."
            );


            // Si tarda mucho en mirar,
            // hacemos pequeños recordatorios auditivos.
            if (rutinaRecordatorioRadio != null)
                StopCoroutine(rutinaRecordatorioRadio);

            rutinaRecordatorioRadio =
                StartCoroutine(
                    RecordatorioRadio()
                );
        }

        private IEnumerator InterferenciaRadio()
        {
            if (audioEstaticaRadio != null)
            {
                audioEstaticaRadio.Stop();
                audioEstaticaRadio.Play();
            }


            if (audioRadio != null)
            {
                for (int i = 0;
                    i < cantidadCortesRadio;
                    i++)
                {
                    // CORTE
                    audioRadio.volume = 0f;

                    yield return new WaitForSeconds(
                        Random.Range(
                            intervaloCorteMin,
                            intervaloCorteMax
                        )
                    );


                    // VUELVE
                    audioRadio.volume =
                        volumenOriginalRadio;

                    yield return new WaitForSeconds(
                        Random.Range(
                            intervaloCorteMin,
                            intervaloCorteMax
                        )
                    );
                }


                // CORTE DEFINITIVO
                audioRadio.Stop();

                audioRadio.volume =
                    volumenOriginalRadio;
            }


            if (audioEstaticaRadio != null)
            {
                audioEstaticaRadio.Stop();
            }
        }

        private IEnumerator RecordatorioRadio()
        {
            while (esperandoMirada &&
                !revelada)
            {
                yield return new WaitForSeconds(
                    tiempoRecordatorioMirada
                );

                if (!esperandoMirada ||
                    revelada)
                {
                    yield break;
                }


                // Un ruido corto desde la radio:
                // "mirá para acá".
                if (audioEstaticaRadio != null)
                {
                    audioEstaticaRadio.Stop();
                    audioEstaticaRadio.Play();
                }
            }
        }


    private void RevelarCriatura()
    {
        if (revelada)
            return;

        revelada = true;
        esperandoMirada = false;

        // ==========================================
        // DETENER TODO RASTRO DE LA RADIO
        // ==========================================

        if (rutinaRecordatorioRadio != null)
        {
            StopCoroutine(
                rutinaRecordatorioRadio
            );

            rutinaRecordatorioRadio = null;
        }

        if (audioRadio != null)
            audioRadio.Stop();

        if (audioEstaticaRadio != null)
            audioEstaticaRadio.Stop();


        // ==========================================
        // APARICIÓN REAL DE LA CRIATURA
        // ==========================================

        if (criatura != null)
        {
            criatura.SetActive(true);
        }

        // ==========================================
        // SACUDIDA INMEDIATA
        // ==========================================

        StartCoroutine(SacudirCamara());

        // ==========================================
        // SONIDO DE APARICIÓN CON PEQUEÑO RETRASO
        // ==========================================

        if (sonidoAparicion != null)
        {
            sonidoAparicion.Stop();
            sonidoAparicion.loop = false;

            sonidoAparicion.PlayDelayed(
                Mathf.Max(0f, retrasoSonidoAparicion)
            );
        }

        // ==========================================
        // VOLUME DE COMBATE
        // ==========================================

        ActivarVolumeCombate();

        // ==========================================
        // AUDIOS PERMANENTES DEL COMBATE
        // ==========================================

        if (baseEnemy1 != null && !baseEnemy1.isPlaying)
            baseEnemy1.Play();

        if (baseEnemy2 != null && !baseEnemy2.isPlaying)
            baseEnemy2.Play();

        if (enemyVoices != null && !enemyVoices.isPlaying)
            enemyVoices.Play();

        Debug.Log(
            "[AparicionSombra] Jugador vio la criatura."
        );

        // ==========================================
        // COMIENZA LA PERSECUCIÓN NORMAL
        // ==========================================

        if (navMeshAgent != null)
            navMeshAgent.enabled = true;

        if (scriptPersecucion != null)
            scriptPersecucion.enabled = true;


        // ==========================================
        // TUTORIAL DEL PRIMER ESPECTRO
        // ==========================================

        if (activarTutorial)
        {
            StartCoroutine(TutorialPrimerEspectro());
        }
    }
    private IEnumerator TutorialPrimerEspectro()
    {
        tutorialActivo = true;

        // ==========================================
        // 1. DEJAR QUE LA APARICIÓN RESPIRE
        // ==========================================

        // Durante este segundo:
        // - todo está a velocidad normal
        // - NO hay blanco y negro
        // - NO aparece ningún tutorial
        // - el Espectro ya viene caminando hacia Lucas

        yield return new WaitForSecondsRealtime(1f);


        // ==========================================
        // 2. INICIAR EL MOMENTO TUTORIAL
        // ==========================================

        Time.timeScale = escalaTiempoTutorial;


        // ==========================================
        // 3. ENTRAR BLANCO Y NEGRO
        // ==========================================

        if (volumenTutorial != null)
        {
            volumenTutorial.weight = 0f;
        }


        // Entrada gradual del blanco y negro.
        if (volumenTutorial != null)
        {
            while (volumenTutorial.weight < 0.99f)
            {
                volumenTutorial.weight =
                    Mathf.MoveTowards(
                        volumenTutorial.weight,
                        1f,
                        velocidadEntradaTutorial *
                        Time.unscaledDeltaTime
                    );

                yield return null;
            }

            volumenTutorial.weight = 1f;
        }


        // ==========================================
        // 4. ESPERAR UN CLICK DERECHO NUEVO
        // ==========================================

        // Si el jugador ya venía manteniendo click,
        // primero obligamos a que lo suelte.
        yield return new WaitUntil(
            () => !Input.GetMouseButton(1)
        );

        if (Act1Manager.Instance != null)
        {
            Act1Manager.Instance.MostrarDialogo(
                "Mantené [CLICK DERECHO] para concentrar la linterna.",
                true
            );
        }

        // Ahora esperamos un click derecho nuevo.
        yield return new WaitUntil(
            () => Input.GetMouseButtonDown(1)
        );


        // ==========================================
        // 5. SEGUNDA INSTRUCCIÓN
        // ==========================================

        // Ya entendió cómo concentrar la linterna.
        // Ahora damos un poco más de velocidad,
        // pero seguimos en cámara lenta.
        Time.timeScale = escalaTiempoTutorialAtaque;


        string mensajeDisipar =
            "Mantené el haz sobre el Espectro hasta disiparlo.";


        // Conseguimos EnemyCore ANTES de esperar daño.
        EnemyCore enemyCore = null;

        if (criatura != null)
        {
            enemyCore =
                criatura.GetComponent<EnemyCore>();
        }


        float vidaAntes =
            enemyCore != null
            ? enemyCore.health
            : 0f;


        if (Act1Manager.Instance != null)
        {
            Act1Manager.Instance.MostrarDialogo(
                mensajeDisipar,
                true
            );
        }


        // ==========================================
        // ESPERAR A QUE TERMINE EL TYPEWRITER
        // ==========================================

        // Calculamos aproximadamente cuánto tarda
        // en escribirse toda la frase.
        float velocidadTexto =
            Act1Manager.Instance != null &&
            Act1Manager.Instance.velocidadEscritura > 0f
            ? Act1Manager.Instance.velocidadEscritura
            : 0.03f;

        float tiempoEscritura =
            mensajeDisipar.Length * velocidadTexto;


        // Tiempo REAL: no afectado por cámara lenta.
        yield return new WaitForSecondsRealtime(
            tiempoEscritura
        );


        // ==========================================
        // 6. ESPERAR DAÑO REAL
        // ==========================================

        if (enemyCore != null)
        {
            yield return new WaitUntil(
                () =>
                    enemyCore == null ||
                    enemyCore.health < vidaAntes
            );
        }
        else
        {
            Debug.LogWarning(
                "[TutorialEspectro] No se encontró EnemyCore."
            );

            yield return new WaitForSecondsRealtime(0.5f);
        }


        // ==========================================
        // 7. SALIR DEL TUTORIAL
        // ==========================================

        float escalaInicial = Time.timeScale;
        float progreso = 0f;

        while (progreso < 1f)
        {
            progreso +=
                Time.unscaledDeltaTime / 0.6f;

            float t = Mathf.Clamp01(progreso);


            // Volver progresivamente al tiempo normal.
            Time.timeScale =
                Mathf.Lerp(
                    escalaInicial,
                    1f,
                    t
                );


            // Sacar el blanco y negro progresivamente.
            if (volumenTutorial != null)
            {
                volumenTutorial.weight =
                    Mathf.MoveTowards(
                        volumenTutorial.weight,
                        0f,
                        velocidadSalidaTutorial *
                        Time.unscaledDeltaTime
                    );
            }

            yield return null;
        }


        Time.timeScale = 1f;


        if (volumenTutorial != null)
            volumenTutorial.weight = 0f;


        // Dejamos visible la segunda indicación
        // un instante más.
        yield return new WaitForSecondsRealtime(1f);


        if (textoTutorial != null)
            textoTutorial.gameObject.SetActive(false);


        tutorialActivo = false;
    }

        private void ActivarVolumeCombate()
    {
        if (volumenCombate == null)
            return;

        if (rutinaVolumeCombate != null)
            StopCoroutine(rutinaVolumeCombate);

        rutinaVolumeCombate = StartCoroutine(
            TransicionarVolumeCombate(pesoVolumeCombate)
        );
    }

    private IEnumerator TransicionarVolumeCombate(float objetivo)
    {
        while (Mathf.Abs(volumenCombate.weight - objetivo) > 0.01f)
        {
            volumenCombate.weight = Mathf.MoveTowards(
                volumenCombate.weight,
                objetivo,
                velocidadEntradaVolumeCombate * Time.deltaTime
            );

            yield return null;
        }

        volumenCombate.weight = objetivo;
        rutinaVolumeCombate = null;
    }
        private IEnumerator SacudirCamara()
    {
        if (objetivoSacudida == null)
            yield break;

        Vector3 posicionOriginal =
            objetivoSacudida.localPosition;

        float tiempo = 0f;

        while (tiempo < duracionSacudida)
        {
            tiempo += Time.deltaTime;

            Vector3 desplazamiento =
                Random.insideUnitSphere * fuerzaSacudida;

            objetivoSacudida.localPosition =
                posicionOriginal + desplazamiento;

            yield return null;
        }

        objetivoSacudida.localPosition =
            posicionOriginal;
    }
}