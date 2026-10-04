using UnityEngine;

/// <summary>
/// NOCHE 2 — Cliente "normal" del servicio.
/// Guion: "vamos a atender a varios clientes que se van activando de a uno, y el último es el corrupto".
///
/// Flujo: [E] toma el pedido → el jugador prepara la bebida (vaso + canilla, igual que ahora) →
/// [E] con la bebida en la mano para entregar → el Manager habilita al siguiente cliente.
/// Cuando todos los ClienteAct2 fueron atendidos, se habilita el ClienteCorrupto.
///
/// SETUP:
///  - Poner en cada cliente normal (con Collider + capa "Interactable").
///  - Arrastrarlos EN ORDEN al array "Clientes Normales En Orden" del Act2Manager.
///  - itemPedido: el ItemSO que pide (por ejemplo el de cerveza). Si queda vacío, al tomarle el
///    pedido ya cuenta como atendido (útil para probar rápido).
/// </summary>
public class ClienteAct2 : MonoBehaviour, IInteractable
{
    public enum Estado { EsperandoAtencion, EsperandoPedido, Atendido }

    [Header("Estado")]
    public Estado estado = Estado.EsperandoAtencion;
    [Tooltip("Lo maneja el Act2Manager. No tocar.")]
    public bool esSuTurno = false;

    [Header("Cliente")]
    public string nombreCliente = "Cliente";
    [TextArea] public string dialogoPedido = "Una cerveza, por favor.";
    [TextArea] public string dialogoGracias = "Gracias.";
    [Tooltip("ItemSO de la bebida que pide. Vacío = no pide nada.")]
    public ItemSO itemPedido;

    [Header("Visual")]
    [Tooltip("Indicador que aparece sobre el cliente cuando es su turno (opcional).")]
    public GameObject indicadorTurno;

    void Update()
    {
        if (indicadorTurno == null) return;
        bool mostrar = esSuTurno && estado != Estado.Atendido;
        if (indicadorTurno.activeSelf != mostrar) indicadorTurno.SetActive(mostrar);
    }

    public bool CanInteract() => esSuTurno && estado != Estado.Atendido;

    public string GetDescription()
    {
        if (!CanInteract()) return "";
        return estado == Estado.EsperandoAtencion ? "Presiona [E] para tomar pedido" : "Presiona [E] para entregar pedido";
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        Act2Manager m = Act2Manager.Instance;
        if (m == null) return;

        if (estado == Estado.EsperandoAtencion)
        {
            if (!string.IsNullOrEmpty(dialogoPedido)) m.MostrarDialogo(nombreCliente + ": " + dialogoPedido);
            estado = Estado.EsperandoPedido;
            if (itemPedido == null) Atender(m);
            return;
        }

        // Entregar
        ItemSO enMano = ControladorMano3D.Instance != null ? ControladorMano3D.Instance.ObtenerItemActual() : null;
        bool correcto = enMano != null &&
                        (enMano == itemPedido ||
                         (!string.IsNullOrEmpty(enMano.nombreItem) &&
                          string.Equals(enMano.nombreItem, itemPedido.nombreItem, System.StringComparison.OrdinalIgnoreCase)));

        if (!correcto) return;   // sin diálogo agregado: el cartel sigue diciendo "entregar pedido"

        if (ControladorMano3D.Instance != null) ControladorMano3D.Instance.VaciarMano();
        if (!string.IsNullOrEmpty(dialogoGracias)) m.MostrarDialogo(nombreCliente + ": " + dialogoGracias);
        Atender(m);
    }

    void Atender(Act2Manager m)
    {
        estado = Estado.Atendido;
        esSuTurno = false;
        if (indicadorTurno != null) indicadorTurno.SetActive(false);

        // Para que se pueda volver a tomar un vaso para el próximo cliente
        foreach (PuntoVasosAct2 punto in Object.FindObjectsByType<PuntoVasosAct2>(FindObjectsSortMode.None))
            punto.RestaurarVaso();

        m.ClienteNormalAtendido(this);
    }
}
