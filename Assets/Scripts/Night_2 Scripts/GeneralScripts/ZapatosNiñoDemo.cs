using UnityEngine;

/// <summary>
/// Zapatos de niño encontrados en el estante de la cocina/depósito.
/// Al interactuar, sube la paranoia al 50% y notifica al Act2Manager.
/// Implementa IInteractable para ser detectado por PlayerInteraction.
///
/// SETUP: Colocar en el GameObject del modelo de zapatos en el estante de la cocina.
/// Asegurarse de que el estante esté en el pasillo/cocina, visible para el jugador.
/// </summary>
public class ZapatosNinoDemo : MonoBehaviour, IInteractable
{
    [Header("Estado")]
    private bool yaInteractuado = false;

    [Header("Visual")]
    [Tooltip("Indicador flotante sobre los zapatos (punto de interés)")]
    public GameObject indicadorFlotante;

    [Header("Audio")]
    [Tooltip("Sonido perturbador al encontrar los zapatos (scrape, breathing, etc.)")]
    public AudioSource sonidoDescubrimiento;

    [Header("Paranoia")]
    [Tooltip("Cuánta paranoia sube al encontrar los zapatos. El guion pide llegar al 50%.")]
    public float paranoiaAlEncontrar = 30f;

    // ---- REESTRUCTURA NOCHE 2 ----
    [Header("Reestructura Noche 2")]
    [Tooltip("Los zapatos están ocultos hasta que el cliente corrupto hace el pedido (en las tareas el jugador pasa por el depósito a buscar la escoba).")]
    public bool ocultarHastaElPedido = true;
    [Tooltip("OPCIONAL: la botella que está en el estante durante las tareas. \"Al llegar y buscar, la botella no está\": se oculta cuando aparecen los zapatos.")]
    public GameObject botellaEnElEstante;
    private bool visibles = true;

    void Start()
    {
        if (indicadorFlotante != null) indicadorFlotante.SetActive(true);

        // REESTRUCTURA
        if (ocultarHastaElPedido) MostrarZapatos(false);
    }

    // REESTRUCTURA: aparecen en cuanto empieza la fase Pasillo
    void Update()
    {
        if (visibles) return;
        Act2ManagerDemo m = Act2ManagerDemo.Instance;
        if (m != null && m.estadoActual == Act2ManagerDemo.Act2State.Pasillo) MostrarZapatos(true);
    }

    void MostrarZapatos(bool mostrar)
    {
        visibles = mostrar;
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = mostrar;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = mostrar;
        if (indicadorFlotante != null) indicadorFlotante.SetActive(mostrar && !yaInteractuado);
        if (mostrar && botellaEnElEstante != null) botellaEnElEstante.SetActive(false);
    }

    public void Interact()
    {
        if (yaInteractuado) return;

        // REESTRUCTURA: solo en la fase Pasillo (si no, quedaban "usados" antes de tiempo)
        Act2ManagerDemo manager = Act2ManagerDemo.Instance;
        if (manager != null && manager.estadoActual != Act2ManagerDemo.Act2State.Pasillo) return;

        yaInteractuado = true;

        if (indicadorFlotante != null) indicadorFlotante.SetActive(false);
        if (sonidoDescubrimiento != null) sonidoDescubrimiento.Play();

        ParanoiaSystem.Instance?.AddParanoia(paranoiaAlEncontrar);
        Act2ManagerDemo.Instance?.ZapatosEncontrados();

        // El objeto sigue siendo visible pero ya no interactuable
        // (los zapatos quedan ahí, el jugador no los lleva)
    }
    public bool CanInteract()
    {
        // ---- VERSIÓN ANTERIOR ----
        // return true;

        // ---- REESTRUCTURA NOCHE 2 ----
        if (yaInteractuado || !visibles) return false;
        Act2ManagerDemo m = Act2ManagerDemo.Instance;
        return m == null || m.estadoActual == Act2ManagerDemo.Act2State.Pasillo;
    }

    public string GetDescription()
    {
        if (yaInteractuado) return "";
        return "Presiona [E] para examinar";
    }
}
