using UnityEngine;
using System.Collections;

/// <summary>
/// El Vigilante aparece reflejado en el espejo del baño justo cuando el jugador
/// recoge la llave. La aparición es breve: aparece, el jugador lo ve por un instante,
/// y cuando mira más de cerca ya no está.
///
/// SETUP:
///  - Colocar este script en un GameObject vacío cerca del espejo del baño.
///  - modeloVigilante: el prefab/GameObject del Vigilante (debe empezar DESACTIVADO).
///  - El modelo debe estar posicionado DETRÁS del espejo o en el espacio del reflejo.
///  - Añadir un AudioSource con un sonido de respiración/susurro.
/// </summary>
public class VigenteMirror : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El modelo del Vigilante, ya posicionado en el 'espacio del espejo'. Empieza desactivado.")]
    public GameObject modeloVigilante;

    [Tooltip("Cámara principal del jugador para el shake")]
    public CameraShake sacudidaCamara;

    [Header("Audio")]
    [Tooltip("AudioSource con sonido de respiración pesada o susurro")]
    public AudioSource sonidoVigilante;

    [Tooltip("AudioSource con un stinger de susto (jump scare suave)")]
    public AudioSource sonidoSusto;

    [Header("Tiempos")]
    [Tooltip("Cuántos segundos se queda visible el Vigilante en el espejo")]
    public float duracionAparicion = 2.2f;

    [Tooltip("Segundos que el jugador ve algo antes del shake de cámara")]
    public float delayAntesSusto = 0.8f;

    /// <summary>
    /// Coroutine llamada por Act2Manager.LlaveRecogida().
    /// </summary>
    public IEnumerator AparicionEnEspejo()
    {
        // Aparece el Vigilante en el espejo
        if (modeloVigilante != null) modeloVigilante.SetActive(true);
        if (sonidoVigilante != null) sonidoVigilante.Play();

        yield return new WaitForSeconds(delayAntesSusto);

        // Susto suave + shake de cámara
        if (sonidoSusto != null) sonidoSusto.Play();
        if (sacudidaCamara != null)
            yield return StartCoroutine(sacudidaCamara.Shake(0.4f, 0.18f));

        yield return new WaitForSeconds(duracionAparicion - delayAntesSusto);

        // Desaparece
        if (modeloVigilante != null) modeloVigilante.SetActive(false);
        if (sonidoVigilante != null) sonidoVigilante.Stop();
    }
}
