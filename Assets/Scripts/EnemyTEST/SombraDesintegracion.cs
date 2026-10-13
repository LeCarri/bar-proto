
using UnityEngine;

public class SombraDesintegracion : MonoBehaviour
{
    [Header("Renderers de la criatura")]
    public Renderer[] renderersSombra;

    [Header("Shader")]
    [Tooltip("Nombre exacto de la propiedad del Shader Graph.")]
    public string propiedadFragmentacion = "_Fragmentacion";

    [Tooltip("Valor normal del shader.")]
    public float fragmentacionInicial = 0.036f;

    [Tooltip("Valor de desintegracion total al morir.")]
    public float fragmentacionFinal = 1f;

    [Header("Fragmentacion mientras sigue viva")]
    [Range(0f, 1f)]
    [Tooltip("Maximo valor de fragmentacion antes de morir.")]
    public float fragmentacionMaximaConVida = 0.40f;

    [Header("Velocidades")]
    [Min(0f)]
    public float velocidadCambio = 8f;

    [Min(0f)]
    [Tooltip("Velocidad a la que se vuelve visible.")]
    public float velocidadReaparicion = 1.5f;

    [Header("Reaparicion")]
    [Min(0f)]
    [Tooltip("Tiempo sin recibir dano antes de reconstruirse.")]
    public float tiempoAntesReaparecer = 0.30f;

    [Header("Particulas de desintegracion")]
    public ParticleSystem particulasDesintegracion;

    [Header("Audio de desintegracion")]
    public AudioSource audioDesintegracion;

    [Tooltip("Tiempo sin recibir dano antes de cortar sonido.")]
    public float tiempoParaDetenerAudio = 0.15f;

    [Tooltip("Tiempo sin recibir dano antes de cortar particulas.")]
    public float tiempoParaDetenerParticulas = 0.12f;

    private MaterialPropertyBlock propertyBlock;

    private float fragmentacionActual;
    private float fragmentacionObjetivo;

    private float ultimoGolpe = -100f;

    // Impide reconstruirse despues de morir.
    private bool muriendo = false;

    // =====================================
    // AWAKE
    // =====================================

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();

        fragmentacionActual = fragmentacionInicial;
        fragmentacionObjetivo = fragmentacionInicial;

        AplicarFragmentacion(fragmentacionInicial);

        if (particulasDesintegracion != null)
        {
            particulasDesintegracion.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }

    // =====================================
    // UPDATE
    // =====================================

    private void Update()
    {
        float tiempoSinDanio =
            Time.time - ultimoGolpe;

        // Si sigue viva y Lucas dejo de iluminarla,
        // comienza a reconstruirse visualmente.
        if (!muriendo &&
            tiempoSinDanio >= tiempoAntesReaparecer)
        {
            fragmentacionObjetivo =
                fragmentacionInicial;
        }

        // Mientras recibe dano se fragmenta rapido.
        // Cuando deja de recibirlo vuelve gradualmente.
        float velocidad = velocidadCambio;

        if (!muriendo &&
            tiempoSinDanio >= tiempoAntesReaparecer)
        {
            velocidad = velocidadReaparicion;
        }

        fragmentacionActual = Mathf.MoveTowards(
            fragmentacionActual,
            fragmentacionObjetivo,
            Mathf.Max(0f, velocidad) * Time.deltaTime
        );

        AplicarFragmentacion(fragmentacionActual);

        // =====================================
        // PARTICULAS
        // =====================================

        if (!muriendo &&
            particulasDesintegracion != null &&
            particulasDesintegracion.isPlaying &&
            tiempoSinDanio > tiempoParaDetenerParticulas)
        {
            particulasDesintegracion.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );
        }

        // =====================================
        // AUDIO
        // =====================================

        if (audioDesintegracion != null &&
            audioDesintegracion.isPlaying &&
            tiempoSinDanio > tiempoParaDetenerAudio)
        {
            DetenerAudioDesintegracion();
        }
    }

    // =====================================
    // RECIBIR DANO
    // =====================================

    public void ActualizarDesintegracion(
        float vidaActual,
        float vidaMaxima)
    {
        if (muriendo || vidaMaxima <= 0f)
            return;

        float porcentajeVida = Mathf.Clamp01(
            vidaActual / vidaMaxima
        );

        float porcentajeDanio =
            1f - porcentajeVida;

        // Nunca permitir la fragmentacion total
        // mientras el enemigo tenga vida.
        float limiteConVida = Mathf.Clamp(
            fragmentacionMaximaConVida,
            fragmentacionInicial,
            fragmentacionFinal
        );

        fragmentacionObjetivo = Mathf.Lerp(
            fragmentacionInicial,
            limiteConVida,
            porcentajeDanio
        );

        ultimoGolpe = Time.time;

        // Particulas existentes
        if (particulasDesintegracion != null &&
            !particulasDesintegracion.isPlaying)
        {
            particulasDesintegracion.Play();
        }

        // Audio existente
        if (audioDesintegracion != null &&
            !audioDesintegracion.isPlaying)
        {
            audioDesintegracion.loop = true;
            audioDesintegracion.Play();
        }
    }

    // =====================================
    // MUERTE DEFINITIVA
    // =====================================

    public void ForzarDesintegracionCompleta()
    {
        if (muriendo)
            return;

        muriendo = true;

        // La criatura ya no puede reconstruirse.
        fragmentacionObjetivo = fragmentacionFinal;
        fragmentacionActual = fragmentacionFinal;

        AplicarFragmentacion(fragmentacionActual);

        if (particulasDesintegracion != null)
        {
            particulasDesintegracion.Play();
        }

        DetenerAudioDesintegracion();

        Debug.Log(
            "[SombraDesintegracion] Desintegracion definitiva."
        );
    }

    // =====================================
    // APLICAR SHADER
    // =====================================

    private void AplicarFragmentacion(float valor)
    {
        if (renderersSombra == null)
            return;

        foreach (Renderer rend in renderersSombra)
        {
            if (rend == null)
                continue;

            rend.GetPropertyBlock(propertyBlock);

            propertyBlock.SetFloat(
                propiedadFragmentacion,
                valor
            );

            rend.SetPropertyBlock(propertyBlock);
        }
    }

    // =====================================
    // AUDIO
    // =====================================

    public void DetenerAudioDesintegracion()
    {
        if (audioDesintegracion == null)
            return;

        audioDesintegracion.Stop();
        audioDesintegracion.loop = false;
    }

    private void OnDisable()
    {
        DetenerAudioDesintegracion();
    }

    private void OnDestroy()
    {
        DetenerAudioDesintegracion();
    }
}
