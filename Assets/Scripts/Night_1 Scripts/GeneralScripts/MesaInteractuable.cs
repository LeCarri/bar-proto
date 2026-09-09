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
        if (Act1Manager.Instance != null && !Act1Manager.Instance.tieneTrapo)
        {
            Act1Manager.Instance.MostrarDialogo("Necesito los elementos de limpieza para acomodar las mesas.");
            return;
        }

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