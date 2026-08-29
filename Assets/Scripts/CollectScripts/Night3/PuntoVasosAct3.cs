using UnityEngine;

public class PuntoVasosAct3 : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private ItemSO itemVasoVacio;

    [Header("Visual")]
    [SerializeField] private GameObject vasoDisponible;

    private bool vasoTomado = false;

    public bool PuedeInteractuar()
    {
        Act3Manager manager = Act3Manager.Instance;

        if (manager == null)
            return false;

        // Solo durante la fase de servicio.
        if (!manager.PuedeUsarServicioBebidas())
            return false;

        // Ya sacamos el vaso.
        if (vasoTomado)
            return false;

        // No puede agarrar otro objeto si ya tiene algo en la mano.
        if (ControladorMano3D.Instance != null &&
            ControladorMano3D.Instance.TieneManoOcupada())
        {
            return false;
        }

        return true;
    }

    public void Interact()
    {
        if (!PuedeInteractuar())
            return;

        if (ControladorMano3D.Instance == null)
        {
            Debug.LogError(
                "[PuntoVasosAct3] No se encontró ControladorMano3D."
            );
            return;
        }

        vasoTomado = true;

        if (vasoDisponible != null)
        {
            vasoDisponible.SetActive(false);
        }

        ControladorMano3D.Instance.EquiparItem(itemVasoVacio);

        Debug.Log("[PuntoVasosAct3] Vaso vacío recogido.");
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