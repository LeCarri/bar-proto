using UnityEngine;

public class PuntoSuministro : MonoBehaviour, IInteractable
{
    [Header("Configuración del Punto")]
    public bool esBarra;

    public bool CanInteract()
    {
        Act1Manager manager = Act1Manager.Instance;

        if (manager == null)
        {
            manager = FindObjectOfType<Act1Manager>();
        }

        if (manager == null)
            return false;

        // En la barra necesitamos poder interactuar:
        // - si tenemos un vaso vacío
        // - si ya hay un vaso colocado para servir
        if (esBarra)
        {
            if (manager.vasoEnCanilla)
                return true;

            if (ControladorMano3D.Instance != null &&
                ControladorMano3D.Instance.ObtenerItemActual() == manager.itemVasoVacio)
            {
                return true;
            }
        }

        // Para el resto del sistema mantenemos la lógica anterior:
        // solo interactuar si la mano está libre.
        if (ControladorMano3D.Instance != null)
        {
            return !ControladorMano3D.Instance.TieneManoOcupada();
        }

        return true;
    }

    public string GetDescription()
    {
        Act1Manager manager = Act1Manager.Instance;

        if (manager == null)
        {
            manager = FindObjectOfType<Act1Manager>();
        }

        if (manager != null && esBarra)
        {
            // Ya pusimos el vaso debajo de la canilla
            if (manager.vasoEnCanilla)
            {
                return "Presiona [E] para servir cerveza";
            }

            // Tenemos el vaso vacío en la mano
            if (ControladorMano3D.Instance != null &&
                ControladorMano3D.Instance.ObtenerItemActual() == manager.itemVasoVacio)
            {
                return "Presiona [E] para colocar el vaso";
            }
        }

        return "Presiona [E] para interactuar";
    }

    public void Interact()
    {
        Act1Manager manager = Act1Manager.Instance;

        if (manager == null)
        {
            manager = FindObjectOfType<Act1Manager>();
        }

        if (manager == null)
        {
            Debug.LogWarning("[PuntoSuministro] No se encontró Act1Manager.");
            return;
        }

        // SEGUNDO PASO:
        // ya hay un vaso colocado -> servir cerveza
        if (esBarra && manager.vasoEnCanilla)
        {
            manager.ServirCerveza();
            return;
        }

        // PRIMER PASO:
        // tenemos vaso vacío -> colocarlo bajo la canilla
        if (esBarra &&
            ControladorMano3D.Instance != null &&
            ControladorMano3D.Instance.ObtenerItemActual() == manager.itemVasoVacio)
        {
            manager.ColocarVasoEnCanilla();
            return;
        }

        // Mantiene la lógica anterior para otras interacciones.
        manager.RecogerObjeto(esBarra);
    }
}