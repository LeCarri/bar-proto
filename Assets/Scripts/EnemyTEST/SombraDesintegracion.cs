using UnityEngine;

public class SombraDesintegracion : MonoBehaviour
{
    [Header("Renderers de la criatura")]
    public Renderer[] renderersSombra;

    [Header("Shader")]
    [Tooltip("Reference exacto de la propiedad Fragmentacion en Shader Graph.")]
    public string propiedadFragmentacion = "_Fragmentacion";

    [Tooltip("Valor normal del shader.")]
    public float fragmentacionInicial = 0.036f;

    [Tooltip("Valor con el que la criatura desaparece completamente.")]
    public float fragmentacionFinal = 1f;

    [Header("Suavizado")]
    public float velocidadCambio = 8f;

    [Header("Partículas de desintegración")]
    public ParticleSystem particulasDesintegracion;

    [Header("Audio de desintegración")]
    public AudioSource audioDesintegracion;

    [Tooltip("Tiempo sin recibir daño antes de cortar el sonido.")]
    public float tiempoParaDetenerAudio = 0.15f;

    [Tooltip("Cuánto tiempo sin recibir daño antes de cortar partículas.")]
    public float tiempoParaDetenerParticulas = 0.12f;

    private MaterialPropertyBlock propertyBlock;

    private float fragmentacionActual;
    private float fragmentacionObjetivo;

    private float ultimoGolpe = -100f;

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

    private void Update()
    {
        // Transición suave del shader
        fragmentacionActual = Mathf.Lerp(
            fragmentacionActual,
            fragmentacionObjetivo,
            Time.deltaTime * velocidadCambio
        );

        AplicarFragmentacion(fragmentacionActual);

        // Si el jugador dejó de quemarla,
        // dejamos de emitir partículas.
        if (particulasDesintegracion != null &&
            particulasDesintegracion.isPlaying &&
            Time.time - ultimoGolpe > tiempoParaDetenerParticulas)
        {
            particulasDesintegracion.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            ActualizarDesintegracion(50f, 100f);
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            ActualizarDesintegracion(0f, 100f);
        }
        
        if (audioDesintegracion != null &&
            audioDesintegracion.isPlaying &&
            Time.time - ultimoGolpe > tiempoParaDetenerAudio)
        {
            audioDesintegracion.Stop();
        }
    }

    public void ActualizarDesintegracion(
        float vidaActual,
        float vidaMaxima)
    {
        if (vidaMaxima <= 0f)
            return;

        float porcentajeVida =
            Mathf.Clamp01(vidaActual / vidaMaxima);

        // 100% vida = fragmentación mínima
        // 0% vida = fragmentación máxima
        float porcentajeDanio = 1f - porcentajeVida;

        fragmentacionObjetivo = Mathf.Lerp(
            fragmentacionInicial,
            fragmentacionFinal,
            porcentajeDanio
        );

        ultimoGolpe = Time.time;

        if (particulasDesintegracion != null &&
            !particulasDesintegracion.isPlaying)
        {
            particulasDesintegracion.Play();
        }

        if (audioDesintegracion != null &&
            !audioDesintegracion.isPlaying)
        {
            audioDesintegracion.loop = true;
            audioDesintegracion.Play();
        }
    }

    public void ForzarDesintegracionCompleta()
    {
        fragmentacionObjetivo = fragmentacionFinal;

        if (particulasDesintegracion != null)
            particulasDesintegracion.Play();
    }

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
}