using UnityEngine;

public class Cliente : MonoBehaviour, IInteractable
{
    public EstadoCliente estadoActual = EstadoCliente.EsperandoAtencion;

    [Header("Configuración")]
    public string nombreCliente;
    public string dialogoPedido = "Hola, traeme una cerveza.";
    public string dialogoGracias = "Gracias, Lucas. Dejala ahí.";

    [SerializeField]
    [InspectorName("Pedido del cliente")]
    private Liquid bebidaDeseada = Liquid.beer;

    [SerializeField]
    [InspectorName("Collision grab")]
    [Tooltip("¿Puede el cliente agarrar pedidos que colisionen con él?")]
    private bool canCollisionGrab = false;

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
        Manager.Instance.MostrarDialogo(nombreCliente + ": " + dialogoPedido);
        estadoActual = EstadoCliente.EsperandoPedido;
        
        // El indicador violeta se queda encendido pero quizás podrías cambiarle el color 
        // o dejarlo para que Lucas sepa que todavía tiene algo pendiente aquí.
        
        Debug.Log("Pedido tomado. Lucas debe buscar el objeto.");
    }

    void EntregarPedido()
    {
        if (PlayerInteract.Instance.currentPickup == null)
            return;

        // Solo entregamos si Lucas realmente tiene el objeto (esto lo validamos en el Manager)
        if (PlayerInteract.Instance.currentPickup.lc.liquid == bebidaDeseada)
        {
            Manager.Instance.MostrarDialogo(nombreCliente + ": " + dialogoGracias);
            estadoActual = EstadoCliente.Atendido;
            
            if (indicadorVioleta != null) indicadorVioleta.SetActive(false);
            
            if (FindObjectOfType<Act1Manager>() != null)
                FindObjectOfType<Act1Manager>().ClienteCompletado();

            PlayerInteract.Instance.ResetCurrentPickup();
            DeactivateLc(PlayerInteract.Instance.currentPickup.lc); 
        } if (PlayerInteract.Instance.currentPickup.lc.liquid != bebidaDeseada)
        {
            Manager.Instance.MostrarDialogo("Lucas: Esto no es lo que me pidió...");
        }
        else
        {
            Manager.Instance.MostrarDialogo("Lucas: Todavía no tengo lo que me pidió...");
        }
    }

    void CollideGrab(LiquidContainer lc)
    {
        if (lc.liquid == bebidaDeseada)
        {
            Manager.Instance.MostrarDialogo(nombreCliente + ": " + dialogoGracias);
            estadoActual = EstadoCliente.Atendido;

            if (indicadorVioleta != null) indicadorVioleta.SetActive(false);

            if (FindObjectOfType<Act1Manager>() != null)
                FindObjectOfType<Act1Manager>().ClienteCompletado();

            DeactivateLc(lc);
            Debug.Log("Performed collision grab.");
        }
    }

    private void DeactivateLc(LiquidContainer lc) 
    {
        lc.gameObject.SetActive(false);
    }

    public string GetDescription()
    {
        if (estadoActual == EstadoCliente.EsperandoAtencion) return "Presiona [E] para tomar pedido";
        if (estadoActual == EstadoCliente.EsperandoPedido) return "Presiona [E] para entregar pedido";
        return "";
    }

    public bool CanInteract()
    {
        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (canCollisionGrab == true 
            && collision.transform.gameObject.GetComponent<LiquidContainer>() != null
            && estadoActual == EstadoCliente.EsperandoPedido) 
        {
            CollideGrab(collision.transform.gameObject.GetComponent<LiquidContainer>());
        }
    }

}