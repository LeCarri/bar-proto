using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ParanoiaSystem : MonoBehaviour
{
    public static ParanoiaSystem Instance { get; private set; }

    [SerializeField]
    private Image paranoiaBarImage;

    private float paranoiaBarFillAmmount;
    private float paranoia = 0f;

    float lerpTime = 5f; // Tiempo que tarda la "animacion"
    float currentLerpTime;

    [Header("Feedback Sensorial")]
    public AudioSource heartBeatAudio; // Arrastrá un AudioSource con un loop de latido
    public float paranoiaUmbralEfectos = 40f; // A qué nivel empiezan los efectos

    [Header("Configuración Estática")]
    public RawImage vignetteRawImage;
    public float opacidadMaxima = 1f;
    public float umbralParanoiaActiva = 40f;
    public float velocidadEstaticaX = 0.5f;
    public float velocidadEstaticaY = 0.5f;
    private Vector2 offsetActual;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Asegurarnos que empiece invisible
        if (vignetteRawImage != null)
        {
            Color c = vignetteRawImage.color;
            c.a = 0f;
            vignetteRawImage.color = c;
        }
    }

    private void Update()
    {
        paranoiaBarFillAmmount = paranoiaBarImage.fillAmount;

        if (currentLerpTime < lerpTime)
            currentLerpTime += Time.deltaTime;

        if (currentLerpTime > lerpTime)
            currentLerpTime = lerpTime;

        InterpolateBar();

        ActualizarEfectosSensoriales();
    }

    void InterpolateBar()
    {
        float t = currentLerpTime / lerpTime;
        t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ecuación que da un efecto de Ease Out
        paranoiaBarImage.fillAmount = Mathf.Lerp(paranoiaBarFillAmmount, paranoia / 100, t);
    }

    public void AddParanoia(float value)
    {
        currentLerpTime = 0;
        paranoia = Mathf.Clamp(paranoia + value, 0f, 100f);
    }

    public float GetParanoia() => paranoia;

    void ActualizarEfectosSensoriales()
    {
        if (heartBeatAudio != null)
        {
            if (paranoia > paranoiaUmbralEfectos)
            {
                if (!heartBeatAudio.isPlaying) heartBeatAudio.Play();

                // Calculamos un factor de 0 a 1 basado en la paranoia (de 40 a 100)
                float factor = (paranoia - paranoiaUmbralEfectos) / (100f - paranoiaUmbralEfectos);

                heartBeatAudio.volume = Mathf.Lerp(0.1f, 1f, factor);
                heartBeatAudio.pitch = Mathf.Lerp(1f, 1.8f, factor); // El latido se acelera
            }
            else
            {
                heartBeatAudio.Stop();
            }
        }

        if (vignetteRawImage != null)
        {
            float nuevoAlpha = (paranoia >= umbralParanoiaActiva) ? opacidadMaxima : 0f;

            Color c = vignetteRawImage.color;

            // Si querés que sea INSTANTÁNEO, usá esta línea:
            c.a = nuevoAlpha;

            // Si querés que tenga un "parpadeo" rápido al activarse:
            // c.a = Mathf.Lerp(c.a, nuevoAlpha, Time.deltaTime * 20f);

            vignetteRawImage.color = c;
        }
    }

    void AnimarEstatica()
    {
        if (vignetteRawImage != null && paranoia > 15f) // Solo si se debe ver
        {
            // Calculamos el nuevo offset basado en el tiempo
            offsetActual.x += velocidadEstaticaX * Time.deltaTime;
            offsetActual.y += velocidadEstaticaY * Time.deltaTime;

            // Para evitar que el offset crezca infinitamente (buena práctica)
            if (offsetActual.x > 1f) offsetActual.x -= 1f;
            if (offsetActual.y > 1f) offsetActual.y -= 1f;

            // Aplicamos el nuevo offset al UV Rect de la Raw Image
            // El Rect es (X, Y, Ancho, Alto). Mantenemos Ancho/Alto en 1
            vignetteRawImage.uvRect = new Rect(offsetActual.x, offsetActual.y, 1f, 1f);
        }
    }
}