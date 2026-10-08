using UnityEngine;

public class TriggerNanielaPasillo : MonoBehaviour
{
    [Header("Referencia")]
    public MarielaPeekPasillo mariela;

    [Header("Acción")]
    public bool mostrarMasCabeza;
    public bool ocultarYDesaparecer;

    private bool usado = false;


    private void OnTriggerEnter(Collider other)
    {
        if (usado)
            return;


        PlayerHealth jugador =
            other.GetComponentInParent<PlayerHealth>();

        if (jugador == null)
            return;


        if (mariela == null)
        {
            Debug.LogError(
                "[TriggerNaniela] Falta asignar Mariela en " +
                gameObject.name
            );

            return;
        }


        // ==========================================
        // IMPORTANTE:
        // EL EVENTO TODAVÍA NO EMPEZÓ
        // ==========================================

        if (!mariela.gameObject.activeInHierarchy)
        {
            Debug.Log(
                "[TriggerNaniela] " +
                gameObject.name +
                " ignorado porque Mariela todavía está inactiva."
            );

            return;
        }


        // Recién ahora consumimos el trigger.
        usado = true;


        if (mostrarMasCabeza)
        {
            Debug.Log(
                "[TriggerNaniela] ASOMO 2"
            );

            mariela.MostrarMasCabeza();
        }


        if (ocultarYDesaparecer)
        {
            Debug.Log(
                "[TriggerNaniela] OCULTAR"
            );

            mariela.OcultarYDesaparecer();
        }
    }
}