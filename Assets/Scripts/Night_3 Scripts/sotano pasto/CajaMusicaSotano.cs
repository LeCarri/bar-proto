using System.Collections;
using UnityEngine;

public class CajaMusicalSotano : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource musica;

    [Header("Partes móviles")]
    [SerializeField] private Transform espejo;
    [SerializeField] private Transform bailarina;
    [SerializeField] private Transform llave;

    [Header("Tiempos")]
    [SerializeField] private float tiempoAntesDeApagar = 5f;
    [SerializeField] private float duracionDesvanecimiento = 8f;

    [Header("Volumen")]
    [Range(0f, 1f)]
    [SerializeField] private float volumenInicial = 0.8f;

    [Header("Movimiento")]
    [SerializeField] private float velocidadGiroInicial = 30f;

    [SerializeField] private Vector3 ejeEspejo = new Vector3(0, 1, 0);
    [SerializeField] private Vector3 ejeBailarina = new Vector3(0, 0, 1);
    [SerializeField] private Vector3 ejeLlave = new Vector3(1, 0, 0);

    private bool iniciada;
    private Coroutine secuenciaMusica;

    private void Start()
    {
        if (musica != null)
        {
            musica.playOnAwake = false;
            musica.Stop();
        }
    }

    private void Update()
    {
        if (!iniciada)
            return;

        float multiplicador = 1f;

        // Durante el desvanecimiento, el movimiento pierde velocidad.
        if (secuenciaMusica != null && duracionDesvanecimiento > 0f)
        {
            // El valor se actualiza mediante el progreso de la secuencia.
            multiplicador = multiplicadorMovimiento;
        }

        float velocidad = velocidadGiroInicial * multiplicador;

        if (espejo != null)
            espejo.Rotate(ejeEspejo * velocidad * Time.deltaTime, Space.Self);

        if (bailarina != null)
            bailarina.Rotate(ejeBailarina * velocidad * Time.deltaTime, Space.Self);

        if (llave != null)
            llave.Rotate(ejeLlave * velocidad * Time.deltaTime, Space.Self);
    }

    private float multiplicadorMovimiento = 1f;

    public void IniciarMelodia()
    {
        if (iniciada || musica == null)
            return;

        iniciada = true;
        secuenciaMusica = StartCoroutine(ReproducirMelodia());
    }

    private IEnumerator ReproducirMelodia()
    {
        musica.volume = volumenInicial;
        musica.loop = true;
        musica.Play();

        multiplicadorMovimiento = 1f;

        yield return new WaitForSeconds(tiempoAntesDeApagar);

        float tiempo = 0f;

        while (tiempo < duracionDesvanecimiento)
        {
            tiempo += Time.deltaTime;

            float progreso = Mathf.Clamp01(
                tiempo / duracionDesvanecimiento
            );

            musica.volume = Mathf.Lerp(volumenInicial, 0f, progreso);

            // La caja se va frenando a medida que pierde sonido.
            multiplicadorMovimiento = 1f - progreso;

            yield return null;
        }

        musica.Stop();
        musica.volume = volumenInicial;

        multiplicadorMovimiento = 0f;
        secuenciaMusica = null;

        musica.Stop();
        musica.volume = volumenInicial;

        // Detener el movimiento sin cambiar la posición actual.
        multiplicadorMovimiento = 0f;
        iniciada = false;
        secuenciaMusica = null;
    }

    public void DetenerMelodia()
    {
        if (secuenciaMusica != null)
        {
            StopCoroutine(secuenciaMusica);
            secuenciaMusica = null;
        }

        if (musica != null)
        {
            musica.Stop();
            musica.volume = volumenInicial;
        }

        iniciada = false;
        multiplicadorMovimiento = 1f;
    }
}