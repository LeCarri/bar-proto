using UnityEngine;

public class PuntoSuministroWhiskyTest : MonoBehaviour, IInteractable
{
    [Header("Servicio")]
    [SerializeField] private ServicioWhiskyFPS servicioWhisky;

    public bool CanInteract()
    {
        if (servicioWhisky == null)
            return false;

        // Mientras se reproduce la animación no permitimos interactuar
        if (servicioWhisky.EstaSirviendo)
            return false;

        // NUEVO: Si el jugador ya tiene un objeto en la mano, no puede servir otro
        if (ControladorMano3D.Instance != null && ControladorMano3D.Instance.TieneManoOcupada())
        {
            return false;
        }

        return true;
    }

    public string GetDescription()
    {
        if (servicioWhisky != null && servicioWhisky.EstaSirviendo)
            return "";

        // Opcional: Mostrar un aviso si intenta interactuar con la mano llena
        if (ControladorMano3D.Instance != null && ControladorMano3D.Instance.TieneManoOcupada())
        {
            return "Ya tenés las manos ocupadas";
        }

        return "Presiona [E] para servir whisky";
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

        servicioWhisky.IniciarServicio();
    }
}