using UnityEngine;

public class PedidoPickup : MonoBehaviour
{
    [Header("Pedido")]
    public string nombreObjeto;

    [Header("Minijuego")]
    public MinijuegoPedido minijuegoPedido;


    public void Interact()
    {
        Debug.Log("Intentando recoger pedido: " + nombreObjeto);

        if (minijuegoPedido != null)
        {
            minijuegoPedido.IniciarMinijuego(this);
        }

        Debug.LogWarning(
            "No hay un MinijuegoPedido asignado en " + gameObject.name
        );
    }


    public void PedidoCompletado()
    {
        Debug.Log("Pedido conseguido: " + nombreObjeto);

        if (Act3Manager.Instance != null)
        {
            Act3Manager.Instance.RecogerPedido(nombreObjeto);
        }
    }
}
