using UnityEngine;

/// <summary>
/// Colocar en un GameObject con un Collider marcado como "Is Trigger" cerca de la puerta del sótano.
/// Cuando el jugador lo atraviesa con la llave, inicia la secuencia de cierre automáticamente.
/// No requiere ser habilitado desde Act2Manager — funciona de forma autónoma.
///
/// SETUP:
///  1. Crear un GameObject vacío frente a la puerta del sótano.
///  2. Agregar un Box Collider → marcar "Is Trigger".
///  3. Agregar este script.
///  4. (Opcional) Arrastrarlo al campo "Trigger Cierre Sotano" del Act2Manager para referencia.
///  5. El jugador DEBE tener el tag "Player".
///
/// IMPORTANTE: Dejar "Bloqueadores Salidas" vacío en SombrasCombate si querés que
/// el jugador pueda ir a la puerta sin necesidad de matar sombras.
/// </summary>
public class TriggerCierreSotano : MonoBehaviour
{
    private bool disparado = false;

    void OnTriggerEnter(Collider other)
    {
        if (disparado) return;
        if (!other.CompareTag("Player")) return;

        Act2Manager manager = Act2Manager.Instance;
        if (manager == null)
        {
            Debug.LogError("[TriggerCierreSotano] Act2Manager no encontrado en la escena.");
            return;
        }

        // Solo dispara si el jugador tiene la llave (state Psicosis o posterior)
        if (!manager.TieneLlave())
        {
            Debug.Log("[TriggerCierreSotano] El jugador pasó por el trigger pero no tiene la llave todavía.");
            return;
        }

        disparado = true;
        manager.UsarLlave();
    }
}
