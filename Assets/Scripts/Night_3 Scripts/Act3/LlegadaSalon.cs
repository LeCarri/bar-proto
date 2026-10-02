using UnityEngine;

public class LlegadaSalonAct3 : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject jugador;

    [Header("Siluetas")]
    public GameObject clientesActo3;

    private bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(
            "[SALON] Entró algo al trigger: " +
            other.gameObject.name
        );

        Debug.Log(
            "[SALON] Jugador asignado: " +
            (jugador != null ? jugador.name : "NULL")
        );

        if (activado)
        {
            Debug.Log("[SALON] Ya estaba activado.");
            return;
        }

        if (jugador == null)
        {
            Debug.Log("[SALON] ERROR: jugador está vacío.");
            return;
        }

        if (other.gameObject != jugador)
        {
            Debug.Log(
                "[SALON] El objeto que entró NO coincide con jugador."
            );
            return;
        }

        Debug.Log("[SALON] El jugador coincide.");

        if (Act3Manager.Instance == null)
        {
            Debug.Log("[SALON] ERROR: no existe Act3Manager.");
            return;
        }

        Debug.Log(
            "[SALON] elementosGuardados = " +
            Act3Manager.Instance.elementosGuardados
        );

        if (!Act3Manager.Instance.elementosGuardados)
        {
            Debug.Log(
                "[SALON] Entró, pero todavía no guardó los elementos."
            );
            return;
        }

        activado = true;

        Debug.Log(
            "[SALON] ¡Llegó al salón después de guardar los elementos!"
        );

        if (clientesActo3 != null)
        {
            clientesActo3.SetActive(true);

            Debug.Log(
                "[SALON] clientesActo3 ACTIVADO."
            );
        }
        else
        {
            Debug.Log(
                "[SALON] ERROR: clientesActo3 está vacío."
            );
        }

        Act3Manager.Instance.IniciarSecuenciaSalon();

        Debug.Log(
            "[SALON] Secuencia del salón iniciada."
        );
    }
}