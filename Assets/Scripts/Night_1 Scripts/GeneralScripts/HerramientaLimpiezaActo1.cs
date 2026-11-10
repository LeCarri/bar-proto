using UnityEngine;

public class HerramientaLimpiezaActo1 : MonoBehaviour, IInteractable
{
    [Header("Configuración de UI")]
    [SerializeField] private string textoAccion = "Tomar elementos de limpieza";

    [Header("Indicadores Visuales")]
    [SerializeField] private GameObject[] indicadoresLimpieza;

    [Header("Lore / Narrativa")]
    [TextArea(2, 4)]
    [SerializeField] private string mensajeAlTomar =
        "Bien... será mejor limpiar todo esto antes de empezar.";

    public bool CanInteract() => true;

    public string GetDescription() => textoAccion;

    public void Interact()
    {
        Debug.Log("[Acto 1] Elementos de limpieza recogidos.");

        if (Act1Manager.Instance != null)
        {
            // Registrar herramientas recogidas
            Act1Manager.Instance.tieneEscoba = true;
            Act1Manager.Instance.tieneTrapo = true;

            // Mostrar diálogo
            Act1Manager.Instance.MostrarDialogo(mensajeAlTomar);
        }

        // Ocultar indicadores visuales
        foreach (GameObject indicador in indicadoresLimpieza)
        {
            if (indicador != null)
                indicador.SetActive(false);
        }

        // Ocultar el objeto interactuable
        gameObject.SetActive(false);
    }
}