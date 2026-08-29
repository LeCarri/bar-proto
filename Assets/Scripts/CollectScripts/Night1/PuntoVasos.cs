using UnityEngine;

public class PuntoVasos : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private ItemSO itemVasoVacio;

    [Header("Visual")]
    [SerializeField] private GameObject vasoDisponible;

    private bool vasoTomado = false;

    public bool CanInteract()
    {
        // Si ya tomamos el vaso, no se puede volver a tomar.
        if (vasoTomado)
            return false;

        // Si Lucas ya tiene algo en la mano, tampoco.
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
        Act1Manager manager = Act1Manager.Instance;

        if (manager == null)
            return;

        // Todavía no empezó el servicio
        if (manager.estadoActual != Act1Manager.ActoState.Servicio)
        {
            manager.MostrarDialogo("Lucas: Ahora no necesito un vaso.");
            return;
        }

        // Carlos todavía no pidió
        if (manager.clientesAtendidosTotal == 0 &&
            !manager.carlosPidioCerveza)
        {
            manager.MostrarDialogo(
                "Lucas: Primero debería ver qué quiere tomar el cliente."
            );
            return;
        }

        // Por ahora este sistema es solamente para la cerveza de Carlos
        if (manager.clientesAtendidosTotal != 0)
        {
            manager.MostrarDialogo(
                "Lucas: Ahora no necesito otro vaso."
            );
            return;
        }

        TomarVaso();
    }

    private void TomarVaso()
    {
        if (ControladorMano3D.Instance == null)
            return;

        vasoTomado = true;

        // Desaparece solamente el vaso elegido.
        if (vasoDisponible != null)
        {
            vasoDisponible.SetActive(false);
        }

        // Aparece el vaso vacío en la mano.
        ControladorMano3D.Instance.EquiparItem(itemVasoVacio);
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