using UnityEngine;

public class PuntoVasos : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private ItemSO itemVasoVacio;

    [Header("Visual (Opcional)")]
    [SerializeField] private GameObject vasoDisponible;

    public bool CanInteract()
    {
        // Se puede tomar un vaso siempre que la mano del jugador esté libre
        if (ControladorMano3D.Instance != null && ControladorMano3D.Instance.TieneManoOcupada())
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

        // Si es el inicio y Carlos todavía no pidió
        if (manager.clientesAtendidosTotal == 0 && !manager.carlosPidioCerveza)
        {
            manager.MostrarDialogo("Lucas: Primero debería ver qué quiere tomar el cliente.");
            return;
        }

        TomarVaso();
    }

    private void TomarVaso()
    {
        if (ControladorMano3D.Instance == null || itemVasoVacio == null)
            return;

        // Equipamos el vaso vacío en la mano de Lucas
        ControladorMano3D.Instance.EquiparItem(itemVasoVacio);

        // Notificamos al Act1Manager que el jugador tiene un objeto en mano
        if (Act1Manager.Instance != null)
        {
            Act1Manager.Instance.tieneObjetoEnMano = true;
        }

        Debug.Log("[PuntoVasos] Vaso vacío equipado en mano.");
    }

    // Mantenemos el método por compatibilidad con tus otros scripts si lo llaman
    public void RestaurarVaso()
    {
        if (vasoDisponible != null)
        {
            vasoDisponible.SetActive(true);
        }
    }
}