using UnityEngine;

/// <summary>
/// Zapatos de niño encontrados en el estante de la cocina/depósito.
/// Al interactuar, sube la paranoia al 50% y notifica al Act2Manager.
/// Implementa IInteractable para ser detectado por PlayerInteraction.
///
/// SETUP: Colocar en el GameObject del modelo de zapatos en el estante de la cocina.
/// Asegurarse de que el estante esté en el pasillo/cocina, visible para el jugador.
/// </summary>
public class ZapatosNino : MonoBehaviour, IInteractable
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

    void Start()
    {
        if (indicadorFlotante != null) indicadorFlotante.SetActive(true);
    }

    public void Interact()
    {
        if (yaInteractuado) return;
        yaInteractuado = true;

        if (indicadorFlotante != null) indicadorFlotante.SetActive(false);
        if (sonidoDescubrimiento != null) sonidoDescubrimiento.Play();

        ParanoiaSystem.Instance?.AddParanoia(paranoiaAlEncontrar);
        Act2Manager.Instance?.ZapatosEncontrados();

        // El objeto sigue siendo visible pero ya no interactuable
        // (los zapatos quedan ahí, el jugador no los lleva)
    }

    public string GetDescription()
    {
        if (yaInteractuado) return "";
        return "Presiona [E] para examinar";
    }
}
