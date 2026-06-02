using UnityEngine;

/// <summary>
/// Nota pegada en la puerta del sótano, escrita con la letra del propio protagonista.
/// Al leerla, le da al jugador la pista de ir al baño.
/// Notifica al Act2Manager para habilitar la llave en el baño.
/// Implementa IInteractable.
///
/// SETUP: Colocar en un GameObject plano (papel) pegado sobre la puerta del sótano.
/// Act2Manager lo activa cuando corresponde (después de ZapatosEncontrados).
/// </summary>
public class NotaPuerta : MonoBehaviour, IInteractable
{
    [Header("Estado")]
    private bool yaLeida = false;

    [Header("Visual")]
    [Tooltip("Renderer del papel de la nota, para el efecto de parpadeo al activarse")]
    public Renderer rendererNota;

    [Header("Audio")]
    [Tooltip("Sonido de papel al interactuar (crinkle sound)")]
    public AudioSource sonidoPapel;
    public GameObject NotaImagen;

    public void Interact()
    {
        if (yaLeida) return;
        yaLeida = true;

        if (sonidoPapel != null) sonidoPapel.Play();
        if (NotaImagen != null) NotaImagen.gameObject.SetActive(true);
        StartCoroutine(DesactivarNotaImagen());

        // Pequeña pausa antes de la reacción de Lucas
        Invoke(nameof(ReaccionLucas), 2.5f);
    }

    private System.Collections.IEnumerator DesactivarNotaImagen()
    {
        yield return new WaitForSeconds(3.5f);
        if (NotaImagen != null) NotaImagen.gameObject.SetActive(false);
    }

    void ReaccionLucas()
    {
        Act2Manager.Instance?.NotaLeida();
    }
    public bool CanInteract()
    {
        return true;
    }

    public string GetDescription()
    {
        if (yaLeida) return "Una nota pegada en la puerta";
        return "Presiona [E] para leer la nota";
    }
}
