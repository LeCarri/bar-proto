using UnityEngine;

public class ClienteInteractuable : MonoBehaviour, IInteractable
{
    public EstadoCliente estadoActual = EstadoCliente.EsperandoAtencion;

    [Header("Configuración")]
    public string nombreCliente = "Carlos";
    [TextArea] public string dialogoPedido = "Hola Lucas, ¿todo bien?... Dame lo de siempre.";
    [TextArea] public string dialogoGracias = "Gracias, maestro.";

    [Header("Cliente Especial (Quiebre)")]
    [Tooltip("Marcar solo si este cliente es el Cliente 4 que pide el Whisky Sangre")]
    public bool esClienteCuatro = false;

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

            Debug.Log("[ClienteInteractuable] Carlos pidió cerveza.");
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
        // OTROS CLIENTES / CLIENTE 4
        // =========================
        manager.MostrarDialogo(nombreCliente + ": " + dialogoPedido);
        estadoActual = EstadoCliente.EsperandoPedido;
    }

    void EntregarPedido()
    {
        string clienteNormalizado = nombreCliente.Trim().ToLower();

        if (!manager.tieneObjetoEnMano && (ControladorMano3D.Instance == null || ControladorMano3D.Instance.ObtenerItemActual() == null))
        {
            manager.MostrarDialogo("Lucas: Todavía no tengo lo que me pidió...");
            return;
        }

        ItemSO itemMano = ControladorMano3D.Instance != null ? ControladorMano3D.Instance.ObtenerItemActual() : null;

        // =========================
        // CLIENTE 4 (QUIEBRE - WHISKY SANGRE)
        // =========================
        if (esClienteCuatro || clienteNormalizado == "cliente 4" || clienteNormalizado == "mariela quiebre")
        {
            bool esWhisky = (manager.itemWhisky != null && itemMano == manager.itemWhisky) ||
                            (itemMano != null && itemMano.nombreItem.ToLower().Contains("whisky"));

            if (esWhisky)
            {
                if (ControladorMano3D.Instance != null) ControladorMano3D.Instance.VaciarMano();
                manager.tieneObjetoEnMano = false;

                if (indicadorVioleta != null) indicadorVioleta.SetActive(false);

                estadoActual = EstadoCliente.Atendido;

                manager.IniciarSecuenciaQuiebre();
                gameObject.SetActive(false);
                return;
            }
            else
            {
                manager.MostrarDialogo("Cliente: Eso no es lo que pedí...");
                return;
            }
        }

        // =========================
        // CARLOS
        // =========================
        if (clienteNormalizado == "carlos")
        {
            bool esCerveza = (manager.itemCerveza != null && itemMano == manager.itemCerveza) || 
                             (itemMano != null && itemMano.nombreItem.ToLower().Contains("cerveza"));

            if (esCerveza)
            {
                manager.MostrarDialogo(nombreCliente + ": " + dialogoGracias);
                manager.carlosAtendido = true;
                manager.ClienteCompletado();

                if (manager.indicadorCervezas != null) manager.indicadorCervezas.SetActive(false);
                if (indicadorVioleta != null) indicadorVioleta.SetActive(false);

                if (ControladorMano3D.Instance != null) ControladorMano3D.Instance.VaciarMano();
                manager.tieneObjetoEnMano = false;

                estadoActual = EstadoCliente.Atendido;
            }
            else
            {
                manager.MostrarDialogo("Carlos: Eso no es lo que te pedí, che.");
            }
            return;
        }

        // =========================
        // OTROS CLIENTES (MARIELA / CLIENTES SECUNDARIOS)
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

// DECLARACIÓN DEL ENUM OBLIGATORIA
public enum EstadoCliente
{
    EsperandoAtencion,
    EsperandoPedido,
    Atendido
}