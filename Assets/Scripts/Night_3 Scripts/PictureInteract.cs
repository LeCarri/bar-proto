using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Portarretrato interactuable del sótano.
/// Al presionar E mediante el sistema IInteractable:
/// - Reproduce un sonido.
/// - Muestra la imagen/foto en pantalla durante unos segundos.
/// - Muestra una frase usando el sistema visual de diálogos existente.
/// - Permite volver a interactuar después de cerrarse.
/// </summary>
public class PortarretratoInteractuable : MonoBehaviour, IInteractable
{
    [Header("Estado")]
    [SerializeField] private bool imagenAbierta = false;
    [SerializeField] private bool yaVistoAlMenosUnaVez = false;

    [Header("Visual del objeto")]
    [Tooltip("Renderer del portarretrato o foto en el mundo. Opcional.")]
    [SerializeField] private Renderer rendererPortarretrato;

    [Header("Imagen en pantalla")]
    [Tooltip("Panel o imagen grande que aparece en pantalla al interactuar.")]
    [SerializeField] private GameObject imagenEnPantalla;

    [Tooltip("Tiempo que la imagen queda visible en pantalla.")]
    [SerializeField] private float tiempoImagenVisible = 6f;

    [Header("Audio")]
    [Tooltip("Sonido al interactuar con el portarretrato.")]
    [SerializeField] private AudioSource sonidoInteraccion;

    [Header("Diálogo")]
    [Tooltip("Panel de diálogo existente. Por ejemplo: FondoDialogo.")]
    [SerializeField] private GameObject panelDialogo;

    [Tooltip("Texto TMP del diálogo existente. Por ejemplo: Dialogo.")]
    [SerializeField] private TMP_Text textoDialogo;

    [TextArea(1, 3)]
    [SerializeField] private string fraseLucas = "Mi familia...";

    [Tooltip("Cuánto tarda en aparecer la frase después de abrir la foto.")]
    [SerializeField] private float delayFrase = 0.4f;

    [Tooltip("Cuánto tiempo queda visible la frase.")]
    [SerializeField] private float tiempoFraseVisible = 3f;

    [Header("Interacción")]
    [SerializeField] private string descripcionPrimeraVez = "Presiona [E] para mirar el portarretrato";
    [SerializeField] private string descripcionLuego = "Presiona [E] para volver a mirar el portarretrato";

    private Coroutine rutinaImagen;
    private Coroutine rutinaDialogo;

    private void Awake()
    {
        if (imagenEnPantalla != null)
            imagenEnPantalla.SetActive(false);

        if (panelDialogo != null)
            panelDialogo.SetActive(false);

        if (textoDialogo != null)
            textoDialogo.text = "";

        if (sonidoInteraccion != null)
        {
            sonidoInteraccion.playOnAwake = false;
            sonidoInteraccion.Stop();
        }
    }

    public void Interact()
    {
        if (imagenAbierta)
            return;

        imagenAbierta = true;
        yaVistoAlMenosUnaVez = true;

        if (sonidoInteraccion != null)
        {
            sonidoInteraccion.Stop();
            sonidoInteraccion.Play();
        }

        if (imagenEnPantalla != null)
            imagenEnPantalla.SetActive(true);

        if (rutinaImagen != null)
            StopCoroutine(rutinaImagen);

        if (rutinaDialogo != null)
            StopCoroutine(rutinaDialogo);

        rutinaImagen = StartCoroutine(CerrarImagenDespues());
        rutinaDialogo = StartCoroutine(MostrarDialogoDespues());
    }

    private IEnumerator CerrarImagenDespues()
    {
        yield return new WaitForSeconds(tiempoImagenVisible);

        if (imagenEnPantalla != null)
            imagenEnPantalla.SetActive(false);

        imagenAbierta = false;
        rutinaImagen = null;
    }

    private IEnumerator MostrarDialogoDespues()
    {
        yield return new WaitForSeconds(delayFrase);

        MostrarDialogo(fraseLucas);

        yield return new WaitForSeconds(tiempoFraseVisible);

        OcultarDialogo();

        rutinaDialogo = null;
    }

    private void MostrarDialogo(string texto)
    {
        if (panelDialogo != null)
            panelDialogo.SetActive(true);

        if (textoDialogo != null)
            textoDialogo.text = texto;
    }

    private void OcultarDialogo()
    {
        if (textoDialogo != null)
            textoDialogo.text = "";

        if (panelDialogo != null)
            panelDialogo.SetActive(false);
    }

    public bool CanInteract()
    {
        return !imagenAbierta;
    }

    public string GetDescription()
    {
        if (imagenAbierta)
            return "Mirando el portarretrato...";

        if (yaVistoAlMenosUnaVez)
            return descripcionLuego;

        return descripcionPrimeraVez;
    }
}