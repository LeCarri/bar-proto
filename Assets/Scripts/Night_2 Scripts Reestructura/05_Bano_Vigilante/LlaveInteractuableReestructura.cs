using UnityEngine;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Copia de LlaveInteractuable.cs adaptada a la reestructura.
/// (El original sigue intacto para la demo.)
///
/// La llave escondida en el cubículo FUERA DE SERVICIO. El Act2ManagerReestructura la activa
/// después de leer la nota. Al agarrarla:
///  - las puertas de los cubículos de al lado se cierran de golpe,
///  - se escuchan Sombras acercándose desde el salón,
///  - empiezan las apariciones del Vigilante.
///
/// SETUP: en el GameObject de la llave dentro del cubículo (Collider + capa "Interactable").
/// </summary>
public class LlaveInteractuableReestructura : MonoBehaviour, IInteractable
{
    [Header("Visual")]
    [Tooltip("Indicador flotante sobre la llave (opcional).")]
    public GameObject indicadorFlotante;
    [Tooltip("Modelo 3D de la llave que desaparece al recogerla. Vacío = se ocultan todos los renderers de este objeto.")]
    public GameObject modeloLlave;

    [Header("Audio")]
    [Tooltip("Sonido metálico al recoger la llave (opcional).")]
    public AudioSource sonidoRecoger;

    private bool recogida;

    void Start()
    {
        if (indicadorFlotante != null) indicadorFlotante.SetActive(true);
    }

    public bool CanInteract() => !recogida;

    public string GetDescription() => recogida ? "" : "Presiona [E] para recoger la llave";

    public void Interact()
    {
        if (recogida) return;
        recogida = true;

        if (indicadorFlotante != null) indicadorFlotante.SetActive(false);
        if (sonidoRecoger != null) sonidoRecoger.Play();

        if (modeloLlave != null && modeloLlave != gameObject)
        {
            modeloLlave.SetActive(false);
        }
        else
        {
            // Se ocultan renderers y colliders (no el GameObject, así el sonido termina de sonar)
            foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        }
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;

        if (Act2ManagerReestructura.Instance != null) Act2ManagerReestructura.Instance.LlaveRecogida();
        else Debug.LogError("[LlaveInteractuableReestructura] No hay Act2ManagerReestructura en la escena.");
    }
}
