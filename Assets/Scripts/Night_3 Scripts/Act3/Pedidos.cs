using UnityEngine;

public class PedidoPickup : MonoBehaviour
{
    [Header("Pedido")]
    public string nombreObjeto;

    private bool recogido = false;

    public void Interact()
    {
        if (recogido)
            return;

        Debug.Log("Intentando recoger pedido: " + nombreObjeto);

        PedidoCompletado();
    }

    public void PedidoCompletado()
    {
        if (recogido)
            return;

        recogido = true;

        Debug.Log("Pedido conseguido: " + nombreObjeto);

        if (Act3Manager.Instance != null)
        {
            Act3Manager.Instance.RecogerPedido(nombreObjeto);
        }

        // El objeto desaparece después de recogerlo.
        gameObject.SetActive(false);
    }
}