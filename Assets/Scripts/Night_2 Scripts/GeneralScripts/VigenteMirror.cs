using UnityEngine;
using System.Collections;

public class VigenteMirror : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El modelo del Vigilante en el espejo.")]
    public GameObject modeloVigilante;

    [Tooltip("Referencia al componente CameraShake")]
    public CameraShake sacudidaCamara;

    [Header("Audio")]
    public AudioSource sonidoVigilante;
    public AudioSource sonidoSusto;

    [Header("Tiempos")]
    public float duracionAparicion = 2.2f;

    [Header("Ajustes de Sacudida")]
    public bool usarSacudidaCamara = true;
    public float duracionShake = 0.3f;
    public float intensidadShake = 1.0f; // Ajustá este valor si querés más o menos intensidad

    /// <summary>
    /// Método público llamado por el Trigger / Llave
    /// </summary>
    public void Aparecer()
    {
        gameObject.SetActive(true);

        if (modeloVigilante != null)
        {
            modeloVigilante.SetActive(true);
        }

        StopAllCoroutines();
        StartCoroutine(AparicionEnEspejo());
    }

    public IEnumerator AparicionEnEspejo()
    {
        // 1. Un pequeño delay antes del susto
        yield return new WaitForSeconds(0.5f);

        // 2. Reproducir efectos de audio del susto
        if (sonidoVigilante != null) sonidoVigilante.Play();
        if (sonidoSusto != null) sonidoSusto.Play();

        // 3. Disparar el temblor de cámara
        if (usarSacudidaCamara && sacudidaCamara != null)
        {
            StartCoroutine(sacudidaCamara.Shake(duracionShake, intensidadShake));
        }

        // 4. Esperar el tiempo de la visión
        yield return new WaitForSeconds(duracionAparicion);

        // 5. Desaparecer modelo y audio
        if (modeloVigilante != null) modeloVigilante.SetActive(false);
        if (sonidoVigilante != null) sonidoVigilante.Stop();
    }
}