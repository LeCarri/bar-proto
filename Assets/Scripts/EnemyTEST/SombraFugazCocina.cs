
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SombraFugazCocina : MonoBehaviour
{
    [Header("Referencias")]
    public Camera camaraJugador;
    public Transform zonaMirada;
    public Transform puntoA;
    public Transform puntoB;
    public Transform puntoC;
    public GameObject presenciaFugaz;

    [Header("Detección de mirada")]
    [Range(0.01f, 0.5f)]
    public float toleranciaMirada = 0.18f;

    public float tiempoMirada = 0.15f;

    [Header("Movimiento")]
    public float duracionAB = 0.18f;
    public float pausaEnB = 0.65f;
    public float duracionBC = 0.12f;

    [Header("Audio")]
    public AudioSource sonidoDesplazamiento;

    [Header("Continuación")]
    public UnityEvent alLlegarAC;
    [Header("FX - Humo")]
    public ParticleSystem humoNegro;

    private bool habilitado;
    private bool ejecutado;
    private float acumuladoMirada;

    private void Start()
    {
        if (presenciaFugaz != null)
            presenciaFugaz.SetActive(false);
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
            Mathf.Abs(posicionPantalla.x - 0.5f) < toleranciaMirada &&
            Mathf.Abs(posicionPantalla.y - 0.5f) < toleranciaMirada;

        if (estaMirando)
            acumuladoMirada += Time.deltaTime;
        else
            acumuladoMirada = 0f;

        if (acumuladoMirada >= tiempoMirada)
        {
            ejecutado = true;
            StartCoroutine(SecuenciaSombra());
        }
    }

    public void HabilitarEvento()
    {
        if (ejecutado)
            return;

        habilitado = true;
        acumuladoMirada = 0f;

        Debug.Log("[SombraCocina] Esperando mirada del jugador.");
    }

    
private IEnumerator SecuenciaSombra()
{
    if (presenciaFugaz == null ||
        puntoA == null || puntoB == null || puntoC == null)
    {
        Debug.LogError("[SombraCocina] Faltan referencias.");
        yield break;
    }

    // PRIMER CRUCE: A → B
    presenciaFugaz.transform.position = puntoA.position;
    presenciaFugaz.SetActive(true);

    IniciarHumo();

    if (sonidoDesplazamiento != null)
        sonidoDesplazamiento.Play();

    yield return Mover(
        puntoA.position,
        puntoB.position,
        duracionAB
    );

    // La presencia deja de emitir, pero conserva su estela.
    DetenerHumo();

    yield return new WaitForSeconds(pausaEnB);

    // SEGUNDO CRUCE: B → C
    presenciaFugaz.transform.position = puntoB.position;

    IniciarHumo();

    yield return Mover(
        puntoB.position,
        puntoC.position,
        duracionBC
    );

    DetenerHumo();

    // Dar tiempo a que se disipe el humo restante.
    yield return new WaitForSeconds(0.8f);

    presenciaFugaz.SetActive(false);

    Debug.Log("[SombraCocina] Llegó a C.");

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

    private void IniciarHumo()
    {
        if (humoNegro == null) return;

        humoNegro.Clear(true);
        humoNegro.Play(true);
    }

    private void DetenerHumo()
    {
        if (humoNegro == null) return;

        // Detiene la emisión, pero deja que el humo existente se desvanezca.
        humoNegro.Stop(
            true,
            ParticleSystemStopBehavior.StopEmitting
        );
    }

}
