using UnityEngine;
using System.Collections;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Tarea 3, baño de mujeres.
///
/// Guion: "entramos en el primer cubículo para limpiar, y al salir vemos una sombra que pasa de un
/// cubículo al otro en los últimos dos, mientras se escucha un llanto de mujer o un grito lejano.
/// Al ir a limpiar no hay nada."
///
/// Cómo funciona:
///  1. Cuando se termina de limpiar la "zonaQueLaHabilita" (el inodoro del primer cubículo),
///     el evento queda ARMADO.
///  2. Si este GameObject tiene un Collider "Is Trigger" (que cubra el primer cubículo), la sombra
///     pasa cuando el jugador SALE del trigger. Si no tiene collider, pasa a los "delaySinTrigger" segundos.
///  3. La sombra va de puntoA a puntoB y desaparece.
/// </summary>
public class SombraPasaCubiculoReestructura : MonoBehaviour
{
    [Header("Disparo")]
    [Tooltip("Inodoro del PRIMER cubículo. Al terminar de limpiarlo se arma el evento.")]
    public ZonaLimpiezaReestructura zonaQueLaHabilita;
    public float delaySinTrigger = 1.5f;

    [Header("Sombra")]
    [Tooltip("Modelo/plano de la sombra. Debe empezar DESACTIVADO.")]
    public GameObject sombra;
    [Tooltip("Cubículo desde donde sale (anteúltimo).")]
    public Transform puntoA;
    [Tooltip("Cubículo adonde entra (último).")]
    public Transform puntoB;
    public float velocidad = 5f;

    [Header("Audio")]
    [Tooltip("Llanto de mujer o grito lejano.")]
    public AudioSource llantoOGrito;

    public float paranoia = 8f;

    private bool armado;
    private bool usado;
    private bool jugadorDentro;

    void Start()
    {
        if (sombra != null) sombra.SetActive(false);
        if (zonaQueLaHabilita != null) zonaQueLaHabilita.Completada += AlLimpiarZona;
        else Debug.LogWarning($"[SombraPasaCubiculoReestructura] '{name}': falta asignar 'zonaQueLaHabilita'.");
    }

    void OnDestroy()
    {
        if (zonaQueLaHabilita != null) zonaQueLaHabilita.Completada -= AlLimpiarZona;
    }

    void AlLimpiarZona(ZonaLimpiezaReestructura zona)
    {
        if (usado) return;
        armado = true;

        Collider c = GetComponent<Collider>();
        bool usaTrigger = c != null && c.isTrigger;
        // Sin trigger, o si el jugador limpió desde afuera del cubículo → pasa con un delay
        if (!usaTrigger || !jugadorDentro)
            StartCoroutine(DispararConDelay());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) jugadorDentro = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        jugadorDentro = false;
        if (armado) Disparar();
    }

    IEnumerator DispararConDelay()
    {
        yield return new WaitForSeconds(delaySinTrigger);
        Disparar();
    }

    /// <summary>También se puede llamar a mano (por ejemplo desde un UnityEvent).</summary>
    public void Disparar()
    {
        if (usado) return;
        usado = true;
        armado = false;
        StartCoroutine(Pasar());
    }

    IEnumerator Pasar()
    {
        if (llantoOGrito != null) llantoOGrito.Play();
        Act2ManagerReestructura.Instance?.SumarParanoia(paranoia);

        if (sombra == null || puntoA == null || puntoB == null)
        {
            Debug.LogWarning($"[SombraPasaCubiculoReestructura] '{name}': falta la sombra o los puntos A/B — solo suena el audio.");
            yield break;
        }

        sombra.transform.position = puntoA.position;
        Vector3 dir = puntoB.position - puntoA.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f) sombra.transform.rotation = Quaternion.LookRotation(dir);
        sombra.SetActive(true);

        while (Vector3.Distance(sombra.transform.position, puntoB.position) > 0.05f)
        {
            sombra.transform.position = Vector3.MoveTowards(sombra.transform.position, puntoB.position, velocidad * Time.deltaTime);
            yield return null;
        }

        sombra.SetActive(false);   // "Al ir a limpiar no hay nada"
    }
}
