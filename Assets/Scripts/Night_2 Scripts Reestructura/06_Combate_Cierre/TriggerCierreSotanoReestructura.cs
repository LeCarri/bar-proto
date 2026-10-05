using UnityEngine;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Copia de TriggerCierreSotano.cs adaptada al Act2ManagerReestructura.
/// (El original sigue intacto para la demo.)
///
/// "Al llegar, las Sombras siguen viniendo por detrás, Lucas introduce la llave y la puerta se abre."
/// Cuando el jugador llega a la puerta del sótano CON la llave, arranca solo la apertura
/// (sin tener que apretar [E]: mientras lo persiguen es más fluido).
///
/// SETUP:
///  1. GameObject vacío frente a la puerta del sótano.
///  2. Box Collider → marcar "Is Trigger" (aprox. 2 x 2 x 1.5, del lado del salón).
///  3. Agregar este script. El jugador tiene que tener el tag "Player".
/// </summary>
[RequireComponent(typeof(Collider))]
public class TriggerCierreSotanoReestructura : MonoBehaviour
{
    private bool disparado;

    void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other) => Probar(other);
    void OnTriggerStay(Collider other) => Probar(other);   // por si llega la llave con el jugador ya adentro

    void Probar(Collider other)
    {
        if (disparado || !other.CompareTag("Player")) return;

        Act2ManagerReestructura manager = Act2ManagerReestructura.Instance;
        if (manager == null || !manager.PuedeUsarLlave()) return;

        disparado = true;
        manager.UsarLlave();
    }
}
