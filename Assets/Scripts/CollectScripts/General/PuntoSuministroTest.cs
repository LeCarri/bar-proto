using UnityEngine;

public class PuntoSuministroTest : MonoBehaviour, IInteractable
{
    [Header("Servicio")]
    [SerializeField] private ServicioCervezaTest servicioCerveza;

    public bool CanInteract()
    {
        if (servicioCerveza == null)
            return false;

        if (servicioCerveza.EstaSirviendo)
            return false;

        // Ya dejamos el vaso debajo de la canilla.
        if (servicioCerveza.VasoEnCanilla)
            return true;

        // Tenemos un vaso vacío en la mano.
        if (ControladorMano3D.Instance != null &&
            ControladorMano3D.Instance.ObtenerItemActual() ==
            servicioCerveza.ItemVasoVacio)
        {
            return true;
        }

        return false;
    }

    public string GetDescription()
    {
        if (servicioCerveza == null)
            return "";

        if (servicioCerveza.VasoEnCanilla)
            return "Presiona [E] para servir cerveza";

        if (ControladorMano3D.Instance != null &&
            ControladorMano3D.Instance.ObtenerItemActual() ==
            servicioCerveza.ItemVasoVacio)
        {
            return "Presiona [E] para colocar el vaso";
        }

        return "";
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

        if (servicioCerveza.VasoEnCanilla)
        {
            servicioCerveza.Servir();
            return;
        }

        servicioCerveza.ColocarVaso();
    }
}