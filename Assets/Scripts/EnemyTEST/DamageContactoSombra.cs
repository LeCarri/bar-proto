using UnityEngine;
using UnityEngine.Rendering;

public class DamageContactoSombra : MonoBehaviour
{
    [Header("Daño")]
    [Tooltip("Daño que recibe el jugador por segundo mientras permanece en contacto.")]
    public float damagePorSegundo = 25f;

    [Tooltip("Pequeño tiempo antes de empezar a hacer daño real.")]
    public float tiempoAntesDeDanio = 0.4f;


    [Header("Paranoia")]
    [Tooltip("Paranoia que aumenta por segundo durante el contacto.")]
    public float paranoiaPorSegundo = 12f;


    [Header("Volume de contacto")]
    public Volume volumenContacto;

    [Range(0f, 1f)]
    public float pesoMaximoVolume = 1f;

    [Tooltip("Qué tan rápido entra el efecto cuando la Sombra toca al jugador.")]
    public float velocidadEntradaVolume = 0.6f;

    [Tooltip("Qué tan rápido desaparece al escapar.")]
    public float velocidadSalidaVolume = 1.5f;


    [Header("Audio")]
    public AudioSource audioContacto;

    [Range(0f, 1f)]
    public float volumenAudioMaximo = 1f;

    [Tooltip("Velocidad con la que aumenta el audio de contacto.")]
    public float velocidadAudio = 1.5f;


    [Header("Debug")]
    public bool mostrarDebug = true;


    private PlayerHealth jugadorHealth;

    private bool jugadorEnContacto = false;

    private float tiempoContacto = 0f;

    // Sirve por si el Player tiene más de un Collider.
    private int collidersJugadorDentro = 0;


    private void Start()
    {
        if (volumenContacto != null)
            volumenContacto.weight = 0f;

        if (audioContacto != null)
        {
            audioContacto.playOnAwake = false;
            audioContacto.loop = true;
            audioContacto.volume = 0f;
            audioContacto.Stop();
        }
    }


    private void Update()
    {
        ActualizarVolume();
        ActualizarAudio();

        if (!jugadorEnContacto)
        {
            tiempoContacto = 0f;
            return;
        }


        tiempoContacto += Time.deltaTime;


        // Todavía estamos en el pequeño período de gracia.
        if (tiempoContacto < tiempoAntesDeDanio)
            return;


        // ==========================================
        // DAÑO CONTINUO
        // ==========================================

        if (jugadorHealth != null)
        {
            float damageFrame =
                damagePorSegundo * Time.deltaTime;

            jugadorHealth.RecibirDanio(damageFrame);
        }


        // ==========================================
        // PARANOIA CONTINUA
        // ==========================================

        if (ParanoiaSystem.Instance != null)
        {
            float paranoiaFrame =
                paranoiaPorSegundo * Time.deltaTime;

            ParanoiaSystem.Instance.AddParanoia(
                paranoiaFrame
            );
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health == null)
            return;


        collidersJugadorDentro++;

        jugadorHealth = health;

        if (!jugadorEnContacto)
        {
            jugadorEnContacto = true;
            tiempoContacto = 0f;

            if (mostrarDebug)
            {
                Debug.Log(
                    "[DamageContactoSombra] Jugador entró en contacto."
                );
            }
        }


        if (audioContacto != null &&
            !audioContacto.isPlaying)
        {
            audioContacto.volume = 0f;
            audioContacto.Play();
        }
    }


    private void OnTriggerExit(Collider other)
    {
        PlayerHealth health =
            other.GetComponentInParent<PlayerHealth>();

        if (health == null)
            return;


        collidersJugadorDentro--;

        if (collidersJugadorDentro > 0)
            return;


        collidersJugadorDentro = 0;

        jugadorEnContacto = false;
        tiempoContacto = 0f;
        jugadorHealth = null;


        if (mostrarDebug)
        {
            Debug.Log(
                "[DamageContactoSombra] Jugador salió del contacto."
            );
        }
    }


    private void ActualizarVolume()
    {
        if (volumenContacto == null)
            return;


        float objetivo =
            jugadorEnContacto
            ? pesoMaximoVolume
            : 0f;


        float velocidad =
            jugadorEnContacto
            ? velocidadEntradaVolume
            : velocidadSalidaVolume;


        volumenContacto.weight =
            Mathf.MoveTowards(
                volumenContacto.weight,
                objetivo,
                velocidad * Time.deltaTime
            );
    }


    private void ActualizarAudio()
    {
        if (audioContacto == null)
            return;


        float objetivo =
            jugadorEnContacto
            ? volumenAudioMaximo
            : 0f;


        audioContacto.volume =
            Mathf.MoveTowards(
                audioContacto.volume,
                objetivo,
                velocidadAudio * Time.deltaTime
            );


        // Cuando terminó de bajar, lo apagamos.
        if (!jugadorEnContacto &&
            audioContacto.volume <= 0.01f &&
            audioContacto.isPlaying)
        {
            audioContacto.Stop();
        }
    }


    private void OnDisable()
    {
        jugadorEnContacto = false;
        tiempoContacto = 0f;
        collidersJugadorDentro = 0;
        jugadorHealth = null;


        if (volumenContacto != null)
            volumenContacto.weight = 0f;


        if (audioContacto != null)
        {
            audioContacto.Stop();
            audioContacto.volume = 0f;
        }
    }
}