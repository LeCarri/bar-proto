using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ParanoiaSystem : MonoBehaviour
{
    public static ParanoiaSystem Instance { get; private set; }

    [Header("Valores de Paranoia")]
    [Range(0, 100)]
    public float paranoiaActual = 0f;
    public float paranoiaMaxima = 100f;

    [Header("Interfaz de Usuario (UI)")]
    public Image barraParanoiaImage;             // Si usás Image Type = Filled
    public RectTransform barraParanoiaTransform; // Si modificás la escala en X

    [Header("Configuración de URP Post Processing")]
    public Volume globalVolume;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private Vignette vignette;
    private FilmGrain filmGrain;
    private DepthOfField depthOfField; // <-- Agregado para el toque al 100%

    [Header("Efectos de Audio de Estado")]
    public AudioSource audioLatidos;
    public AudioSource audioRespiracion;
    public AudioMixer audioMixer;
    
    [Header("Alucinaciones Auditivas")]
    public AudioSource audioAlucinaciones;
    public AudioClip[] clipsSusurros;
    public Transform transformJugador;
    
    [Header("Ajustes de Susurros")]
    [Range(0.1f, 1f)] public float volumenMaxSusurros = 0.45f;
    public float intervaloMinAlucinacion = 6f;
    public float intervaloMaxAlucinacion = 12f;
    private float tiempoProximaAlucinacion;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out chromaticAberration);
            globalVolume.profile.TryGet(out lensDistortion);
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out filmGrain);
            globalVolume.profile.TryGet(out depthOfField);
        }

        tiempoProximaAlucinacion = Time.time + Random.Range(intervaloMinAlucinacion, intervaloMaxAlucinacion);
    }

    private void Update()
    {
        ActualizarUI();
        ActualizarEfectosVisuales();
        ActualizarEfectosSonoros();
        GestionarAlucinaciones();
    }

    /// <summary>
    /// Suma paranoia. Si algún script intenta mandar un valor negativo, se ignora.
    /// </summary>
    public void AddParanoia(float cantidad)
    {
        if (cantidad <= 0f) return; // Bloquea reducciones no deseadas al matar sombras individualmente

        paranoiaActual = Mathf.Clamp(paranoiaActual + cantidad, 0f, paranoiaMaxima);
        Debug.Log($"[ParanoiaSystem] Paranoia aumentada a: {paranoiaActual}");
    }

    /// <summary>
    /// Usar SOLO para la mecánica final donde se limpian todas las sombras.
    /// </summary>
    public void ResetParanoia()
    {
        paranoiaActual = 0f;
        Debug.Log("[ParanoiaSystem] Paranoia reseteada a 0.");
    }

    public void SetParanoia(float valor)
    {
        paranoiaActual = Mathf.Clamp(valor, 0f, paranoiaMaxima);
    }

    private void ActualizarUI()
    {
        float porcentaje = paranoiaActual / paranoiaMaxima;

        if (barraParanoiaImage != null)
        {
            barraParanoiaImage.fillAmount = porcentaje;
        }

        if (barraParanoiaTransform != null)
        {
            Vector3 escala = barraParanoiaTransform.localScale;
            escala.x = porcentaje;
            barraParanoiaTransform.localScale = escala;
        }
    }

    private void ActualizarEfectosVisuales()
    {
        // Debajo de 50: Vista nítida/limpia
        if (paranoiaActual < 50f)
        {
            if (chromaticAberration != null) chromaticAberration.intensity.value = 0f;
            if (lensDistortion != null) lensDistortion.intensity.value = 0f;
            if (vignette != null) vignette.intensity.value = 0.18f;
            if (filmGrain != null) filmGrain.intensity.value = 0f;
            if (depthOfField != null) depthOfField.focusDistance.value = 10f;
            return;
        }

        // Escalado para el rango 50 a 100
        float factorEfectos = (paranoiaActual - 50f) / 50f;

        // 1. ABERRACIÓN CROMÁTICA
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = Mathf.Lerp(0.70f, 1.0f, factorEfectos);
        }

        // 2. DISTORSIÓN DE LENTE
        if (lensDistortion != null)
        {
            float baseDistorsion = Mathf.Lerp(-0.25f, -0.65f, factorEfectos);
            
            if (paranoiaActual >= 70f)
            {
                float pulsoMareo = Mathf.Sin(Time.time * 2.5f) * 0.05f;
                baseDistorsion += pulsoMareo;
            }

            lensDistortion.intensity.value = baseDistorsion;
        }

        // 3. VIÑETA
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(0.45f, 0.75f, factorEfectos);
            vignette.smoothness.value = Mathf.Lerp(0.40f, 0.80f, factorEfectos);
        }

        // 4. GRANO DE PELÍCULA
        if (filmGrain != null)
        {
            filmGrain.intensity.value = Mathf.Lerp(0.35f, 0.75f, factorEfectos);
        }

        // 5. DESENFOQUE SUTIL AL 100% (Súper tenue: de 10f a 4.5f para no cegar)
        if (depthOfField != null)
        {
            if (paranoiaActual >= 90f)
            {
                float factorDesenfoque = (paranoiaActual - 90f) / 10f;
                depthOfField.focusDistance.value = Mathf.Lerp(10f, 4.5f, factorDesenfoque);
            }
            else
            {
                depthOfField.focusDistance.value = 10f;
            }
        }
    }

    private void ActualizarEfectosSonoros()
    {
        float factor = paranoiaActual / paranoiaMaxima;

        // LATIDOS
        if (audioLatidos != null)
        {
            if (!audioLatidos.isPlaying && paranoiaActual > 10f) audioLatidos.Play();
            
            if (paranoiaActual > 10f)
            {
                audioLatidos.volume = Mathf.Lerp(0.35f, 1f, factor);
            }
            else
            {
                audioLatidos.volume = 0f;
            }
        }

        // RESPIRACIÓN
        if (audioRespiracion != null)
        {
            if (paranoiaActual > 25f)
            {
                if (!audioRespiracion.isPlaying) audioRespiracion.Play();

                float factorRespiracion = (paranoiaActual - 25f) / 75f; 
                audioRespiracion.volume = Mathf.Lerp(0.65f, 1.0f, Mathf.Pow(factorRespiracion, 0.5f));
                audioRespiracion.pitch = Mathf.Lerp(1.0f, 1.35f, factorRespiracion);
            }
            else
            {
                if (audioRespiracion.isPlaying) audioRespiracion.Stop();
            }
        }

        // MIXER LOW PASS
        if (audioMixer != null)
        {
            if (paranoiaActual >= 60f)
            {
                float corteFrecuencia = Mathf.Lerp(22000f, 1800f, (paranoiaActual - 60f) / 40f);
                audioMixer.SetFloat("EntornoLowPass", corteFrecuencia);
            }
            else
            {
                audioMixer.SetFloat("EntornoLowPass", 22000f);
            }
        }
    }

    private void GestionarAlucinaciones()
    {
        if (clipsSusurros == null || clipsSusurros.Length == 0) return;

        bool enRangoInicial = (paranoiaActual >= 10f && paranoiaActual <= 60f);
        bool enRangoCritico = (paranoiaActual >= 80f && paranoiaActual <= 100f);

        if (!enRangoInicial && !enRangoCritico) return;

        if (Time.time >= tiempoProximaAlucinacion)
        {
            ReproducirSusurro3D();

            float multiplicadorFrecuencia = enRangoCritico ? 0.6f : 1.0f;
            tiempoProximaAlucinacion = Time.time + (Random.Range(intervaloMinAlucinacion, intervaloMaxAlucinacion) * multiplicadorFrecuencia);
        }
    }

    private void ReproducirSusurro3D()
    {
        if (audioAlucinaciones == null || transformJugador == null) return;

        Vector3 posicionAleatoria = transformJugador.position + (Random.insideUnitSphere * 3.5f);
        posicionAleatoria.y = transformJugador.position.y + 1.2f;

        audioAlucinaciones.transform.position = posicionAleatoria;
        AudioClip clipElegido = clipsSusurros[Random.Range(0, clipsSusurros.Length)];

        float factorParanoia = paranoiaActual / paranoiaMaxima;
        float volumenCalculado = Mathf.Lerp(0.15f, volumenMaxSusurros, factorParanoia);

        audioAlucinaciones.PlayOneShot(clipElegido, volumenCalculado);
    }
}