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


        usado = true;


        // ==========================================
        // ASOMAR MÁS
        // ==========================================

        if (mostrarMasCabeza)
        {
            Debug.Log(
                "[TriggerNaniela] ASOMO 2"
            );

            mariela.MostrarMasCabeza();

            if (Act1Manager.Instance != null)
            {
                Act1Manager.Instance
                    .IntensificarPasilloMariela();
            }
        }


        // ==========================================
        // OCULTAR Y DESAPARECER
        // ==========================================

        if (ocultarYDesaparecer)
        {
            Debug.Log(
                "[TriggerNaniela] OCULTAR"
            );

            mariela.OcultarYDesaparecer();
        }
    }
}