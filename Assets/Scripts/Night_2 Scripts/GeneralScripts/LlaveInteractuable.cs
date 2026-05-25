using UnityEngine;

/// <summary>
/// Llave encontrada en el baño (zona sucia).
/// Al recogerla, dispara:
///  1. Aparición del Vigilante en el espejo (VigenteMirror)
///  2. Inicio del estado de psicosis (EfectoPsicosis)
///  3. Notifica al Act2Manager que el jugador tiene la llave
/// Implementa IInteractable.
///
/// SETUP: Colocar en el GameObject de la llave en el baño.
/// Act2Manager.LlaveRecogida() activa la secuencia de Vigilante + Psicosis.
/// </summary>
public class LlaveInteractuable : MonoBehaviour, IInteractable
{
    [Header("Estado")]
    private bool recogida = false;

    [Header("Visual")]
    [Tooltip("Indicador flotante sobre la llave")]
    public GameObject indicadorFlotante;

    [Tooltip("Modelo 3D de la llave que desaparece al recogerla")]
    public GameObject modeloLlave;

    [Header("Audio")]
    [Tooltip("Sonido metálico al recoger la llave")]
    public AudioSource sonidoRecoger;

    void Start()
    {
        if (indicadorFlotante != null) indicadorFlotante.SetActive(true);
    }

    public void Interact()
    {
        if (recogida) return;
        recogida = true;

        if (indicadorFlotante != null) indicadorFlotante.SetActive(false);
        if (sonidoRecoger != null)     sonidoRecoger.Play();
        if (modeloLlave != null)       modeloLlave.SetActive(false);

        // El Act2Manager se encarga del resto (Vigilante + Psicosis)
        Act2Manager.Instance?.LlaveRecogida();
    }
    public bool CanInteract()
    {
        return true;
    }
    public string GetDescription()
    {
        if (recogida) return "";
        return "Presiona [E] para recoger la llave";
    }
}
