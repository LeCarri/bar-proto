using UnityEngine;

/// <summary>
/// NOCHE 2 — Tarea 1: la mancha oscura (sangre) que aparece "a lo lejos".
///
/// Guion: "hay 3 montículos para barrer. Cada montículo tiene cerca un decal de esta mancha
/// oscura que no está activado. Cuando falte solo un montículo, se activa la mancha que tiene
/// cerca; cuando nos acercamos para seguir barriendo desaparece, y Lucas dice: ..."
///
/// SETUP:
///  - Crear un GameObject vacío cerca de cada montículo con este script.
///  - Como HIJO, poner el decal / plano de la mancha y arrastrarlo a "visual".
///    (El hijo se apaga solo al empezar; NO apagues este GameObject padre.)
///  - En la ZonaLimpiezaAct2 del montículo, arrastrar este objeto a "Mancha Cercana".
/// </summary>
public class ManchaFantasmaAct2 : MonoBehaviour
{
    [Tooltip("Decal / plano de la mancha (hijo de este objeto).")]
    public GameObject visual;

    [Tooltip("Cuando el jugador se acerca a menos de esta distancia, la mancha desaparece.")]
    public float distanciaDesaparicion = 4f;

    [Tooltip("Segundos mínimos que la mancha se ve antes de poder desaparecer (para que el jugador llegue a verla).")]
    public float tiempoMinimoVisible = 1.5f;

    [TextArea]
    public string dialogoAlDesaparecer = "Lucas: ¿Y eso?... ¿Qué pasó ahí?... No importa… Mejor sigo…";

    public AudioSource sonidoAlDesaparecer;
    public float paranoiaAlDesaparecer = 5f;

    private bool activa;
    private bool yaUsada;
    private float tiempoVisible;
    private Transform jugador;

    void Start()
    {
        if (visual != null) visual.SetActive(false);
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;
    }

    /// <summary>Llamado por el Act2Manager cuando queda un solo montículo.</summary>
    public void Activar()
    {
        if (yaUsada || activa) return;
        activa = true;
        if (visual != null) visual.SetActive(true);
        else Debug.LogWarning("[ManchaFantasmaAct2] Falta asignar 'visual' (el decal de la mancha).");
    }

    void Update()
    {
        if (!activa || jugador == null) return;

        tiempoVisible += Time.deltaTime;
        if (tiempoVisible < tiempoMinimoVisible) return;

        if (Vector3.Distance(jugador.position, transform.position) <= distanciaDesaparicion)
            Desaparecer();
    }

    void Desaparecer()
    {
        activa = false;
        yaUsada = true;
        if (visual != null) visual.SetActive(false);
        if (sonidoAlDesaparecer != null) sonidoAlDesaparecer.Play();

        Act2Manager m = Act2Manager.Instance;
        if (m != null)
        {
            if (!string.IsNullOrEmpty(dialogoAlDesaparecer)) m.MostrarDialogo(dialogoAlDesaparecer);
            m.SumarParanoia(paranoiaAlDesaparecer);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, distanciaDesaparicion);
    }
}
