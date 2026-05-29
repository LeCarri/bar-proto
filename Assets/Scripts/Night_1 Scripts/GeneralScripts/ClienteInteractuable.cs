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

    private Act1Manager manager;

    private void Awake()
    {
        manager = FindObjectOfType<Act1Manager>();
    }

    private void Start()
    {
        // Al iniciar partida, todos los clientes arrancan esperando atención.
        // Esto evita que queden estados raros entre pruebas.
        estadoActual = EstadoCliente.EsperandoAtencion;
    }

    public void Interact()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<Act1Manager>();
        }

        if (manager == null)
        {
            Debug.LogWarning("[ClienteInteractuable] No se encontró Act1Manager en la escena.");
            return;
        }

        switch (estadoActual)
        {
            case EstadoCliente.EsperandoAtencion:
                TomarPedido();
                break;

            case EstadoCliente.EsperandoPedido:
                EntregarPedido();
                break;

            case EstadoCliente.Atendido:
                break;
        }
    }

    void TomarPedido()
    {
        string clienteNormalizado = nombreCliente.Trim().ToLower();

        // =========================
        // MARIELA
        // =========================
        if (clienteNormalizado == "mariela")
        {
            // Mariela NO puede pedir antes de que Carlos haya sido atendido.
            if (manager.clientesAtendidosTotal < 1)
            {
                manager.MostrarDialogo("Lucas: Primero debería atender al cliente de la barra.");
                Debug.Log("[ClienteInteractuable] Mariela bloqueada porque Carlos todavía no fue atendido.");
                return;
            }

            // Si Carlos ya fue atendido, ahora sí Mariela hace su pedido especial.
            manager.RegistrarPedidoMarielaHoney();

            estadoActual = EstadoCliente.EsperandoPedido;

            Debug.Log("[ClienteInteractuable] Pedido de Mariela tomado.");
            return;
        }

        // =========================
        // CARLOS
        // =========================
        if (clienteNormalizado == "carlos")
        {
            // Si Carlos ya fue atendido, no vuelve a pedir.
            if (manager.clientesAtendidosTotal >= 1)
            {
                manager.MostrarDialogo("Carlos: Gracias, maestro.");
                return;
            }

            manager.MostrarDialogo(nombreCliente + ": " + dialogoPedido);

            estadoActual = EstadoCliente.EsperandoPedido;

            Debug.Log("[ClienteInteractuable] Pedido de Carlos tomado.");
            return;
        }

        // =========================
        // OTROS CLIENTES
        // =========================
        manager.MostrarDialogo(nombreCliente + ": " + dialogoPedido);
        estadoActual = EstadoCliente.EsperandoPedido;

        Debug.Log("[ClienteInteractuable] Pedido tomado.");
    }

    void EntregarPedido()
    {
        string clienteNormalizado = nombreCliente.Trim().ToLower();

        // =========================
        // MARIELA
        // =========================
        if (clienteNormalizado == "mariela")
        {
            // Si por algún motivo Mariela llegó a EsperandoPedido antes de Carlos,
            // la devolvemos al estado correcto.
            if (manager.clientesAtendidosTotal < 1)
            {
                estadoActual = EstadoCliente.EsperandoAtencion;
                manager.marielaPidioHoney = false;

                manager.MostrarDialogo("Lucas: Primero debería atender al cliente de la barra.");
                Debug.Log("[ClienteInteractuable] Estado de Mariela corregido: no podía estar esperando pedido antes de Carlos.");
                return;
            }

            // Si todavía no pidió oficialmente la Honey, la hacemos pedir ahora.
            if (!manager.marielaPidioHoney)
            {
                manager.RegistrarPedidoMarielaHoney();
                estadoActual = EstadoCliente.EsperandoPedido;
                return;
            }

            // Si ya pidió, pero Lucas todavía no tiene algo entregable.
            if (!manager.TienePedidoEntregable())
            {
                manager.MostrarDialogo("Lucas: Todavía no tengo lo que me pidió...");
                return;
            }

            manager.MostrarDialogo(nombreCliente + ": " + dialogoGracias);
            estadoActual = EstadoCliente.Atendido;

            if (indicadorVioleta != null)
            {
                indicadorVioleta.SetActive(false);
            }

            manager.ClienteCompletado();

            Debug.Log("[ClienteInteractuable] Mariela atendida.");
            return;
        }

        // =========================
        // CARLOS
        // =========================
        if (clienteNormalizado == "carlos")
        {
            if (manager.TienePedidoEntregable())
            {
                manager.MostrarDialogo(nombreCliente + ": " + dialogoGracias);

                estadoActual = EstadoCliente.Atendido;

                if (indicadorVioleta != null)
                {
                    indicadorVioleta.SetActive(false);
                }

                manager.ClienteCompletado();

                Debug.Log("[ClienteInteractuable] Carlos atendido.");
            }
            else
            {
                manager.MostrarDialogo("Lucas: Todavía no tengo lo que me pidió...");
            }

            return;
        }

        // =========================
        // OTROS CLIENTES
        // =========================
        if (manager.TienePedidoEntregable())
        {
            manager.MostrarDialogo(nombreCliente + ": " + dialogoGracias);

            estadoActual = EstadoCliente.Atendido;

            if (indicadorVioleta != null)
            {
                indicadorVioleta.SetActive(false);
            }

            manager.ClienteCompletado();
        }
        else
        {
            manager.MostrarDialogo("Lucas: Todavía no tengo lo que me pidió...");
        }
    }

    public string GetDescription()
    {
        if (estadoActual == EstadoCliente.EsperandoAtencion)
        {
            return "Presiona [E] para tomar pedido";
        }

        if (estadoActual == EstadoCliente.EsperandoPedido)
        {
            return "Presiona [E] para entregar pedido";
        }

        return "";
    }

    public bool CanInteract()
    {
        return estadoActual != EstadoCliente.Atendido;
    }
}

public enum EstadoCliente
{
    EsperandoAtencion,
    EsperandoPedido,
    Atendido
}