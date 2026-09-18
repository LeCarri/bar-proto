using UnityEngine;

public class ClienteInteractuable : MonoBehaviour, IInteractable
{
    public EstadoCliente estadoActual = EstadoCliente.EsperandoAtencion;

    [Header("Configuración")]
    public string nombreCliente = "Carlos";
    public string dialogoPedido = "Hola Lucas, ¿todo bien?... Dame lo de siempre.";
    public string dialogoGracias = "Gracias, maestro.";

    [Header("UI")]
    public GameObject indicadorVioleta;

    private Act1Manager manager;

    private void Awake()
    {
        manager = FindObjectOfType<Act1Manager>();
    }

    private void Start()
    {
        estadoActual = EstadoCliente.EsperandoAtencion;
    }

    public void Interact()
    {
        if (manager == null) manager = FindObjectOfType<Act1Manager>();

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
                // Diálogo extra si volvés a hablarle ya atendido
                if (nombreCliente.Trim().ToLower() == "carlos")
                {
                    manager.MostrarDialogo("Carlos: ¿Mucho laburo?\nLucas: Lo de siempre, la verdad.\nCarlos: ¡Te vas a terminar matando!\nLucas: Y... no me queda otra.");
                }
                break;
        }
    }

    void TomarPedido()
    {
        string clienteNormalizado = nombreCliente.Trim().ToLower();

        // =========================
        // CARLOS
        // =========================
        if (clienteNormalizado == "carlos")
        {
            if (manager.carlosAtendido)
            {
                manager.MostrarDialogo("Carlos: Gracias, maestro.");
                estadoActual = EstadoCliente.Atendido;
                return;
            }

            manager.carlosPidioCerveza = true;
            manager.MostrarDialogo(nombreCliente + ": " + dialogoPedido);
            estadoActual = EstadoCliente.EsperandoPedido;

            if (manager.indicadorCervezas != null)
            {
                manager.indicadorCervezas.SetActive(true);
            }

            Debug.Log("[ClienteInteractuable] Carlos pidió cerveza. carlosPidioCerveza = TRUE.");
            return;
        }

        // =========================
        // MARIELA
        // =========================
        if (clienteNormalizado == "mariela")
        {
            if (!manager.carlosAtendido)
            {
                manager.MostrarDialogo("Lucas: Primero debería atender a Carlos en la barra.");
                return;
            }

            manager.RegistrarPedidoMarielaHoney();
            manager.MostrarDialogo(nombreCliente + ": " + dialogoPedido);
            estadoActual = EstadoCliente.EsperandoPedido;
            return;
        }

        // =========================
        // OTROS CLIENTES
        // =========================
        manager.MostrarDialogo(nombreCliente + ": " + dialogoPedido);
        estadoActual = EstadoCliente.EsperandoPedido;
    }

    void EntregarPedido()
    {
        string clienteNormalizado = nombreCliente.Trim().ToLower();

        // Chequeo de seguridad: ¿El jugador tiene un objeto en la mano?
        if (!manager.tieneObjetoEnMano && (ControladorMano3D.Instance == null || ControladorMano3D.Instance.ObtenerItemActual() == null))
        {
            manager.MostrarDialogo("Lucas: Todavía no tengo lo que me pidió...");
            return;
        }

        // =========================
        // CARLOS
        // =========================
        if (clienteNormalizado == "carlos")
        {
            ItemSO itemMano = ControladorMano3D.Instance != null ? ControladorMano3D.Instance.ObtenerItemActual() : null;
            
            // Verificamos que sea Cerveza (por slot o por nombre)
            bool esCerveza = (manager.itemCerveza != null && itemMano == manager.itemCerveza) || 
                             (itemMano != null && itemMano.nombreItem.ToLower().Contains("cerveza"));

            if (esCerveza)
            {
                // 1. Decir diálogo y actualizar flags en Manager
                manager.MostrarDialogo(nombreCliente + ": " + dialogoGracias);
                manager.carlosAtendido = true;
                manager.ClienteCompletado();

                // 2. Desactivar indicadores UI
                if (manager.indicadorCervezas != null) manager.indicadorCervezas.SetActive(false);
                if (indicadorVioleta != null) indicadorVioleta.SetActive(false);

                // 3. Vaciar la mano del jugador
                if (ControladorMano3D.Instance != null) ControladorMano3D.Instance.VaciarMano();
                manager.tieneObjetoEnMano = false;

                // 4. Marcar cliente como completado en este script
                estadoActual = EstadoCliente.Atendido;
                Debug.Log("[ClienteInteractuable] Carlos atendido con éxito y cerveza entregada.");
            }
            else
            {
                manager.MostrarDialogo("Carlos: Eso no es lo que te pedí, che.");
            }
            return;
        }

        // =========================
        // MARIELA
        // =========================
        if (clienteNormalizado == "mariela")
        {
            manager.MostrarDialogo(nombreCliente + ": " + dialogoGracias);
            manager.ClienteCompletado();

            if (indicadorVioleta != null) indicadorVioleta.SetActive(false);
            if (ControladorMano3D.Instance != null) ControladorMano3D.Instance.VaciarMano();
            manager.tieneObjetoEnMano = false;

            estadoActual = EstadoCliente.Atendido;
            return;
        }

        // =========================
        // OTROS CLIENTES
        // =========================
        manager.MostrarDialogo(nombreCliente + ": " + dialogoGracias);
        manager.ClienteCompletado();

        if (indicadorVioleta != null) indicadorVioleta.SetActive(false);
        if (ControladorMano3D.Instance != null) ControladorMano3D.Instance.VaciarMano();
        manager.tieneObjetoEnMano = false;

        estadoActual = EstadoCliente.Atendido;
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