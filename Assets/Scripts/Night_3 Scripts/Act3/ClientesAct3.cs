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
        }
    }
}