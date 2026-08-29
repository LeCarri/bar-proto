using UnityEngine;

public class PuntoVasosAct2 : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private ItemSO itemVasoVacio;

    [Header("Visual")]
    [SerializeField] private GameObject vasoDisponible;

    private bool vasoTomado = false;

    public bool CanInteract()
    {
        Act2Manager manager = Act2Manager.Instance;

        if (manager == null)
            return false;

        // Solo durante la fase de servicio
        if (manager.estadoActual != Act2Manager.Act2State.Servicio)
            return false;

        // Si ya tomamos este vaso, no volvemos a agarrarlo
        if (vasoTomado)
            return false;

        // Si Lucas ya tiene algo en la mano, tampoco
        if (ControladorMano3D.Instance != null &&
            ControladorMano3D.Instance.TieneManoOcupada())
        {
            return false;
        }

        return true;
    }

    public string GetDescription()
    {
        return "Presiona [E] para tomar un vaso";
    }

    public void Interact()
    {
        Act2Manager manager = Act2Manager.Instance;

        if (manager == null)
            return;

        if (manager.estadoActual != Act2Manager.Act2State.Servicio)
        {
            manager.MostrarDialogo(
                "Lucas: Ahora no necesito preparar bebidas."
            );
            return;
        }

        if (ControladorMano3D.Instance == null)
        {
            Debug.LogError(
                "[PuntoVasosAct2] No se encontró ControladorMano3D."
            );
            return;
        }

        if (ControladorMano3D.Instance.TieneManoOcupada())
            return;

        TomarVaso();
    }

    private void TomarVaso()
    {
        vasoTomado = true;

        if (vasoDisponible != null)
        {
            vasoDisponible.SetActive(false);
        }

        ControladorMano3D.Instance.EquiparItem(itemVasoVacio);

        Debug.Log("[PuntoVasosAct2] Vaso vacío recogido.");
    }

    public void RestaurarVaso()
    {
        vasoTomado = false;

        if (vasoDisponible != null)
        {
            vasoDisponible.SetActive(true);
        }
    }
}