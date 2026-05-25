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

    [Header("Texto completo de la nota (para referencia del diseñador)")]
    [TextArea(3, 5)]
    public string textoNota = "La guardé donde nadie limpia.";

    public void Interact()
    {
        if (yaLeida) return;
        yaLeida = true;

        if (sonidoPapel != null) sonidoPapel.Play();

        // Muestra el texto de la nota como diálogo y notifica al manager
        Act2Manager.Instance?.MostrarDialogo("Nota (tu letra): \"" + textoNota + "\"");

        // Pequeña pausa antes de la reacción de Lucas
        Invoke(nameof(ReaccionLucas), 2.5f);
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
