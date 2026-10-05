using UnityEngine;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Copia de CollectScripts/Night2/PuntoSuministroAct2.cs adaptada al
/// Act2ManagerReestructura. (El original sigue intacto para la demo.)
///
/// La canilla de cerveza: con el vaso vacío en la mano, [E] sirve la cerveza
/// (lo hace el Manager con ServicioCervezaVisual) y la cerveza llena queda en la mano.
/// Solo funciona durante el SERVICIO.
///
/// SETUP: en la canilla (Collider + capa "Interactable"). En el Act2ManagerReestructura hay que
/// completar la sección "Servicio de cerveza" (Item Cerveza, Item Vaso Vacio, Vaso Servicio,
/// Servicio Cerveza Visual).
/// </summary>
public class PuntoSuministroAct2Reestructura : MonoBehaviour, IInteractable
{
    public bool CanInteract()
    {
        Act2ManagerReestructura manager = Act2ManagerReestructura.Instance;
        if (manager == null) return false;

        // Solo se puede usar durante el servicio
        if (manager.estadoActual != Act2ManagerReestructura.Act2State.Servicio) return false;
        if (ControladorMano3D.Instance == null) return false;

        // Solo con el vaso vacío en la mano
        return ControladorMano3D.Instance.ObtenerItemActual() == manager.itemVasoVacio;
    }

    public string GetDescription()
    {
        return "Presiona [E] para servir cerveza";
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        Act2ManagerReestructura.Instance.ColocarVasoEnCanilla();
    }
}
