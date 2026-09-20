using UnityEngine;

public class CajaMusicalInteractuable : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        if (Act1Manager.Instance != null && Act1Manager.Instance.estadoActual == Act1Manager.ActoState.Quiebre)
        {
            Debug.Log("[CajaMusical] Interacción detectada.");
            Act1Manager.Instance.InteractuarCajaMusical();
            
            // Opcional: desactivar el collider para no interactuar dos veces
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }

    public string GetDescription()
    {
        if (Act1Manager.Instance != null && Act1Manager.Instance.estadoActual == Act1Manager.ActoState.Quiebre)
        {
            return "Presiona [E] para examinar la caja musical";
        }
        return "";
    }

    public bool CanInteract()
    {
        return Act1Manager.Instance != null && Act1Manager.Instance.estadoActual == Act1Manager.ActoState.Quiebre;
    }
}