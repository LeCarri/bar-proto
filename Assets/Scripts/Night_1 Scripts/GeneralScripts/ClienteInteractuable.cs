using UnityEngine;

public class ClienteInteractuable : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    public string nombreCliente;
    public string dialogoPedido;
    
    [Header("UI Indicador")]
    public GameObject indicadorVisual; // Arrastrá acá el Canvas o la Imagen de la flecha

    private bool yaAtendido = false;

    void Start()
    {
        // Al empezar el servicio, la flecha debe estar encendida
        if (indicadorVisual != null) 
            indicadorVisual.SetActive(true);
    }

    public void Interact()
    {
        if (!yaAtendido)
        {
            FindObjectOfType<Act1Manager>().MostrarDialogo(nombreCliente + ": " + dialogoPedido);
            yaAtendido = true;

            // ¡Apagamos la señalización!
            if (indicadorVisual != null) 
                indicadorVisual.SetActive(false);

            FindObjectOfType<Act1Manager>().ClienteAtendido();
        }
    }

    public string GetDescription() => yaAtendido ? "" : "Presiona [E] para atender";
}