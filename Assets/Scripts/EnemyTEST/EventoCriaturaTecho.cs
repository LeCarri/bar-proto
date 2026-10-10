
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class EventoCriaturaTecho : MonoBehaviour
{
    [Header("Jugador")]
    public Camera camaraJugador;
    public string tagJugador = "Player";

    [Header("Criatura")]
    public GameObject criatura;
    public Transform puntoMirada;
    public CuelloCriaturaTecho cuelloCriatura;
    public LinternaJumpscarePose linternaPose;

    [Header("Spawn")]
    public Transform puntoSpawnCriatura;

    [Header("Deteccion")]
    public float esperaInicial = 1f;

    [Range(0.1f, 0.95f)]
    public float toleranciaMirada = 0.75f;

    public float tiempoMirada = 0.15f;

    [Header("Audio - Aparicion")]
    public AudioSource audioRasgunos;
    public AudioSource audioJumpscare;

    [Header("Audio Jumpscare")]
    [Min(0f)]
    public float duracionAudioJumpscare = 0.8f;

    [Min(0f)]
    public float duracionFadeOutJumpscare = 0.35f;

    [Header("Audio - Combate")]
    public AudioSource baseEnemy1;
    public AudioSource baseEnemy2;
    public AudioSource enemyVoices;

    [Header("Volume del Jumpscare")]
    public Volume volumeJumpscare;
    public float duracionVolume = 0.35f;
    public float tiempoVolumeIntenso = 0.10f;

    [Header("Sacudida de camara")]
    public Transform pivoteSacudida;
    public float intensidadSacudida = 0.08f;
    public float duracionSacudida = 0.22f;

    [Header("FX - Danio recibido por Lucas")]
    public Volume volumeDesintegracion;

    [Range(0f, 1f)]
    public float intensidadMaximaDesintegracion = 1f;

    [Min(0f)]
    public float velocidadCambioDesintegracion = 5f;

    [Min(0f)]
    public float duracionSalidaDesintegracion = 1.5f;

    [Header("Volume por golpes recibidos")]
    [Min(0.01f)]
    public float vidaReferenciaVolume = 100f;

    [Header("Combate - Criatura 3")]
    public float tiempoVentajaJugador = 3f;
    public float tiempoEntreAtaques = 2f;
    [Header("Radio de ataque")]
    public Transform centroAtaque;

    [Min(0.1f)]
    public float radioAtaque = 3f;

    [Header("Ataque")]
    public float danioAtaque = 35f;
    [Header("Audio - Ataque Criatura 3")]
    [SerializeField] private AudioSource audioAtaque;

    // =====================================
    // ESTADOS
    // =====================================

    private bool activado;
    private bool esperandoMirada;
    private bool jumpscareIniciado;
    private bool combateFinalizado;
    private bool criaturaDerrotada;
    private bool terminandoDesintegracion;

    private float contadorMirada;

    // Enemigo
    private EnemyCore vidaCriatura;

    // Solo registra el daño que esta criatura
    // le provoca realmente a Lucas.
    private float danioAcumuladoCriatura3 = 0f;

    // Corrutinas
    private Coroutine rutinaSalidaDesintegracion;
    private Coroutine rutinaAudioJumpscare;
    private Coroutine rutinaSacudida;
    private Coroutine rutinaCombate;

    // =====================================
    // START
    // =====================================

    void Start()
    {
        if (criatura != null)
        {
            vidaCriatura = criatura.GetComponent<EnemyCore>();

            if (vidaCriatura == null)
            {
                vidaCriatura =
                    criatura.GetComponentInChildren<EnemyCore>(true);
            }

            criatura.SetActive(false);
        }

        if (volumeJumpscare != null)
            volumeJumpscare.weight = 0f;

        if (volumeDesintegracion != null)
            volumeDesintegracion.weight = 0f;

        PrepararAudioCombate(baseEnemy1);
        PrepararAudioCombate(baseEnemy2);
        PrepararAudioCombate(enemyVoices);

        if (audioJumpscare != null)
        {
            audioJumpscare.playOnAwake = false;
            audioJumpscare.loop = false;
        }

        if (audioRasgunos != null)
        {
            audioRasgunos.playOnAwake = false;
            audioRasgunos.loop = true;
        }

        danioAcumuladoCriatura3 = 0f;
    }

    void PrepararAudioCombate(AudioSource audio)
    {
        if (audio == null)
            return;

        audio.playOnAwake = false;
        audio.loop = true;
        audio.Stop();
    }

    // =====================================
    // UPDATE
    // =====================================

    void Update()
    {
        // Actualizar el Volume segun el daño
        // causado por esta criatura a Lucas.
        ActualizarVolumeDanioJugador();

        // Deteccion de mirada
        if (!esperandoMirada ||
            camaraJugador == null ||
            puntoMirada == null)
            return;

        Vector3 haciaPunto =
            puntoMirada.position -
            camaraJugador.transform.position;

        if (haciaPunto.sqrMagnitude < 0.001f)
            return;

        float coincidencia = Vector3.Dot(
            camaraJugador.transform.forward,
            haciaPunto.normalized
        );

        if (coincidencia >= toleranciaMirada)
        {
            contadorMirada += Time.deltaTime;

            if (contadorMirada >= tiempoMirada)
            {
                esperandoMirada = false;

                if (!jumpscareIniciado)
                    StartCoroutine(EjecutarJumpscare());
            }
        }
        else
        {
            contadorMirada = 0f;
        }
    }

    // =====================================
    // TRIGGER
    // =====================================

    void OnTriggerEnter(Collider other)
    {
        if (activado || !other.CompareTag(tagJugador))
            return;

        activado = true;

        StartCoroutine(IniciarEvento());
    }

    IEnumerator IniciarEvento()
    {
        yield return new WaitForSeconds(
            Mathf.Max(0f, esperaInicial)
        );

        if (audioRasgunos != null)
            audioRasgunos.Play();

        esperandoMirada = true;
    }

    // =====================================
    // APARICION Y JUMPSCARE
    // =====================================

    IEnumerator EjecutarJumpscare()
    {
        if (jumpscareIniciado)
            yield break;

        if (criatura == null ||
            cuelloCriatura == null ||
            puntoSpawnCriatura == null)
        {
            Debug.LogError(
                "[Criatura3] Faltan referencias."
            );

            yield break;
        }

        jumpscareIniciado = true;

        if (audioRasgunos != null)
            audioRasgunos.Stop();

        // Aparece en el punto de spawn
        criatura.transform.SetPositionAndRotation(
            puntoSpawnCriatura.position,
            puntoSpawnCriatura.rotation
        );

        criatura.SetActive(true);

        // Obtener EnemyCore
        if (vidaCriatura == null)
        {
            vidaCriatura =
                criatura.GetComponentInChildren<EnemyCore>();
        }

        if (vidaCriatura == null)
        {
            Debug.LogError(
                "[Criatura3] No se encontro EnemyCore."
            );

            yield break;
        }

        // Iniciar sonidos de combate
        IniciarAudioCombate();

        // Jumpscare inicial SIN daño
        yield return StartCoroutine(
            cuelloCriatura.ReproducirJumpscare(
                ActivarImpacto,
                ComenzarRetroceso
            )
        );

        cuelloCriatura.ActivarSeguimiento();

        // Comienza el combate
        if (CriaturaSigueViva())
        {
            rutinaCombate =
                StartCoroutine(CicloCombate());
        }

        Debug.Log(
            "[Criatura3] Jumpscare finalizado. Combate iniciado."
        );
    }

    // =====================================
    // IMPACTO JUMPSCARE
    // =====================================

    void ActivarImpacto()
    {
        // Apartar linterna
        if (linternaPose != null)
            linternaPose.ActivarPoseJumpscare();

        // Audio jumpscare
        if (audioJumpscare != null)
        {
            if (rutinaAudioJumpscare != null)
                StopCoroutine(rutinaAudioJumpscare);

            audioJumpscare.Stop();
            audioJumpscare.loop = false;
            audioJumpscare.Play();

            rutinaAudioJumpscare =
                StartCoroutine(DetenerAudioJumpscare());
        }

        // Volume del impacto
        if (volumeJumpscare != null)
            StartCoroutine(EfectoVolume());

        // Sacudida de camara
        if (pivoteSacudida != null)
        {
            if (rutinaSacudida != null)
                StopCoroutine(rutinaSacudida);

            rutinaSacudida =
                StartCoroutine(SacudirCamara());
        }
    }

    void ComenzarRetroceso()
    {
        if (linternaPose != null)
            linternaPose.RestaurarPoseNormal();
    }

    // =====================================
    // AUDIO JUMPSCARE - FADE OUT
    // =====================================

    IEnumerator DetenerAudioJumpscare()
    {
        if (audioJumpscare == null)
            yield break;

        float volumenOriginal =
            audioJumpscare.volume;

        yield return new WaitForSeconds(
            Mathf.Max(0f, duracionAudioJumpscare)
        );

        float tiempo = 0f;

        float duracion = Mathf.Max(
            0.01f,
            duracionFadeOutJumpscare
        );

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(tiempo / duracion)
            );

            audioJumpscare.volume = Mathf.Lerp(
                volumenOriginal,
                0f,
                t
            );

            yield return null;
        }

        audioJumpscare.Stop();
        audioJumpscare.volume = volumenOriginal;

        rutinaAudioJumpscare = null;
    }

    // =====================================
    // VOLUME JUMPSCARE
    // =====================================

    IEnumerator EfectoVolume()
    {
        if (volumeJumpscare == null)
            yield break;

        volumeJumpscare.weight = 1f;

        yield return new WaitForSeconds(
            Mathf.Max(0f, tiempoVolumeIntenso)
        );

        float tiempo = 0f;

        float duracion = Mathf.Max(
            0.001f,
            duracionVolume
        );

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            volumeJumpscare.weight = Mathf.Lerp(
                1f,
                0f,
                Mathf.Clamp01(tiempo / duracion)
            );

            yield return null;
        }

        volumeJumpscare.weight = 0f;
    }

    // =====================================
    // SACUDIDA ORIGINAL
    // =====================================

    IEnumerator SacudirCamara()
    {
        if (pivoteSacudida == null)
            yield break;

        Vector3 posicionOriginal =
            pivoteSacudida.localPosition;

        float tiempo = 0f;

        while (tiempo < duracionSacudida)
        {
            tiempo += Time.deltaTime;

            Vector3 desplazamiento =
                Random.insideUnitSphere *
                intensidadSacudida;

            pivoteSacudida.localPosition =
                posicionOriginal + desplazamiento;

            yield return null;
        }

        pivoteSacudida.localPosition =
            posicionOriginal;

        rutinaSacudida = null;
    }

    // =====================================
    // AUDIOS DE COMBATE
    // =====================================

    public void IniciarAudioCombate()
    {
        if (combateFinalizado)
            return;

        if (baseEnemy1 != null &&
            !baseEnemy1.isPlaying)
        {
            baseEnemy1.Play();
        }

        if (baseEnemy2 != null &&
            !baseEnemy2.isPlaying)
        {
            baseEnemy2.Play();
        }

        if (enemyVoices != null &&
            !enemyVoices.isPlaying)
        {
            enemyVoices.Play();
        }

        Debug.Log(
            "[Criatura3] Audios de combate iniciados."
        );
    }

    public void DetenerAudioCombate()
    {
        DetenerAudio(baseEnemy1);
        DetenerAudio(baseEnemy2);
        DetenerAudio(enemyVoices);

        if (audioRasgunos != null)
            audioRasgunos.Stop();
    }

    void DetenerAudio(AudioSource audio)
    {
        if (audio != null)
            audio.Stop();
    }

    // =====================================
    // COMBATE
    // =====================================

    bool CriaturaSigueViva()
    {
        return !criaturaDerrotada &&
               !combateFinalizado &&
               vidaCriatura != null &&
               vidaCriatura.health > 0f;
    }


    IEnumerator CicloCombate()
    {
        while (CriaturaSigueViva())
        {
            // Ventaja inicial para Lucas
            yield return new WaitForSeconds(
                Mathf.Max(0f, tiempoVentajaJugador)
            );

            if (!CriaturaSigueViva())
                yield break;

            // Si Lucas está lejos, la criatura espera.
            // No mueve el cuello ni hace daño.
            while (CriaturaSigueViva() &&
                   !JugadorDentroDelRadio())
            {
                yield return null;
            }

            if (!CriaturaSigueViva())
                yield break;

            Debug.Log(
                "[Criatura3] Lucas dentro del radio. Iniciando ataque."
            );

            // Sonido de ataque al comenzar la embestida
            if (audioAtaque != null && audioAtaque.clip != null)
            {
                audioAtaque.Stop();
                audioAtaque.loop = false;
                audioAtaque.Play();
            }

            // Ejecutar ataque del cuello
            yield return StartCoroutine(
                cuelloCriatura.EjecutarAtaqueCuello(() =>
                {

                    // La cabeza llegó a Lucas: detener sonido de embestida
                    if (audioAtaque != null)
                    {
                        audioAtaque.Stop();
                    }
                    // Verificar nuevamente la distancia
                    // justo cuando la cabeza golpea.
                    if (!CriaturaSigueViva() ||
                        !JugadorDentroDelRadio() ||
                        PlayerHealth.Instance == null)
                    {
                        Debug.Log(
                            "[Criatura3] Ataque fallido: Lucas fuera del radio."
                        );
                        return;
                    }

                    // Sacudir camara al impactar
                    if (rutinaSacudida != null)
                        StopCoroutine(rutinaSacudida);

                    if (pivoteSacudida != null)
                    {
                        rutinaSacudida = StartCoroutine(
                            SacudirCamara()
                        );
                    }

                    float vidaAntes =
                        PlayerHealth.Instance.vidaActual;

                    // Aplicar daño
                    PlayerHealth.Instance.RecibirDanio(
                        danioAtaque
                    );

                    float vidaDespues =
                        PlayerHealth.Instance.vidaActual;

                    float danioReal = Mathf.Max(
                        0f,
                        vidaAntes - vidaDespues
                    );

                    // Volume de contacto
                    danioAcumuladoCriatura3 += danioReal;

                    Debug.Log(
                        "[Criatura3] Golpe confirmado. Daño: " +
                        danioReal +
                        " | Daño acumulado: " +
                        danioAcumuladoCriatura3
                    );
                })
            );

            if (!CriaturaSigueViva())
                yield break;

            // Espera hasta el siguiente ciclo
            yield return new WaitForSeconds(
                Mathf.Max(0f, tiempoEntreAtaques)
            );
        }

        rutinaCombate = null;
    }


    // =====================================
    // VOLUME POR GOLPES A LUCAS
    // =====================================

    void ActualizarVolumeDanioJugador()
    {
        if (volumeDesintegracion == null ||
            !jumpscareIniciado ||
            terminandoDesintegracion)
            return;

        // Porcentaje del daño causado por
        // esta criatura al jugador.
        float porcentajeDanio = Mathf.Clamp01(
            danioAcumuladoCriatura3 /
            Mathf.Max(0.01f, vidaReferenciaVolume)
        );

        float intensidadObjetivo =
            porcentajeDanio *
            intensidadMaximaDesintegracion;

        // Subida progresiva del Volume
        volumeDesintegracion.weight =
            Mathf.MoveTowards(
                volumeDesintegracion.weight,
                intensidadObjetivo,
                Mathf.Max(
                    0f,
                    velocidadCambioDesintegracion
                ) * Time.deltaTime
            );
    }

    // =====================================
    // MUERTE DE LA CRIATURA
    // =====================================

    public void FinalizarCombate()
    {
        // Evitar que quede sonando después del ataque
        if (audioAtaque != null)
        {
            audioAtaque.Stop();
        }
        if (combateFinalizado)
            return;

        combateFinalizado = true;
        criaturaDerrotada = true;
        terminandoDesintegracion = true;

        // Detener ataque futuro
        if (rutinaCombate != null)
        {
            StopCoroutine(rutinaCombate);
            rutinaCombate = null;
        }

        // Detener sonidos de combate
        DetenerAudioCombate();

        // Restaurar Volume gradualmente
        if (volumeDesintegracion != null)
        {
            if (rutinaSalidaDesintegracion != null)
                StopCoroutine(rutinaSalidaDesintegracion);

            rutinaSalidaDesintegracion =
                StartCoroutine(SalirVolumeDesintegracion());
        }

        // Registrar exclusivamente la muerte de la Criatura 3
        if (Act1Manager.Instance != null)
        {
            Act1Manager.Instance.RegistrarMuerteCriatura3();
        }
        else
        {
            Debug.LogWarning(
                "[Criatura3] No se encontró Act1Manager."
            );
        }

        Debug.Log(
            "[Criatura3] Combate finalizado."
        );

        if (cuelloCriatura != null)
        cuelloCriatura.DesactivarSeguimiento();
    
    }

    // =====================================
    // FADE OUT VOLUME AL MATAR CRIATURA
    // =====================================

    IEnumerator SalirVolumeDesintegracion()
    {
        if (volumeDesintegracion == null)
            yield break;

        float intensidadInicial =
            volumeDesintegracion.weight;

        float tiempo = 0f;

        float duracion = Mathf.Max(
            0.01f,
            duracionSalidaDesintegracion
        );

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(tiempo / duracion)
            );

            volumeDesintegracion.weight =
                Mathf.Lerp(
                    intensidadInicial,
                    0f,
                    t
                );

            yield return null;
        }

        volumeDesintegracion.weight = 0f;
        rutinaSalidaDesintegracion = null;
    }



    private bool JugadorDentroDelRadio()
    {
        if (centroAtaque == null || camaraJugador == null)
            return false;

        float distancia = Vector3.Distance(
            centroAtaque.position,
            camaraJugador.transform.position
        );

        return distancia <= radioAtaque;
    }



    private void OnDrawGizmosSelected()
    {
        if (centroAtaque == null)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            centroAtaque.position,
            radioAtaque
        );
    }

}
