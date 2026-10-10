
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

    [Header("Detección")]
    public float esperaInicial = 1f;

    [Range(0.1f, 0.95f)]
    public float toleranciaMirada = 0.75f;

    public float tiempoMirada = 0.15f;

    [Header("Audio")]
    [Header("Audio - Aparición")]
    public AudioSource audioRasgunos;
    public AudioSource audioJumpscare;
    [Header("Duracion Audio Jumpscare")]
    [Min(0f)]
    public float duracionAudioJumpscare = 0.8f; [Header("Fade Out Audio Jumpscare")]
    [Min(0f)]
    public float duracionFadeOutJumpscare = 0.35f;

    [Header("Audio - Combate")]
    public AudioSource baseEnemy1;
    public AudioSource baseEnemy2;
    public AudioSource enemyVoices;

    private bool combateFinalizado = false;

    [Header("Volume del Jumpscare")]
    public Volume volumeJumpscare;
    public float duracionVolume = 0.35f;
    public float tiempoVolumeIntenso = 0.10f;

    [Header("Sacudida de cámara")]
    public Transform pivoteSacudida;
    public float intensidadSacudida = 0.08f;
    public float duracionSacudida = 0.22f;

    private bool activado;
    private bool esperandoMirada;
    private bool jumpscareIniciado;
    private float contadorMirada;

    void Start()
    {
        if (criatura != null)
            criatura.SetActive(false);

        if (volumeJumpscare != null)
            volumeJumpscare.weight = 0f;

        // Preparar audios de combate
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
        if (audio == null) return;

        audio.playOnAwake = false;
        audio.loop = true;
        audio.Stop();
    }

    void Update()
    {
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

    void OnTriggerEnter(Collider other)
    {
        if (activado || !other.CompareTag(tagJugador))
            return;

        activado = true;
        StartCoroutine(IniciarEvento());
    }

    IEnumerator IniciarEvento()
    {
        yield return new WaitForSeconds(esperaInicial);

        if (audioRasgunos != null)
            audioRasgunos.Play();

        esperandoMirada = true;
    }

    IEnumerator EjecutarJumpscare()
    {
        if (criatura == null ||
            cuelloCriatura == null ||
            puntoSpawnCriatura == null)
        {
            Debug.LogError(
                "Faltan referencias para el jumpscare de Criatura 3."
            );
            yield break;
        }

        jumpscareIniciado = true;

        if (audioRasgunos != null)
            audioRasgunos.Stop();

        // Colocar la criatura exactamente en el spawn
        criatura.transform.SetPositionAndRotation(
            puntoSpawnCriatura.position,
            puntoSpawnCriatura.rotation
        );

        criatura.SetActive(true);

        // Iniciar ambiente de combate desde la aparición
        IniciarAudioCombate();

        // El script del cuello marca el impacto y retroceso
        yield return StartCoroutine(
            cuelloCriatura.ReproducirJumpscare(
                ActivarImpacto,
                ComenzarRetroceso
            )
        );

        Debug.Log("Jumpscare finalizado. Iniciar combate.");
    }

    public void IniciarAudioCombate()
    {
        if (combateFinalizado)
            return;

        if (baseEnemy1 != null && !baseEnemy1.isPlaying)
            baseEnemy1.Play();

        if (baseEnemy2 != null && !baseEnemy2.isPlaying)
            baseEnemy2.Play();

        if (enemyVoices != null && !enemyVoices.isPlaying)
            enemyVoices.Play();

        Debug.Log("[Criatura3] Audios de combate iniciados.");
    }

    void ActivarImpacto()
    {
        // Linterna fuera del encuadre
        if (linternaPose != null)
            linternaPose.ActivarPoseJumpscare();

        // Sonido del impacto
        if (audioJumpscare != null)
        {
            audioJumpscare.Stop();
            audioJumpscare.loop = false;
            audioJumpscare.Play();

            StartCoroutine(DetenerAudioJumpscare());
        }

        // Distorsión visual
        if (volumeJumpscare != null)
            StartCoroutine(EfectoVolume());

        // Sacudida
        if (pivoteSacudida != null)
            StartCoroutine(SacudirCamara());
    }



    private IEnumerator DetenerAudioJumpscare()
    {
        if (audioJumpscare == null)
            yield break;

        // Guardamos el volumen original
        float volumenOriginal = audioJumpscare.volume;

        // Esperamos hasta el momento de comenzar el Fade Out
        yield return new WaitForSeconds(
            Mathf.Max(0f, duracionAudioJumpscare)
        );

        float tiempo = 0f;
        float duracion = Mathf.Max(0.01f, duracionFadeOutJumpscare);

        // Disminuimos progresivamente el volumen
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / duracion);

            // Transición suave
            t = Mathf.SmoothStep(0f, 1f, t);

            audioJumpscare.volume = Mathf.Lerp(
                volumenOriginal,
                0f,
                t
            );

            yield return null;
        }

        // Detener y restaurar volumen original
        audioJumpscare.Stop();
        audioJumpscare.volume = volumenOriginal;
    }



    void ComenzarRetroceso()
    {
        if (linternaPose != null)
            linternaPose.RestaurarPoseNormal();
    }

    IEnumerator EfectoVolume()
    {
        volumeJumpscare.weight = 1f;

        yield return new WaitForSeconds(
            Mathf.Max(0f, tiempoVolumeIntenso)
        );

        float tiempo = 0f;
        float duracion = Mathf.Max(
            0.001f, duracionVolume
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




    private IEnumerator SacudirCamara()
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
                Random.insideUnitSphere * intensidadSacudida;

            pivoteSacudida.localPosition =
                posicionOriginal + desplazamiento;

            yield return null;
        }

        pivoteSacudida.localPosition =
            posicionOriginal;
    }

    public void FinalizarCombate()
    {
        if (combateFinalizado)
            return;

        combateFinalizado = true;

        DetenerAudioCombate();

        Debug.Log("[Criatura3] Combate finalizado.");
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
        if (audio == null)
            return;

        audio.Stop();
    }

}
