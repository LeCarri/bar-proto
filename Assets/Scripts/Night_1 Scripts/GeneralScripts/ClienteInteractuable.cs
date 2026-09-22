using UnityEngine;

public class ClienteInteractuable : MonoBehaviour, IInteractable
{
    public enum EstadoCliente
    {
        EsperandoAtencion,
        EsperandoPedido,
        Atendido
    }
    [Header("Estado Actual")]
    public EstadoCliente estadoActual = EstadoCliente.EsperandoAtencion;

    [Header("Configuración del Cliente")]
    public string nombreCliente = "Carlos";
    [TextArea] public string dialogoPedido = "Hola Lucas, ¿todo bien?... Dame lo de siempre.";
    [TextArea] public string dialogoGracias = "Gracias, maestro.";

    [Header("Ítem Requerido")]
    [Tooltip("El ítem que este cliente pide (ej. Cerveza, Whisky).")]
    public ItemSO itemPedidoRequerido;

    [Header("Control de Turno")]
    [Tooltip("Indica si es el turno activo de este cliente. Lo gestiona el Act1Manager.")]
    public bool esSuTurno = false;

    [Header("Configuración Especiales")]
    public bool esClienteCuatro = false;
    public bool disparaCelularAlEntregar = false; // Se cambió el nombre para mayor claridad

    [Header("UI / Indicador Visual")]
    public GameObject indicadorVisual;

    private Act1Manager manager;

    private void Awake()
    {
        manager = Object.FindFirstObjectByType<Act1Manager>();
    }

    private void Start()
    {
        estadoActual = EstadoCliente.EsperandoAtencion;
    }

    private void Update()
    {
        ActualizarEstadoIndicador();
    }

    private void ActualizarEstadoIndicador()
    {
        if (indicadorVisual == null) return;

        bool debeMostrar = esSuTurno && estadoActual != EstadoCliente.Atendido;
        
        if (indicadorVisual.activeSelf != debeMostrar)
        {
            indicadorVisual.SetActive(debeMostrar);
        }
    }

    public void Interact()
    {
        if (manager == null) manager = Object.FindFirstObjectByType<Act1Manager>();
        if (manager == null) return;

        if (!esSuTurno)
        {
            manager.MostrarDialogo("Lucas: Primero tengo que atender a los otros clientes...");
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
                HablarClienteAtendido();
                break;
        }
    }

    void TomarPedido()
    {
        manager.MostrarDialogo(nombreCliente + ": " + dialogoPedido);
        estadoActual = EstadoCliente.EsperandoPedido;

        if (nombreCliente.Trim().ToLower() == "carlos")
        {
            manager.carlosPidioCerveza = true;
        }
    }

    void EntregarPedido()
    {
        ItemSO itemMano = ControladorMano3D.Instance != null ? ControladorMano3D.Instance.ObtenerItemActual() : null;

        if (itemMano == null && !manager.tieneObjetoEnMano)
        {
            manager.MostrarDialogo("Lucas: Todavía no tengo lo que me pidió...");
            return;
        }

        // CASO ESPECIAL: CLIENTE 4 (QUIEBRE)
        if (esClienteCuatro || nombreCliente.Trim().ToLower() == "cliente 4")
        {
            bool esWhiskySangre = (manager.itemWhiskySangre != null && itemMano == manager.itemWhiskySangre) ||
                                  (itemMano != null && itemMano.nombreItem.ToLower().Contains("whisky"));

            if (esWhiskySangre)
            {
                VaciarManoJugador();
                estadoActual = EstadoCliente.Atendido;
                esSuTurno = false;

                if (indicadorVisual != null) indicadorVisual.SetActive(false);

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

        // CLIENTES ESTÁNDAR
        bool pedidoValido = false;

        if (itemPedidoRequerido != null)
        {
            pedidoValido = (itemMano == itemPedidoRequerido || (itemMano != null && itemMano.nombreItem.Equals(itemPedidoRequerido.nombreItem, System.StringComparison.OrdinalIgnoreCase)));
        }
        else
        {
            pedidoValido = true; 
        }

        if (pedidoValido)
        {
            manager.MostrarDialogo(nombreCliente + ": " + dialogoGracias);
            manager.ClienteCompletado();

            if (nombreCliente.Trim().ToLower() == "carlos") manager.carlosAtendido = true;

            VaciarManoJugador();
            estadoActual = EstadoCliente.Atendido;
            esSuTurno = false;

            if (indicadorVisual != null) indicadorVisual.SetActive(false);

            // DISPARAR NOTIFICACIÓN CON 2 SEGUNDOS DE RETRASO
            if (disparaCelularAlEntregar)
            {
                manager.DispararVibracionCelularConRetraso(2f);
            }

            // PASAR AL SIGUIENTE CLIENTE
            manager.AvanzarSiguienteCliente();
        }
        else
        {
            manager.MostrarDialogo(nombreCliente + ": Eso no es lo que te pedí...");
        }
    }

    void HablarClienteAtendido()
    {
        if (nombreCliente.Trim().ToLower() == "carlos")
        {
            manager.MostrarDialogo("Carlos: ¿Mucho laburo?\nLucas: Lo de siempre, la verdad.");
        }
        else
        {
            manager.MostrarDialogo(nombreCliente + ": Gracias por la bebida.");
        }
    }

    private void VaciarManoJugador()
    {
        if (ControladorMano3D.Instance != null) ControladorMano3D.Instance.VaciarMano();
        manager.tieneObjetoEnMano = false;
    }

    public string GetDescription()
    {
        if (!esSuTurno || estadoActual == EstadoCliente.Atendido) return "";

        if (estadoActual == EstadoCliente.EsperandoAtencion)
            return "Presiona [E] para tomar pedido";

        if (estadoActual == EstadoCliente.EsperandoPedido)
            return "Presiona [E] para entregar pedido";

        return "";
    }

    public bool CanInteract()
    {
        return esSuTurno && estadoActual != EstadoCliente.Atendido;
    }
}