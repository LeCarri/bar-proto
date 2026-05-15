using UnityEngine;

public class ClienteAct3 : MonoBehaviour
{
    public string nombreCliente;

    [TextArea]
    public string pedido;

    [TextArea]
    public string respuesta;

    bool pedidoTomado = false;

        public void Interact()
    {
        Debug.Log("ENTRÓ A INTERACT");

        if (!pedidoTomado)
        {
            Debug.Log("Mostrando pedido");

            ClienteDialogueSystem.Instance.MostrarDialogo(
                nombreCliente,
                pedido
            );

            pedidoTomado = true;
            Act3Manager.Instance.TomarPedido();
        }
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
            }
            else
            {
                Act3Manager.Instance.MostrarDialogo(
                    "Todavía no tengo lo que pidió."
                );
            }
        }
    }
}
