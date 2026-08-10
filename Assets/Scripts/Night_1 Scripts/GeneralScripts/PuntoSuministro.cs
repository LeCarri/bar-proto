using UnityEngine;

public class PuntoSuministro : MonoBehaviour, IInteractable
{
    [Header("Configuración del Punto")]
    public bool esBarra;

    public bool CanInteract()
    {
        // Opcional: Solo permite interactuar si la mano del jugador está libre
        if (ControladorMano3D.Instance != null)
        {
            return !ControladorMano3D.Instance.TieneManoOcupada();
        }

        return true;
    }

    public string GetDescription() => "Presiona [E] para recoger pedido";

    public void Interact()
    {
        // Llamamos al manager de la Act 1 como venías haciendo
        Act1Manager manager = FindObjectOfType<Act1Manager>();

        if (manager != null)
        {
            manager.RecogerObjeto(esBarra);
        }
    }
}