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

    [Header("Audio de muerte - Enemy Kill")]
    [Tooltip("Arrastra aqui el AudioSource Enemy Kill, con su clip asignado. Se reproducira una sola vez en 2D, aunque el enemigo se desactive.")]
    public AudioSource audioEnemyKill;

    private MaterialPropertyBlock propertyBlock;
    private float fragmentacionActual;
    private float fragmentacionObjetivo;
    private float ultimoGolpe = -100f;
    private bool muriendo = false;

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

        // Este AudioSource sirve de referencia/configuracion; no debe sonar por si solo.
        if (audioEnemyKill != null)
            audioEnemyKill.playOnAwake = false;
    }

    private void Update()
    {
        float tiempoSinDanio = Time.time - ultimoGolpe;

        if (!muriendo && tiempoSinDanio >= tiempoAntesReaparecer)
            fragmentacionObjetivo = fragmentacionInicial;

        float velocidad = velocidadCambio;
        if (!muriendo && tiempoSinDanio >= tiempoAntesReaparecer)
            velocidad = velocidadReaparicion;

        fragmentacionActual = Mathf.MoveTowards(
            fragmentacionActual,
            fragmentacionObjetivo,
            Mathf.Max(0f, velocidad) * Time.deltaTime
        );
        AplicarFragmentacion(fragmentacionActual);

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

        if (audioDesintegracion != null &&
            audioDesintegracion.isPlaying &&
            tiempoSinDanio > tiempoParaDetenerAudio)
        {
            DetenerAudioDesintegracion();
        }
    }

    public void ActualizarDesintegracion(float vidaActual, float vidaMaxima)
    {
        if (muriendo || vidaMaxima <= 0f)
            return;

        float porcentajeVida = Mathf.Clamp01(vidaActual / vidaMaxima);
        float porcentajeDanio = 1f - porcentajeVida;
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

        if (particulasDesintegracion != null && !particulasDesintegracion.isPlaying)
            particulasDesintegracion.Play();

        if (audioDesintegracion != null && !audioDesintegracion.isPlaying)
        {
            audioDesintegracion.loop = true;
            audioDesintegracion.Play();
        }
    }

    public void ForzarDesintegracionCompleta()
    {
        if (muriendo)
            return;

        muriendo = true;
        fragmentacionObjetivo = fragmentacionFinal;
        fragmentacionActual = fragmentacionFinal;
        AplicarFragmentacion(fragmentacionActual);

        if (particulasDesintegracion != null)
            particulasDesintegracion.Play();

        DetenerAudioDesintegracion();

        // NUEVO: reproduce el sonido de muerte fuera de la jerarquia del enemigo.
        ReproducirEnemyKill();

        Debug.Log("[SombraDesintegracion] Desintegracion definitiva.", this);
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
            propertyBlock.SetFloat(propiedadFragmentacion, valor);
            rend.SetPropertyBlock(propertyBlock);
        }
    }

    public void DetenerAudioDesintegracion()
    {
        if (audioDesintegracion == null)
            return;

        audioDesintegracion.Stop();
        audioDesintegracion.loop = false;
    }

    private void ReproducirEnemyKill()
    {
        if (audioEnemyKill == null || audioEnemyKill.clip == null)
        {
            Debug.LogWarning(
                "[SombraDesintegracion] Falta asignar Audio Enemy Kill o su AudioClip.",
                this
            );
            return;
        }

        // No es hijo del enemigo: el clip puede terminar incluso si EnemyCore
        // desactiva o destruye al enemigo inmediatamente al morir.
        GameObject sonidoTemporal = new GameObject("Enemy Kill (audio temporal)");
        AudioSource reproductor = sonidoTemporal.AddComponent<AudioSource>();
        reproductor.playOnAwake = false;
        reproductor.clip = audioEnemyKill.clip;
        reproductor.volume = audioEnemyKill.volume;
        reproductor.pitch = audioEnemyKill.pitch;
        reproductor.outputAudioMixerGroup = audioEnemyKill.outputAudioMixerGroup;
        reproductor.spatialBlend = 0f; // 2D: se escucha independientemente de la posicion.
        reproductor.loop = false;
        reproductor.Play();

        float duracion = reproductor.clip.length /
                         Mathf.Max(0.01f, Mathf.Abs(reproductor.pitch));
        Destroy(sonidoTemporal, duracion + 0.2f);

        Debug.Log("[SombraDesintegracion] Reproduciendo Enemy Kill en 2D.", this);
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
