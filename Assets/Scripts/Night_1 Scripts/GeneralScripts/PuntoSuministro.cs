using UnityEngine;

public class PuntoSuministro : MonoBehaviour, IInteractable
{
    public bool esBarra;

    public void Interact()
    {
        FindObjectOfType<Act1Manager>().RecogerObjeto(esBarra);
    }

    public string GetDescription() => "Presiona [E] para recoger pedido";
}