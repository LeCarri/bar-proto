using UnityEngine;

public class PuntoVasos : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private ItemSO itemVasoVacio;

    [Header("Visual (Opcional)")]
    [SerializeField] private GameObject vasoDisponible;

    public bool CanInteract()
    {
        Act1Manager manager = Act1Manager.Instance;
        if (manager == null) return false;

        // 1. Si la mano ya está ocupada, no dejamos agarrar otro vaso
        if (ControladorMano3D.Instance != null && ControladorMano3D.Instance.TieneManoOcupada())
        {
            return false;
        }

        // 2. Validar que estemos en fase de servicio
        if (manager.estadoActual != Act1Manager.ActoState.Servicio)
        {
            return false;
        }

        // 3. Obtener el cliente actual del turno
        ClienteInteractuable clienteActual = manager.ObtenerClienteActual();
        if (clienteActual == null)
            return false;

        // 4. NUEVO: El cliente DEBE estar en estado "EsperandoPedido" (ya le tomamos el pedido hablases con él)
        if (clienteActual.estadoActual != ClienteInteractuable.EstadoCliente.EsperandoPedido)
        {
            return false;
        }

        // 5. Validar si el cliente actual requiere cerveza basándonos en su ítem requerido
        bool clientePideCerveza = (manager.itemCerveza != null && clienteActual.itemPedidoRequerido == manager.itemCerveza);

        // Caso especial para Carlos
        bool esCarlosYQuiere = (clienteActual.nombreCliente.Trim().ToLower() == "carlos" && manager.carlosPidioCerveza);

        return clientePideCerveza || esCarlosYQuiere;
    }

    public string GetDescription()
    {
        Act1Manager manager = Act1Manager.Instance;

        // Si ya tiene las manos ocupadas
        if (ControladorMano3D.Instance != null && ControladorMano3D.Instance.TieneManoOcupada())
        {
            return "Ya tenés las manos ocupadas";
        }

        // Si no es el momento del servicio
        if (manager == null || manager.estadoActual != Act1Manager.ActoState.Servicio)
        {
            return "Ahora no se necesita un vaso";
        }

        ClienteInteractuable clienteActual = manager.ObtenerClienteActual();
        if (clienteActual == null)
        {
            return "Ningún cliente ha pedido cerveza ahora";
        }

        // Si todavía no le tomó el pedido
        if (clienteActual.estadoActual == ClienteInteractuable.EstadoCliente.EsperandoAtencion)
        {
            return "Primero toma el pedido del cliente";
        }

        bool clientePideCerveza = (manager.itemCerveza != null && clienteActual.itemPedidoRequerido == manager.itemCerveza);
        bool esCarlosYQuiere = (clienteActual.nombreCliente.Trim().ToLower() == "carlos" && manager.carlosPidioCerveza);

        if (!clientePideCerveza && !esCarlosYQuiere)
        {
            return "El cliente actual no ha pedido cerveza";
        }

        return "Presiona [E] para tomar un vaso";
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

        TomarVaso();
    }

    private void TomarVaso()
    {
        if (ControladorMano3D.Instance == null || itemVasoVacio == null)
            return;

        // Equipamos el vaso vacío en la mano de Lucas
        ControladorMano3D.Instance.EquiparItem(itemVasoVacio);

        // Notificamos al Act1Manager que el jugador tiene un objeto en mano
        if (Act1Manager.Instance != null)
        {
            Act1Manager.Instance.tieneObjetoEnMano = true;
        }

        Debug.Log("[PuntoVasos] Vaso vacío equipado en mano para pedido de cerveza.");
    }

    public void RestaurarVaso()
    {
        if (vasoDisponible != null)
        {
            vasoDisponible.SetActive(true);
        }
    }
}