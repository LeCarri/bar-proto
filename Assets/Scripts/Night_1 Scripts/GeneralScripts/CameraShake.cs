using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [SerializeField]
    private CinemachineCamera cineCam;

    private CinemachineBasicMultiChannelPerlin noise;

    void Awake()
    {
        ObtenerReferenciaNoise();
    }

    void OnEnable()
    {
        // Reintentamos por si la cámara no estaba lista en Awake
        if (noise == null)
            ObtenerReferenciaNoise();
    }

    private void ObtenerReferenciaNoise()
    {
        if (cineCam == null)
            cineCam = FindFirstObjectByType<CinemachineCamera>();

        if (cineCam != null)
        {
            // En Cinemachine 3.x buscamos el componente de Perlin en la cámara o en sus extensiones
            noise = cineCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
            
            if (noise == null)
            {
                noise = cineCam.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
            }
        }

        if (noise == null)
        {
            Debug.LogWarning("[CameraShake] No se encontró CinemachineBasicMultiChannelPerlin en " + (cineCam != null ? cineCam.name : "ninguna cámara") + ". Asegúrate de agregar el Noise en la CinemachineCamera.");
        }
    }

    public IEnumerator Shake(float duracion, float magnitud)
    {
        if (noise == null)
            ObtenerReferenciaNoise();

        if (noise == null)
        {
            Debug.LogError("[CameraShake] No se puede ejecutar el shake porque 'noise' es NULL.");
            yield break;
        }

        // Aplicamos la magnitud deseada
        noise.AmplitudeGain = magnitud;

        yield return new WaitForSeconds(duracion);

        // Volvemos a cero al finalizar
        noise.AmplitudeGain = 0f;
    }
}
