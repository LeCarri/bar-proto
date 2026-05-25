using UnityEngine;

public class ClienteInteractuable : MonoBehaviour, IInteractable
{
    public EstadoCliente estadoActual = EstadoCliente.EsperandoAtencion;

    [Header("Configuración")]
    public string nombreCliente;
    public string dialogoPedido = "Hola, traeme una cerveza.";
    public string dialogoGracias = "Gracias, Lucas. Dejala ahí.";

    [Header("UI")]
    public GameObject indicadorVioleta;

    public void Interact()
    {
        switch (estadoActual)
        {
            case EstadoCliente.EsperandoAtencion:
                TomarPedido();
                break;
            case EstadoCliente.EsperandoPedido:
                EntregarPedido();
                break;
        }
    }

    void TomarPedido()
    {
        FindObjectOfType<Act1Manager>().MostrarDialogo(nombreCliente + ": " + dialogoPedido);
        estadoActual = EstadoCliente.EsperandoPedido;
        
        // El indicador violeta se queda encendido pero quizás podrías cambiarle el color 
        // o dejarlo para que Lucas sepa que todavía tiene algo pendiente aquí.
        
        Debug.Log("Pedido tomado. Lucas debe buscar el objeto.");
    }

    void EntregarPedido()
    {
        // Solo entregamos si Lucas realmente tiene el objeto (esto lo validamos en el Manager)
        if (FindObjectOfType<Act1Manager>().TienePedidoEntregable())
        {
            FindObjectOfType<Act1Manager>().MostrarDialogo(nombreCliente + ": " + dialogoGracias);
            estadoActual = EstadoCliente.Atendido;
            
            if (indicadorVioleta != null) indicadorVioleta.SetActive(false);
            
            FindObjectOfType<Act1Manager>().ClienteCompletado();
        }
        else
        {
            FindObjectOfType<Act1Manager>().MostrarDialogo("Lucas: Todavía no tengo lo que me pidió...");
        }
    }

    public string GetDescription()
    {
        if (estadoActual == EstadoCliente.EsperandoAtencion) return "Presiona [E] para tomar pedido";
        if (estadoActual == EstadoCliente.EsperandoPedido) return "Presiona [E] para entregar pedido";
        return "";
    }

    public bool CanInteract()
    {
        return estadoActual != EstadoCliente.Atendido;
    }
}

public enum EstadoCliente { EsperandoAtencion, EsperandoPedido, Atendido }
