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

    public void AddParanoia(float cantidad)
    {
        paranoiaActual = Mathf.Clamp(paranoiaActual + cantidad, 0f, paranoiaMaxima);
        Debug.Log($"[ParanoiaSystem] Paranoia actual: {paranoiaActual}");
    }

    public void SetParanoia(float valor)
    {
        paranoiaActual = Mathf.Clamp(valor, 0f, paranoiaMaxima);
    }

    private void ActualizarUI()
    {
        float porcentaje = paranoiaActual / paranoiaMaxima;

        // Opción 1: Imagen tipo Filled
        if (barraParanoiaImage != null)
        {
            barraParanoiaImage.fillAmount = porcentaje;
        }

        // Opción 2: RectTransform escalando en X
        if (barraParanoiaTransform != null)
        {
            Vector3 escala = barraParanoiaTransform.localScale;
            escala.x = porcentaje;
            barraParanoiaTransform.localScale = escala;
        }
    }

    private void ActualizarEfectosVisuales()
    {
        float factor = paranoiaActual / paranoiaMaxima;

        // Aberración Cromática (Progresiva hasta 0.65f)
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = Mathf.Lerp(0f, 0.65f, factor);
        }

        // Distorsión de Lente
        if (lensDistortion != null)
        {
            if (paranoiaActual > 30f)
            {
                float factorDistorsion = (paranoiaActual - 30f) / 70f;
                lensDistortion.intensity.value = Mathf.Lerp(0f, -0.35f, factorDistorsion);
                
                if (paranoiaActual > 70f)
                {
                    float pulsoSuave = Mathf.Sin(Time.time * 2f) * 0.03f;
                    lensDistortion.intensity.value += pulsoSuave;
                }
            }
            else
            {
                lensDistortion.intensity.value = 0f;
            }
        }

        // Viñeta
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(0.2f, 0.52f, factor);
            vignette.smoothness.value = Mathf.Lerp(0.2f, 0.5f, factor);
        }
    }

    private void ActualizarEfectosSonoros()
    {
        float factor = paranoiaActual / paranoiaMaxima;

        // 1. LATIDOS
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

        // 2. RESPIRACIÓN AGITADA Y PRESENTE
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

        // 3. MIXER LOW PASS
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

        // Rangos: 10 a 60 y 80 a 100
        bool enRangoInicial = (paranoiaActual >= 10f && paranoiaActual <= 50f);
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