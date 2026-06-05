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
        if (cineCam == null)
            cineCam = FindFirstObjectByType<CinemachineCamera>();

        if (cineCam != null)
            noise = cineCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public IEnumerator Shake(float duracion, float magnitud)
    {
        if (noise == null)
            yield break;

        noise.AmplitudeGain = magnitud;

        yield return new WaitForSeconds(duracion);

        noise.AmplitudeGain = 0f;
    }
}