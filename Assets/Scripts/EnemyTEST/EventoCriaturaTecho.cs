
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
    public AudioSource audioRasgunos;
    public AudioSource audioJumpscare;

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

        // El script del cuello marca el impacto y retroceso
        yield return StartCoroutine(
            cuelloCriatura.ReproducirJumpscare(
                ActivarImpacto,
                ComenzarRetroceso
            )
        );

        Debug.Log("Jumpscare finalizado. Iniciar combate.");
    }

    void ActivarImpacto()
    {
        // Linterna fuera del encuadre
        if (linternaPose != null)
            linternaPose.ActivarPoseJumpscare();

        // Sonido del impacto
        if (audioJumpscare != null)
            audioJumpscare.Play();

        // Distorsión visual
        if (volumeJumpscare != null)
            StartCoroutine(EfectoVolume());

        // Sacudida
        if (pivoteSacudida != null)
            StartCoroutine(SacudirCamara());
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

    IEnumerator SacudirCamara()
    {
        Vector3 posicionOriginal =
            pivoteSacudida.localPosition;

        float tiempo = 0f;
        float duracion = Mathf.Max(
            0.001f, duracionSacudida
        );

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(
                tiempo / duracion
            );

            float fuerza =
                intensidadSacudida * (1f - progreso);

            Vector3 desplazamiento =
                Random.insideUnitSphere * fuerza;

            pivoteSacudida.localPosition =
                posicionOriginal + desplazamiento;

            yield return null;
        }

        pivoteSacudida.localPosition =
            posicionOriginal;
    }
}
