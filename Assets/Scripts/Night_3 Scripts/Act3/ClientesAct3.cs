using UnityEngine;

public class ClienteAct3 : MonoBehaviour
{
    public string nombreCliente;

    [TextArea]
    public string dialogoPedido;

    public string itemPedido;

    [TextArea]
    public string respuesta;

    bool pedidoTomado = false;

    public void Interact()
    {
        Debug.Log("ENTR� A INTERACT");

        // PRIMERA INTERACCI�N
        if (!pedidoTomado)
        {
            // Si ya hay un pedido activo, no deja tomar otro
            if (Act3Manager.Instance.tienePedido)
            {
                Act3Manager.Instance.MostrarDialogo(
                    "No puedo dejar esto a medias... primero tengo que terminar con el otro pedido."
                );
                return;
            }

            Debug.Log("Mostrando pedido");

            Act3Manager.Instance.MostrarDialogo(nombreCliente + ": " + dialogoPedido);
            Act3Manager.Instance.TomarPedido(itemPedido);

            pedidoTomado = true;
        }

        // SEGUNDA INTERACCI�N
        else
        {
            Debug.Log("Intentando entregar");

            if (Act3Manager.Instance.TienePedidoEntregable())
            {
            Act3Manager.Instance.MostrarDialogo(nombreCliente + ": " + respuesta);      
                Act3Manager.Instance.EntregarPedido();

                Act3Manager.Instance.ClienteCompletado();
            }
            else
            {
                Act3Manager.Instance.MostrarDialogo(
                    "Todavía no... falta algo."
                );
            }
        }
    }
}