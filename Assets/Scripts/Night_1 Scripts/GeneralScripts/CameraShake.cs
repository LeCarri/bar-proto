using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [Tooltip("Transform a sacudir. Asigná la Main Camera aquí. Si se deja vacío, sacude este mismo GameObject.")]
    public Transform camaraObjetivo;

    void Start()
    {
        // Fallback automático: busca la cámara principal si no se asignó nada
        if (camaraObjetivo == null && Camera.main != null)
            camaraObjetivo = Camera.main.transform;
    }

    public IEnumerator Shake(float duracion, float magnitud)
    {
        Transform objetivo = camaraObjetivo != null ? camaraObjetivo : transform;

        Vector3 posicionOriginal = objetivo.localPosition;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracion)
        {
            float x = Random.Range(-1f, 1f) * magnitud;
            float y = Random.Range(-1f, 1f) * magnitud;

            objetivo.localPosition = new Vector3(
                posicionOriginal.x + x,
                posicionOriginal.y + y,
                posicionOriginal.z
            );

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        objetivo.localPosition = posicionOriginal;
    }
}