using UnityEngine;

public class HerramientaLimpiezaActo1 : MonoBehaviour, IInteractable
{
    [Header("Configuración de UI")]
    [SerializeField] private string textoAccion = "Tomar elementos de limpieza";

    [Header("Lore / Narrativa")]
    [TextArea(2, 4)]
    [SerializeField] private string mensajeAlTomar = "Bien... será mejor limpiar todo esto antes de empezar.";

    public bool CanInteract() => true;
    public string GetDescription() => textoAccion;

    public void Interact()
    {
        Debug.Log("[Acto 1] Elementos de limpieza recogidos.");

        if (Act1Manager.Instance != null)
        {
            // Activamos las variables en tu Act1Manager
            Act1Manager.Instance.tieneEscoba = true;
            Act1Manager.Instance.tieneTrapo = true;

            // Mostramos el subtítulo en pantalla
            Act1Manager.Instance.MostrarDialogo(mensajeAlTomar);
        }

        // Ocultamos el objeto de la escena
        gameObject.SetActive(false);
    }
}