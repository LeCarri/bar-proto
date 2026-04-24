using UnityEngine;

public class TriggerOlor : MonoBehaviour
{
    private bool yaSeActivo = false;

    void OnTriggerEnter(Collider other)
    {
        // 1. Verificamos que sea el Player
        if (other.CompareTag("Player") && !yaSeActivo)
        {
            Act1Manager manager = FindObjectOfType<Act1Manager>();

            // 2. Solo se activa si Lucas ya está en la fase de buscar el segundo pedido
            // (cuando ya atendió al menos a 1 cliente pero aún no terminó el Acto)
            if (manager != null && manager.clientesAtendidosTotal >= 1)
            {
                EjecutarEventoOlor(manager);
            }
        }
    }

    void EjecutarEventoOlor(Act1Manager manager)
    {
        yaSeActivo = true;

        // Diálogo de Lucas (aprovechando su tono barítono, esto debería sonar pesado)
        manager.MostrarDialogo("Lucas: Ugh... qué olor a podrido. Viene del sótano... ¿Otra rata muerta?");
    }
}