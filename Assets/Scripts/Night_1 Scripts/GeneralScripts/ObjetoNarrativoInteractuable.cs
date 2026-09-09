using UnityEngine;

public class ObjetoNarrativoInteractuable : MonoBehaviour, IInteractable
{
    [Header("Configuración de Inspección")]
    [SerializeField] private string textoAccion = "Inspeccionar dibujo";
    [SerializeField] private Transform puntoInspeccion; // Un GameObject vacío hijo de la MainCamera

    [Header("Lore / Narrativa")]
    [TextArea(3, 5)]
    [SerializeField] private string descripcionLore = "Un dibujo infantil... me lo guardo en el bolsillo.";

    private bool yaFueTomado = false;

    public bool CanInteract() => !yaFueTomado;
    public string GetDescription() => textoAccion;

    public void Interact()
{
    if (yaFueTomado) return;
    yaFueTomado = true;

    // 1. Apagar Collider
    Collider col = GetComponent<Collider>();
    if (col != null) col.enabled = false;

    // 2. Mover al punto de inspección y orientar de frente a la cámara
    if (puntoInspeccion != null)
    {
        transform.SetParent(puntoInspeccion);
        transform.localPosition = Vector3.zero;
        
        // Aplica la rotación del punto de inspección + offset de 90 grados
        // Si no queda de frente, podés cambiar Vector3.up por Vector3.right o Vector3.forward
        transform.localRotation = Quaternion.identity * Quaternion.Euler(90f, 90f, -90f); 
    }

    // 3. Iniciar inspección
    if (Inspector3D.Instance != null)
    {
        Inspector3D.Instance.IniciarInspeccion(gameObject, descripcionLore);
    }
}
}