using UnityEngine;
using System.Collections;

public class VigenteMirror : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El modelo del Vigilante en el espejo.")]
    public GameObject modeloVigilante;

    [Tooltip("Referencia al script de temblor (si se deja vacío, se busca automáticamente)")]
    public CameraShake sacudidaCamara;

    [Header("Audio")]
    public AudioSource sonidoVigilante;
    public AudioSource sonidoSusto;

    [Header("Tiempos y Ajustes")]
    public float duracionAparicion = 2.2f;
    public bool usarSacudidaCamara = true;
    public float duracionShake = 0.4f;
    public float intensidadShake = 1.2f;

    private void Awake()
    {
        // Fallback por si no se asignó en el Inspector
        if (sacudidaCamara == null)
        {
            sacudidaCamara = Object.FindFirstObjectByType<CameraShake>();
        }
    }

    /// <summary>
    /// Método público llamado al tomar la llave o cruzar el trigger del baño.
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

    private IEnumerator AparicionEnEspejo()
    {
        // 1. Pequeño delay de tensión antes del impacto
        yield return new WaitForSeconds(0.4f);

        // 2. Audio del susto
        if (sonidoVigilante != null) sonidoVigilante.Play();
        if (sonidoSusto != null) sonidoSusto.Play();

        // 3. Disparar el shake usando Perlin en Cinemachine
        if (usarSacudidaCamara && sacudidaCamara != null)
        {
            StartCoroutine(sacudidaCamara.Shake(duracionShake, intensidadShake));
        }

        // 4. Duración de la silueta visible en el espejo
        yield return new WaitForSeconds(duracionAparicion);

        // 5. Ocultar silueta y detener audio
        if (modeloVigilante != null) modeloVigilante.SetActive(false);
        if (sonidoVigilante != null) sonidoVigilante.Stop();
    }
}