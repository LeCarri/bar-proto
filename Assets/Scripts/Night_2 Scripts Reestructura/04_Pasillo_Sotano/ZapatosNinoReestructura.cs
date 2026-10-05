using UnityEngine;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Copia de ZapatosNiño.cs adaptada a la reestructura.
/// (El original sigue intacto para la demo.)
///
/// Guion: "Al llegar y buscar, la botella no está. En su lugar hay un par de zapatos de nena.
/// Pequeños. Sucios. Fuera de lugar." Al interactuar → "No... No deberían estar acá..." y empieza
/// la secuencia de los golpes en la puerta del sótano (la hace el Act2ManagerReestructura).
///
/// - Los zapatos están OCULTOS hasta que el cliente corrupto hace el pedido (en las tareas el
///   jugador pasa por el depósito a buscar la escoba y no los tiene que ver).
/// - Solo se pueden examinar en la fase Pasillo.
///
/// SETUP: en el modelo de los zapatos en el estante del depósito (Collider + capa "Interactable").
/// </summary>
public class ZapatosNinoReestructura : MonoBehaviour, IInteractable
{
    [Header("Visual")]
    [Tooltip("Indicador flotante sobre los zapatos (opcional).")]
    public GameObject indicadorFlotante;
    [Tooltip("Ocultar los zapatos hasta que el cliente corrupto hace el pedido. Dejalo marcado.")]
    public bool ocultarHastaElPedido = true;
    [Tooltip("OPCIONAL: la botella que está en el estante durante las tareas. \"La botella no está\": se oculta cuando aparecen los zapatos.")]
    public GameObject botellaEnElEstante;

    [Header("Audio")]
    [Tooltip("Sonido perturbador al encontrar los zapatos (opcional).")]
    public AudioSource sonidoDescubrimiento;

    [Header("Paranoia")]
    [Tooltip("Cuánta paranoia sube al encontrar los zapatos.")]
    public float paranoiaAlEncontrar = 30f;

    private bool yaInteractuado;
    private bool visibles = true;

    void Start()
    {
        if (ocultarHastaElPedido) MostrarZapatos(false);
        else if (indicadorFlotante != null) indicadorFlotante.SetActive(true);
    }

    // Aparecen en cuanto empieza la fase Pasillo
    void Update()
    {
        if (visibles) return;
        if (EsFasePasillo()) MostrarZapatos(true);
    }

    void MostrarZapatos(bool mostrar)
    {
        visibles = mostrar;
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = mostrar;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = mostrar;
        if (indicadorFlotante != null) indicadorFlotante.SetActive(mostrar && !yaInteractuado);
        if (mostrar && botellaEnElEstante != null) botellaEnElEstante.SetActive(false);
    }

    bool EsFasePasillo()
    {
        Act2ManagerReestructura m = Act2ManagerReestructura.Instance;
        return m != null && m.estadoActual == Act2ManagerReestructura.Act2State.Pasillo;
    }

    public bool CanInteract()
    {
        return !yaInteractuado && visibles && EsFasePasillo();
    }

    public string GetDescription()
    {
        return yaInteractuado ? "" : "Presiona [E] para examinar";
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        yaInteractuado = true;

        if (indicadorFlotante != null) indicadorFlotante.SetActive(false);
        if (sonidoDescubrimiento != null) sonidoDescubrimiento.Play();

        Act2ManagerReestructura m = Act2ManagerReestructura.Instance;
        m.SumarParanoia(paranoiaAlEncontrar);
        m.ZapatosEncontrados();
        // Los zapatos quedan ahí (el jugador no se los lleva), pero ya no se pueden examinar.
    }
}
