using UnityEngine;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Trigger genérico. Avisa al Act2ManagerReestructura cuando el jugador entra.
/// El Manager decide si corresponde según la fase: si todavía no es el momento no pasa nada
/// y el trigger sigue esperando.
///
/// Tipos:
///  - ParpadeoSalidaBanos:   entrada del pasillito de los baños. Al volver al salón con las tareas
///                           terminadas → Parpadeo N°1 (bar destruido un segundo) y empieza el servicio.
///  - SilenciarGolpesSotano: cerca de la puerta del sótano → los golpes se apagan.
///  - FrenoEscaleraSotano:   primer escalón del sótano (después de abrir la puerta) → Lucas se frena.
///
/// SETUP: GameObject vacío + Box Collider con "Is Trigger" + este script. El jugador necesita el tag "Player".
/// </summary>
[RequireComponent(typeof(Collider))]
public class TriggerZonaReestructura : MonoBehaviour
{
    public TipoTriggerReestructura tipo = TipoTriggerReestructura.ParpadeoSalidaBanos;

    private bool usado;

    void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other) => Probar(other);
    void OnTriggerStay(Collider other) => Probar(other);   // por si la fase cambia con el jugador adentro

    void Probar(Collider other)
    {
        if (usado || !other.CompareTag("Player")) return;

        Act2ManagerReestructura m = Act2ManagerReestructura.Instance;
        if (m == null) return;

        // El Manager devuelve true si el trigger se usó (y no tiene que volver a dispararse)
        if (m.TriggerZona(tipo)) usado = true;
    }

    void OnDrawGizmos()
    {
        BoxCollider b = GetComponent<BoxCollider>();
        if (b == null) return;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(b.center, b.size);
    }
}
