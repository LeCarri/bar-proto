using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [SerializeField]
    private CinemachineCamera cineCam;

    private CinemachineBasicMultiChannelPerlin noise;
    private Coroutine corrutinaShakeActual;

    void Awake()
    {
        ObtenerReferenciaNoise();
    }

    void OnEnable()
    {
        if (noise == null)
            ObtenerReferenciaNoise();
    }

    private void ObtenerReferenciaNoise()
    {
        if (cineCam == null)
            cineCam = FindAnyObjectByType<CinemachineCamera>();

        if (cineCam != null)
        {
            // Busca la extensión de Perlin en la CinemachineCamera
            noise = cineCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
            
            if (noise == null)
            {
                noise = cineCam.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
            }
        }

        if (noise == null)
        {
            Debug.LogWarning("[CameraShake] No se encontró CinemachineBasicMultiChannelPerlin en " + (cineCam != null ? cineCam.name : "ninguna cámara") + ". Verifica tener asignado un Noise Profile en la CinemachineCamera.");
        }
    }

    /// <summary>
    /// Inicia la sacudida de cámara. Cancela cualquier sacudida previa para evitar bugs de reseteo.
    /// </summary>
    public void DispararShake(float duracion, float magnitud)
    {
        if (corrutinaShakeActual != null)
        {
            StopCoroutine(corrutinaShakeActual);
        }
        corrutinaShakeActual = StartCoroutine(ShakeCoroutine(duracion, magnitud));
    }

    public IEnumerator Shake(float duracion, float magnitud)
    {
        yield return ShakeCoroutine(duracion, magnitud);
    }

    private IEnumerator ShakeCoroutine(float duracion, float magnitud)
{
    if (noise == null) ObtenerReferenciaNoise();
    if (noise == null) yield break;

    // Guardamos los valores originales para restaurarlos después
    float ampOriginal = noise.AmplitudeGain;
    float freqOriginal = noise.FrequencyGain;

    // Aplicamos alta amplitud Y alta frecuencia para que sea brusco
    noise.AmplitudeGain = magnitud;
    noise.FrequencyGain = 4.0f; // <--- Subí esto (3.0 a 6.0) para vibración rápida

    yield return new WaitForSeconds(duracion);

    // Restauramos
    noise.AmplitudeGain = ampOriginal;
    noise.FrequencyGain = freqOriginal;
    corrutinaShakeActual = null;
}
}