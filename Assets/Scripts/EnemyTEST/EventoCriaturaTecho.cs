
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

    [Header("Combate - Criatura 3")]
    public float tiempoVentajaJugador = 3f;
    public float tiempoEntreAtaques = 2f;

    [Header("Ataque")]
    public float danioAtaque = 35f;

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

    // Vida del enemigo
    private EnemyCore vidaCriatura;

    // Vida del jugador
    private float vidaMaximaJugador = 100f;
    private float vidaReferenciaJugador = 100f;

    // Corrutinas
    private Coroutine rutinaSalidaDesintegracion;
    private Coroutine rutinaAudioJumpscare;
    private Coroutine rutinaSacudida;

    // =====================================
    // START
    // =====================================

    void Start()
    {
        if (criatura != null)
        {
            vidaCriatura =
                criatura.GetComponent<EnemyCore>();

            if (vidaCriatura == null)
                vidaCriatura =
                    criatura.GetComponentInChildren<EnemyCore>();

            criatura.SetActive(false);
        }

        // Guardamos la vida del jugador
        if (PlayerHealth.Instance != null)
        {
            vidaMaximaJugador = Mathf.Max(
                1f,
                PlayerHealth.Instance.vidaActual
            );
        }

        vidaReferenciaJugador = vidaMaximaJugador;

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
        ActualizarVolumeDanioJugador();

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
        if (activado ||
            !other.CompareTag(tagJugador))
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
    // APARICION
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

        criatura.transform.SetPositionAndRotation(
            puntoSpawnCriatura.position,
            puntoSpawnCriatura.rotation
        );

        criatura.SetActive(true);

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

        // La referencia de vida se fija al
        // comenzar el encuentro.
        if (PlayerHealth.Instance != null)
        {
            vidaReferenciaJugador = Mathf.Max(
                1f,
                PlayerHealth.Instance.vidaActual
            );
        }

        IniciarAudioCombate();

        yield return StartCoroutine(
            cuelloCriatura.ReproducirJumpscare(
                ActivarImpacto,
                ComenzarRetroceso
            )
        );

        if (CriaturaSigueViva())
            StartCoroutine(CicloCombate());

        Debug.Log(
            "[Criatura3] Jumpscare finalizado."
        );
    }

    // =====================================
    // IMPACTO
    // =====================================

    void ActivarImpacto()
    {
        if (linternaPose != null)
            linternaPose.ActivarPoseJumpscare();

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

        if (volumeJumpscare != null)
            StartCoroutine(EfectoVolume());

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
            baseEnemy1.Play();

        if (baseEnemy2 != null &&
            !baseEnemy2.isPlaying)
            baseEnemy2.Play();

        if (enemyVoices != null &&
            !enemyVoices.isPlaying)
            enemyVoices.Play();
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
               vidaCriatura != null &&
               vidaCriatura.health > 0f;
    }

    IEnumerator CicloCombate()
    {
        while (CriaturaSigueViva())
        {
            yield return new WaitForSeconds(
                Mathf.Max(0f, tiempoVentajaJugador)
            );

            if (!CriaturaSigueViva())
                yield break;

            yield return StartCoroutine(
                cuelloCriatura.EjecutarAtaqueCuello(() =>
                {
                    if (CriaturaSigueViva() &&
                        PlayerHealth.Instance != null)
                    {
                        PlayerHealth.Instance.RecibirDanio(
                            danioAtaque
                        );
                    }
                })
            );

            if (!CriaturaSigueViva())
                yield break;

            yield return new WaitForSeconds(
                Mathf.Max(0f, tiempoEntreAtaques)
            );
        }
    }

    // =====================================
    // VOLUME POR DANIO RECIBIDO POR LUCAS
    // =====================================

    void ActualizarVolumeDanioJugador()
    {
        if (volumeDesintegracion == null ||
            PlayerHealth.Instance == null ||
            !jumpscareIniciado ||
            terminandoDesintegracion)
            return;

        float vidaActual =
            PlayerHealth.Instance.vidaActual;

        float porcentajeVida = Mathf.Clamp01(
            vidaActual /
            Mathf.Max(1f, vidaReferenciaJugador)
        );

        float porcentajeDanio =
            1f - porcentajeVida;

        float intensidadObjetivo =
            porcentajeDanio *
            intensidadMaximaDesintegracion;

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
        if (combateFinalizado)
            return;

        combateFinalizado = true;
        criaturaDerrotada = true;
        terminandoDesintegracion = true;

        DetenerAudioCombate();

        if (volumeDesintegracion != null)
        {
            if (rutinaSalidaDesintegracion != null)
                StopCoroutine(rutinaSalidaDesintegracion);

            rutinaSalidaDesintegracion =
                StartCoroutine(SalirVolumeDesintegracion());
        }

        Debug.Log(
            "[Criatura3] Combate finalizado."
        );
    }

    // =====================================
    // FADE OUT VOLUME
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
}
