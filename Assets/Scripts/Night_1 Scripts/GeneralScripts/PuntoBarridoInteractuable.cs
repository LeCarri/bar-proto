using UnityEngine;

public class PuntoBarridoInteractuable : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private string descripcion = "Barrer zona";
    [SerializeField] private GameObject objetoSuciedad; // La mancha o basura en el piso
    private bool estaBarrido = false;

    public bool CanInteract()
    {
        return !estaBarrido;
    }

    public string GetDescription()
    {
        return descripcion;
    }

    public void Interact()
    {
        if (estaBarrido) return;

        estaBarrido = true;

        if (objetoSuciedad != null)
        {
            objetoSuciedad.SetActive(false);
        }

        if (Act1Manager.Instance != null)
        {
            Act1Manager.Instance.RegistrarZonaBarrida();
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}