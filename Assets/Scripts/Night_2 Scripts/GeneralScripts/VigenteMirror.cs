using UnityEngine;
using System.Collections;

/// <summary>
/// El Vigilante aparece reflejado en el espejo del baño justo cuando el jugador
/// recoge la llave. La aparición es breve: aparece, el jugador lo ve por un instante,
/// y cuando mira más de cerca ya no está.
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

    [Header("Tiempos y Sacudida")]
    [Tooltip("Cuántos segundos se queda visible el Vigilante en el espejo")]
    public float duracionAparicion = 2.2f;

    [Header("Ajustes de Sacudida Leve")]
    public bool usarSacudidaCamara = true;
    [Tooltip("Duración de la sacudida en segundos")]
    public float duracionShake = 0.3f;
    [Tooltip("Intensidad de la sacudida (reducida para que sea sutil)")]
    public float intensidadShake = 1.0f;

    /// <summary>
    /// Método público para iniciar la secuencia desde Act2ManagerDemo.
    /// </summary>
    public void Aparecer()
{
    gameObject.SetActive(true);
    if (modeloVigilante != null) 
    {
        modeloVigilante.SetActive(true); // <--- Forzamos la activación del objeto 3D
    }
    
    // Inicia la corrutina
    StopAllCoroutines();
    StartCoroutine(AparicionEnEspejo());
}

    /// <summary>
    /// Coroutine llamada por Act2ManagerDemo al recoger la llave / iniciar psicosis.
    /// </summary>
    public IEnumerator AparicionEnEspejo()
    {
        // Aparece el Vigilante en el espejo
        if (modeloVigilante != null) modeloVigilante.SetActive(true);
        if (sonidoVigilante != null) sonidoVigilante.Play();
        if (sonidoSusto != null) sonidoSusto.Play();

        // Sacudida leve de cámara (si está habilitada)
        if (usarSacudidaCamara && sacudidaCamara != null)
        {
            StartCoroutine(sacudidaCamara.Shake(duracionShake, intensidadShake));
        }

        yield return new WaitForSeconds(duracionAparicion);

        // Desaparece
        if (modeloVigilante != null) modeloVigilante.SetActive(false);
        if (sonidoVigilante != null) sonidoVigilante.Stop();
    }
}