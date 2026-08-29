using UnityEngine;

public class PuntoSuministroAct2 : MonoBehaviour, IInteractable
{
    public bool CanInteract()
    {
        Act2Manager manager = Act2Manager.Instance;

        if (manager == null)
            return false;

        // Solo se puede usar durante el servicio
        if (manager.estadoActual != Act2Manager.Act2State.Servicio)
            return false;

        if (ControladorMano3D.Instance == null)
            return false;

        // Solo permitimos usar las canillas si tiene el vaso vacío
        return ControladorMano3D.Instance.ObtenerItemActual()
               == manager.itemVasoVacio;
    }

    public string GetDescription()
    {
        return "Presiona [E] para servir cerveza";
    }

    public void Interact()
    {
        Act2Manager manager = Act2Manager.Instance;

        if (manager == null)
            return;

        if (manager.estadoActual != Act2Manager.Act2State.Servicio)
        {
            manager.MostrarDialogo(
                "Lucas: Ahora no es momento de preparar bebidas."
            );
            return;
        }

        if (ControladorMano3D.Instance == null)
            return;

        if (ControladorMano3D.Instance.ObtenerItemActual()
            != manager.itemVasoVacio)
        {
            manager.MostrarDialogo(
                "Lucas: Necesito un vaso primero."
            );
            return;
        }

        manager.ColocarVasoEnCanilla();
    }
}