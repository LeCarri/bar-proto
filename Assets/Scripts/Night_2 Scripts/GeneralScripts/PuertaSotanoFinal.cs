using System.Collections;
using UnityEngine;

public class PuertaSotanoFinal : MonoBehaviour
{
    [Header("Referencias del Candado y Puerta")]
    [Tooltip("El GameObject del candado o cadena puesto en la puerta")]
    public GameObject candadoObjeto;
    [Tooltip("Componente Rigidbody del candado para hacerlo caer con física (opcional)")]
    public Rigidbody candadoRigidbody;
    [Tooltip("Transform o Animator de la puerta que se va a abrir")]
    public Animator animatorPuerta;
    public string nombreTriggerAbrir = "Abrir"; // O la rotación si la hacés por script

    [Header("Audio")]
    public AudioSource audioSourcePuerta;
    public AudioClip sonidoLlaveGirar;
    public AudioClip sonidoCandadoCaer;
    public AudioClip sonidoPuertaRechinado;

    [Header("UI / Feedback")]
    public string mensajeSinLlave = "La puerta está trancada con un candado. Necesito la llave.";

    private bool yaSeUso = false;

    /// <summary>
    /// Método principal de interacción. Llama a este método desde tu sistema de interacción (Raycast / Interactable).
    /// </summary>
    public void InteractuarConPuerta()
    {
        if (yaSeUso) return;

        Act2ManagerDemo manager = Act2ManagerDemo.Instance;
        if (manager == null)
        {
            Debug.LogError("[PuertaSotanoFinal] Act2ManagerDemo no encontrado.");
            return;
        }

        // Si el jugador no tiene la llave aún
        if (!manager.llaveTenida)
        {
            Debug.Log("[PuertaSotanoFinal] El jugador intentó abrir pero no tiene la llave.");
            // Mostrar mensaje en pantalla si tenés sistema de diálogos/subtítulos
            // manager.MostrarSubtitulo(mensajeSinLlave, 2.0f);
            return;
        }

        // Si tiene la llave, arrancamos la secuencia final
        yaSeUso = true;
        StartCoroutine(SecuenciaAbrirSotanoYCierre());
    }

    private IEnumerator SecuenciaAbrirSotanoYCierre()
    {
        Debug.Log("[PuertaSotanoFinal] ¡Iniciando secuencia de apertura del sótano!");

        // 1. Sonido de insertar y girar la llave en el candado
        if (audioSourcePuerta != null && sonidoLlaveGirar != null)
        {
            audioSourcePuerta.PlayOneShot(sonidoLlaveGirar);
        }

        yield return new WaitForSeconds(1.0f);

        // 2. Liberar / Hacer caer el candado
        if (candadoRigidbody != null)
        {
            candadoRigidbody.isKinematic = false; // Se activa la gravedad y cae
            candadoRigidbody.AddForce(Vector3.down * 2f, ForceMode.Impulse);
        }
        
        if (audioSourcePuerta != null && sonidoCandadoCaer != null)
        {
            audioSourcePuerta.PlayOneShot(sonidoCandadoCaer);
        }

        yield return new WaitForSeconds(0.8f);

        // Si querés ocultar el candado tras la caída:
        // if (candadoObjeto != null) candadoObjeto.SetActive(false);

        // 3. Abrir la puerta del sótano
        if (animatorPuerta != null)
        {
            animatorPuerta.SetTrigger(nombreTriggerAbrir);
        }

        if (audioSourcePuerta != null && sonidoPuertaRechinado != null)
        {
            audioSourcePuerta.PlayOneShot(sonidoPuertaRechinado);
        }

        // 4. Pausa dramática: El jugador contempla la oscuridad del sótano pero no se anima a bajar
        yield return new WaitForSeconds(2.5f);

        // 5. Finalizar la noche llamando al Cierre del Manager
        if (Act2ManagerDemo.Instance != null)
        {
            Debug.Log("[PuertaSotanoFinal] Disparando SecuenciaCierre en Act2ManagerDemo...");
            Act2ManagerDemo.Instance.StartCoroutine(Act2ManagerDemo.Instance.SecuenciaCierre());
        }
    }
}