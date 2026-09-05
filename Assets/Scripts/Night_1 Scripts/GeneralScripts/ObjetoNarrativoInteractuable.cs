using UnityEngine;

public class ObjetoNarrativoInteractuable : MonoBehaviour, IInteractable
{
    [Header("Configuración de UI")]
    [SerializeField] private string textoAccion = "Inspeccionar foto";

    [Header("Lore / Narrativa")]
    [TextArea(3, 5)]
    [SerializeField] private string descripcionLore = "Un dibujo infantil... sin firma.";

    [Header("Opciones")]
    [SerializeField] private bool sePuedeRepetir = true;

    private bool yaFueInspeccionado = false;

    public bool CanInteract()
    {
        if (!sePuedeRepetir && yaFueInspeccionado) return false;
        return true;
    }

    public string GetDescription()
    {
        return textoAccion;
    }

    public void Interact()
    {
        Debug.Log("¡Interacción detectada en el dibujo!");
        yaFueInspeccionado = true;

        if (Act1Manager.Instance != null)
        {
            Act1Manager.Instance.MostrarDialogo(descripcionLore);
        }

        if (!sePuedeRepetir)
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }
}