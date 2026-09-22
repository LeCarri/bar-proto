using UnityEngine;

public class PuntoSuministroWhiskyTest : MonoBehaviour, IInteractable
{
    [Header("Servicio")]
    [SerializeField] private ServicioWhiskyFPS servicioWhisky;

    public bool CanInteract()
    {
        if (servicioWhisky == null)
            return false;

        // Mientras se reproduce la animación no permitimos
        // volver a iniciar el servicio.
        if (servicioWhisky.EstaSirviendo)
            return false;

        return true;
    }

    public string GetDescription()
    {
        if (!CanInteract())
            return "";

        return "Presiona [E] para servir whisky";
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

        servicioWhisky.IniciarServicio();
    }
}