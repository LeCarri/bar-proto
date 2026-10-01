
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;


public class SombraFugazCocina : MonoBehaviour
{
    [Header("Referencias")]
    public Camera camaraJugador;
    public Transform zonaMirada;
    public Transform puntoA;
    public Transform puntoB;
    public Transform puntoC;
    public GameObject presenciaFugaz;

    [Header("FX - Distorsión de la sombra")]
    public Volume volumenSombra;
    public float velocidadEntradaVolume = 12f;
    public float velocidadSalidaVolume = 3f;

    private Coroutine rutinaVolume;


    [Header("Detección de mirada")]
    [Range(0.01f, 0.5f)]
    public float toleranciaMirada = 0.25f;

    [Min(0f)]
    public float tiempoMirada = 0f;

    [Header("Movimiento")]
    public float duracionAB = 0.18f;
    public float pausaEnB = 0.65f;
    public float duracionBC = 0.12f;

    [Header("Audio")]
    public AudioSource sonidoDesplazamiento;

    [Tooltip("Distribuye la duración del audio entre los tramos A-B y B-C.")]
    public bool ajustarRecorridoAlAudio = true;

    [Header("FX - Humo")]
    public ParticleSystem humoNegro;
    public float tiempoDisipacion = 0.8f;

    [Header("Continuación")]
    public UnityEvent alLlegarAC;

    private bool habilitado;
    private bool ejecutado;
    private float acumuladoMirada;

    private void Start()
    {
        if (presenciaFugaz != null)
            presenciaFugaz.SetActive(false);

        if (sonidoDesplazamiento != null)
            sonidoDesplazamiento.playOnAwake = false;
    }

    private void Update()
    {
        if (!habilitado || ejecutado)
            return;

        if (camaraJugador == null || zonaMirada == null)
            return;

        Vector3 posicionPantalla =
            camaraJugador.WorldToViewportPoint(zonaMirada.position);

        bool estaMirando =
            posicionPantalla.z > 0f &&
            Mathf.Abs(posicionPantalla.x - 0.5f) <= toleranciaMirada &&
            Mathf.Abs(posicionPantalla.y - 0.5f) <= toleranciaMirada;

        if (estaMirando)
            acumuladoMirada += Time.deltaTime;
        else
            acumuladoMirada = 0f;

        if (estaMirando && acumuladoMirada >= tiempoMirada)
        {
            ejecutado = true;
            habilitado = false;

            StartCoroutine(SecuenciaSombra());
        }
    }

    public void HabilitarEvento()
    {
        if (ejecutado || habilitado)
            return;

        habilitado = true;
        acumuladoMirada = 0f;

        Debug.Log("[SombraCocina] Evento habilitado. Esperando mirada.");
    }

    private IEnumerator SecuenciaSombra()
    {
        if (presenciaFugaz == null ||
            puntoA == null || puntoB == null || puntoC == null)
        {
            Debug.LogError("[SombraCocina] Faltan referencias.");
            yield break;
        }

        float tiempoAB = Mathf.Max(0.01f, duracionAB);
        float tiempoBC = Mathf.Max(0.01f, duracionBC);
        float pausa = Mathf.Max(0f, pausaEnB);

        // Si hay audio, repartir su duración entre ambos movimientos.
        if (ajustarRecorridoAlAudio &&
            sonidoDesplazamiento != null &&
            sonidoDesplazamiento.clip != null)
        {
            float pitch = Mathf.Max(
                0.01f,
                Mathf.Abs(sonidoDesplazamiento.pitch)
            );

            float duracionAudio =
                sonidoDesplazamiento.clip.length / pitch;

            float tiempoDisponible =
                Mathf.Max(
                    tiempoAB + tiempoBC,
                    duracionAudio - pausa
                );

            float proporcionAB =
                tiempoAB / (tiempoAB + tiempoBC);

            tiempoAB = tiempoDisponible * proporcionAB;
            tiempoBC = tiempoDisponible * (1f - proporcionAB);
        }

        // A: aparición.
        presenciaFugaz.transform.position = puntoA.position;
        presenciaFugaz.SetActive(true);

        CambiarVolume(1f, velocidadEntradaVolume);


        IniciarHumo(true);

        // Se reproduce una sola vez durante toda la secuencia.
        if (sonidoDesplazamiento != null)
        {
            sonidoDesplazamiento.loop = false;
            sonidoDesplazamiento.Play();
        }

        // A → B
        yield return Mover(
            puntoA.position,
            puntoB.position,
            tiempoAB
        );

        // En B dejamos de emitir, pero no borramos la estela.
        DetenerHumo();
        

        yield return new WaitForSeconds(pausa);

        // B → C: reanudamos la emisión sin limpiar partículas previas.
        presenciaFugaz.transform.position = puntoB.position;

        IniciarHumo(false);

        yield return Mover(
            puntoB.position,
            puntoC.position,
            tiempoBC
        );

        DetenerHumo();


        Debug.Log("[SombraCocina] Llegó a C.");

        // Ocultamos la presencia cuando el humo se haya disipado.
        yield return new WaitForSeconds(
            Mathf.Max(0f, tiempoDisipacion)
        );

        CambiarVolume(0f, velocidadSalidaVolume);

        presenciaFugaz.SetActive(false);

        // El audio no se detiene ni se reinicia.
        // Esperamos a que termine antes de continuar el evento.
        if (sonidoDesplazamiento != null)
        {
            while (sonidoDesplazamiento.isPlaying)
                yield return null;
        }

        Debug.Log("[SombraCocina] Secuencia y audio finalizados.");

        alLlegarAC?.Invoke();
    }

    private IEnumerator Mover(
        Vector3 origen,
        Vector3 destino,
        float duracion)
    {
        float tiempo = 0f;
        duracion = Mathf.Max(0.01f, duracion);

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / duracion);

            presenciaFugaz.transform.position =
                Vector3.Lerp(origen, destino, t);

            yield return null;
        }

        presenciaFugaz.transform.position = destino;
    }

    private void IniciarHumo(bool primeraVez)
    {
        if (humoNegro == null)
            return;

        if (primeraVez)
        {
            humoNegro.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            // Precalentamiento únicamente en A.
            humoNegro.Simulate(0.35f, true, true, true);
        }

        humoNegro.Play(true);
    }

    private void DetenerHumo()
    {
        if (humoNegro == null)
            return;

        humoNegro.Stop(
            true,
            ParticleSystemStopBehavior.StopEmitting
        );
    }

private void CambiarVolume(float objetivo, float velocidad)
{
    if (volumenSombra == null) return;

    if (rutinaVolume != null)
        StopCoroutine(rutinaVolume);

    rutinaVolume = StartCoroutine(
        TransicionarVolume(objetivo, velocidad)
    );
}

private IEnumerator TransicionarVolume(
    float objetivo,
    float velocidad)
{
    while (Mathf.Abs(volumenSombra.weight - objetivo) > 0.01f)
    {
        volumenSombra.weight = Mathf.MoveTowards(
            volumenSombra.weight,
            objetivo,
            velocidad * Time.deltaTime
        );

        yield return null;
    }

    volumenSombra.weight = objetivo;
    rutinaVolume = null;
}

}
