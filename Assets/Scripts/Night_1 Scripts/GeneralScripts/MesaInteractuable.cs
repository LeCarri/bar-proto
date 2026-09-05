using UnityEngine;

public class MesaInteractuable : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private string descripcion = "Limpiar mesa";
    private bool estaLimpia = false;

    public bool CanInteract()
    {
        return !estaLimpia;
    }

    public string GetDescription()
    {
        return descripcion;
    }

    public void Interact()
    {
        if (estaLimpia) return;

        estaLimpia = true;

        if (Act1Manager.Instance != null)
        {
            Act1Manager.Instance.RegistrarMesasLimpias();
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}