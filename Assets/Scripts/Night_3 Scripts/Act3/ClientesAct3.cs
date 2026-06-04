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
        Debug.Log("ENTRÓ A INTERACT");

        // PRIMERA INTERACCIÓN
        if (!pedidoTomado)
        {
            // Si ya hay un pedido activo, no deja tomar otro
            if (Act3Manager.Instance.tienePedido)
            {
                Act3Manager.Instance.MostrarDialogo(
                    "Primero debería entregar el pedido actual."
                );
                return;
            }

            Debug.Log("Mostrando pedido");

            ClienteDialogueSystem.Instance.MostrarDialogo(
                nombreCliente,
                dialogoPedido
            );

            Act3Manager.Instance.TomarPedido(itemPedido);

            pedidoTomado = true;
        }

        // SEGUNDA INTERACCIÓN
        else
        {
            Debug.Log("Intentando entregar");

            if (Act3Manager.Instance.TienePedidoEntregable())
            {
                ClienteDialogueSystem.Instance.MostrarDialogo(
                    nombreCliente,
                    respuesta
                );

                Act3Manager.Instance.EntregarPedido();

                Act3Manager.Instance.ClienteCompletado();
            }
            else
            {
                Act3Manager.Instance.MostrarDialogo(
                    "Todavía no tengo tu pedido."
                );
            }
        }
    }
}