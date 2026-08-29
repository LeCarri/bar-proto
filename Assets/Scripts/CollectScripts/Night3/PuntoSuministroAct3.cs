using UnityEngine;

public class PuntoSuministroAct3 : MonoBehaviour
{
    public bool PuedeInteractuar()
    {
        Act3Manager manager = Act3Manager.Instance;

        if (manager == null)
            return false;

        if (!manager.PuedeUsarServicioBebidas())
            return false;

        if (ControladorMano3D.Instance == null)
            return false;

        // Solo aparece disponible llevando el vaso vacío.
        return ControladorMano3D.Instance.ObtenerItemActual()
               == manager.itemVasoVacio;
    }

    public void Interact()
    {
        if (!PuedeInteractuar())
            return;

        Act3Manager manager = Act3Manager.Instance;

        if (manager == null)
            return;

        manager.ColocarVasoEnCanilla();
    }
}