using UnityEngine;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Copia de CollectScripts/Night2/PuntoVasosAct2.cs adaptada al
/// Act2ManagerReestructura. (El original sigue intacto para la demo.)
///
/// Lugar donde Lucas toma un vaso vacío con [E]. Solo funciona durante el SERVICIO.
/// Cada vez que se atiende a un cliente normal, el vaso vuelve a aparecer (RestaurarVaso).
///
/// SETUP: en el objeto de los vasos (Collider + capa "Interactable").
///  - Item Vaso Vacio: el MISMO ItemSO que pusiste en "Item Vaso Vacio" del Act2ManagerReestructura.
///  - Vaso Disponible: el modelo del vaso que se esconde al tomarlo (opcional).
/// </summary>
public class PuntoVasosAct2Reestructura : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private ItemSO itemVasoVacio;

    [Header("Visual")]
    [SerializeField] private GameObject vasoDisponible;

    private bool vasoTomado = false;

    public bool CanInteract()
    {
        Act2ManagerReestructura manager = Act2ManagerReestructura.Instance;
        if (manager == null) return false;

        // Solo durante la fase de servicio
        if (manager.estadoActual != Act2ManagerReestructura.Act2State.Servicio) return false;

        // Si ya tomamos este vaso, no volvemos a agarrarlo
        if (vasoTomado) return false;

        // Si Lucas ya tiene algo en la mano, tampoco
        if (ControladorMano3D.Instance != null && ControladorMano3D.Instance.TieneManoOcupada()) return false;

        return true;
    }

    public string GetDescription()
    {
        return "Presiona [E] para tomar un vaso";
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        if (ControladorMano3D.Instance == null)
        {
            Debug.LogError("[PuntoVasosAct2Reestructura] No se encontró ControladorMano3D en la escena.");
            return;
        }

        vasoTomado = true;
        if (vasoDisponible != null) vasoDisponible.SetActive(false);
        ControladorMano3D.Instance.EquiparItem(itemVasoVacio);
    }

    public void RestaurarVaso()
    {
        vasoTomado = false;
        if (vasoDisponible != null) vasoDisponible.SetActive(true);
    }
}
