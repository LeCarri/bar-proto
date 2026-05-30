using UnityEngine;
public class TriggerDesaparicion : MonoBehaviour
{
    private bool disparado2 = false;

    void OnTriggerEnter(Collider other)
    {
        if (disparado2) return;
        if (!other.CompareTag("Player")) return;

        Act2Manager manager = Act2Manager.Instance;
        if (manager == null)
        {
            Debug.LogError("[TriggerDesaparicion] Act2Manager no encontrado en la escena.");
            return;
        }

        // Solo dispara si el jugador tiene la llave (state Psicosis o posterior)
        if (!manager.TieneLlave())
        {
            Debug.Log("[TriggerDesaparicion] El jugador pasó por el trigger pero no tiene la llave todavía.");
            return;
        }

        disparado2 = true;

        // Buscar una instancia de FiguraNino en la escena y llamar al método de instancia
        FiguraNino figuraNino = FindAnyObjectByType<FiguraNino>();
        if (figuraNino != null)
        {
            StartCoroutine(figuraNino.DesapareceYVozSotano());
        }
        else
        {
            Debug.LogWarning("[TriggerDesaparicion] No se encontró ninguna instancia de FiguraNino en la escena.");
        }
    }
}