using UnityEngine;

public class LinternaInteractuable : MonoBehaviour, IInteractable
{
    [Header("Referencia a la Linterna del Jugador")]
    public GameObject linternaJugadorMano; // La linterna en la mano del jugador

    [Header("Feedback de Interacción")]
    public string mensajeInteraccion = "Presiona [E] para tomar la linterna";
    public string descripcionInteraccion = "Linterna de mano";

    // SOLO PERMITE INTERACTUAR SI EL ACT1MANAGER ESTÁ EN EL ESTADO DE QUIEBRE
    public bool CanInteract()
    {
        if (Act1Manager.Instance != null)
        {
            // Verificamos el nombre del estado en texto para evitar problemas con el enum anidado
            return Act1Manager.Instance.estadoActual.ToString() == "Quiebre";
        }
        return false;
    }

    public string GetDescription()
    {
        return descripcionInteraccion;
    }

    public string GetInteractText()
    {
        return mensajeInteraccion;
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        // 1. Activar la linterna en la mano del jugador
        if (linternaJugadorMano != null)
        {
            linternaJugadorMano.SetActive(true);
            Debug.Log("[LinternaInteractuable] Linterna en la mano ACTIVADA.");
        }
        else
        {
            Debug.LogWarning("[LinternaInteractuable] No se asignó la linterna de la mano.");
        }

        // 2. Notificar al Act1Manager para avanzar la historia (GDD)
        if (Act1Manager.Instance != null)
        {
            Act1Manager.Instance.AlRecogerLinternaBarra();
        }

        // 3. Desactivar este objeto de la barra
        gameObject.SetActive(false);
    }
}