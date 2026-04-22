using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public IEnumerator Shake(float duracion, float magnitud)
    {
        Vector3 posicionOriginal = transform.localPosition;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracion)
        {
            // Generamos un movimiento aleatorio
            float x = Random.Range(-1f, 1f) * magnitud;
            float y = Random.Range(-1f, 1f) * magnitud;

            transform.localPosition = new Vector3(x, y, posicionOriginal.z);

            tiempoTranscurrido += Time.deltaTime;

            yield return null; // Espera al siguiente frame
        }

        transform.localPosition = posicionOriginal; // Volvemos a la normalidad
    }
}