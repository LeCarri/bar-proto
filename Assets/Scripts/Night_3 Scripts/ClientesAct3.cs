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

        // Primera interacción 
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

        // Segunda interacción 
        else
        {
            Debug.Log("Intentando entregar");

            Debug.Log("Tiene pedido entregable: " +
                Act3Manager.Instance.TienePedidoEntregable());

            if (Act3Manager.Instance.TienePedidoEntregable())
            {
                ClienteDialogueSystem.Instance.MostrarDialogo(
                    nombreCliente,
                    respuesta
                );

                Act3Manager.Instance.EntregarPedido();

                Act3Manager.Instance.StartCoroutine(
                    Act3Manager.Instance.SoltarCliente()
                );

                Act3Manager.Instance.ClienteCompletado();
            }
        }
    }
}
