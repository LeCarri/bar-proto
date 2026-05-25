using UnityEngine;

public class PedidoPickup : MonoBehaviour
{
    public string nombreObjeto;

    public void Interact()
    {
        Debug.Log("Recogiendo pedido: " + nombreObjeto);

        if (Act3Manager.Instance != null)
        {
            Act3Manager.Instance.RecogerPedido(nombreObjeto);
        }
    }
}
