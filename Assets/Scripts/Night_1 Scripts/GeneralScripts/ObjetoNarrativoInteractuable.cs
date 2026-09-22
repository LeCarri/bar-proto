using UnityEngine;

public class ObjetoNarrativoInteractuable : MonoBehaviour, IInteractable
{
    [Header("Configuración de Inspección")]
    [SerializeField] private string textoAccion = "Inspeccionar dibujo";
    [SerializeField] private Transform puntoInspeccion; // Opcional desde Inspector

    [Header("Lore / Narrativa")]
    [TextArea(3, 5)]
    [SerializeField] private string descripcionLore = "Un dibujo infantil... me lo guardo en el bolsillo.";

    private bool yaFueTomado = false;

    public bool CanInteract() => !yaFueTomado;
    public string GetDescription() => textoAccion;

    public void Interact()
    {
        if (yaFueTomado) return;

        // 1. Si no se asignó puntoInspeccion manualmente, intentamos obtenerlo desde el Inspector3D
        if (puntoInspeccion == null && Inspector3D.Instance != null)
        {
            puntoInspeccion = Inspector3D.Instance.PuntoInspeccion; // O el transform del Inspector3D
        }

        yaFueTomado = true;

        // 2. Apagar Collider para evitar volver a hacer raycast sobre él mientras se inspecciona
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 3. Mover al punto de inspección y orientar de frente
        if (puntoInspeccion != null)
        {
            transform.SetParent(puntoInspeccion);
            transform.localPosition = Vector3.zero;
            
            // Orientación frontal
            transform.localRotation = Quaternion.identity * Quaternion.Euler(90f, 90f, -90f); 
        }

        // 4. Iniciar inspección en el sistema 3D
        if (Inspector3D.Instance != null)
        {
            Inspector3D.Instance.IniciarInspeccion(gameObject, descripcionLore);
        }
        else
        {
            Debug.LogWarning("[ObjetoNarrativoInteractuable] No se encontró la instancia de Inspector3D en la escena.");
        }
    }
}