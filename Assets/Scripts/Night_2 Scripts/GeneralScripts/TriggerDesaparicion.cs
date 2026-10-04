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
            Debug.Log("[TriggerDesaparicion] El jugador pas� por el trigger pero no tiene la llave todav�a.");
            return;
        }

        disparado2 = true;

        // Buscar una instancia de FiguraNino en la escena y llamar al m�todo de instancia
        FiguraNino figuraNino = FindAnyObjectByType<FiguraNino>();
        if (figuraNino != null)
        {
            // ---- VERSIÓN ANTERIOR (comentada en la reestructura) ----
            // StartCoroutine(figuraNino.DesapareceYVozSotano());

            // ---- REESTRUCTURA NOCHE 2 ----
            // La niña desaparece y queda el dibujo familiar (FiguraNino ya lo hace solo por cercanía;
            // este trigger es una alternativa si preferís marcar el punto exacto).
            figuraNino.Desaparecer();
        }
        else
        {
            Debug.LogWarning("[TriggerDesaparicion] No se encontr� ninguna instancia de FiguraNino en la escena.");
        }
    }
}