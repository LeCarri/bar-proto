using UnityEngine;
using System.Collections;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Objeto que se inspecciona con [E]:
///  - La hoja que cae del armario (dibujo de Pilar con la familia tachada).
///  - La foto familiar "intervenida" del exhibidor.
///
/// Dos formas de mostrar el objeto (podés usar una, las dos o ninguna):
///  1) imagenPrimerPlano: un panel/imagen de UI a pantalla completa (lo más simple).
///     Se cierra con [E], clic o Escape.
///  2) usarInspector3D: usa el Inspector3D de la Noche 1 (si está en la escena) para girar el objeto.
///
/// El guion no le da diálogo a Lucas en estos objetos, así que "dialogo" viene vacío.
/// Si el equipo quiere agregarle una frase, se escribe ahí.
/// </summary>
public class ObjetoNarrativoReestructura : MonoBehaviour, IInteractable
{
    [Header("Interacción")]
    public string textoAccion = "Inspeccionar";
    [Tooltip("Si es true solo se puede inspeccionar una vez.")]
    public bool unaSolaVez = false;

    [Header("Primer plano (UI)")]
    [Tooltip("Imagen/panel de UI con el objeto en grande. Debe empezar DESACTIVADO.")]
    public GameObject imagenPrimerPlano;
    [Tooltip("Bloquea el movimiento del jugador mientras se ve el primer plano.")]
    public bool bloquearJugador = true;

    [Header("Inspector 3D (Noche 1)")]
    public bool usarInspector3D = false;

    [Header("Narrativa")]
    [TextArea] public string dialogo = "";
    public float paranoia = 0f;
    public AudioSource sonidoAlInspeccionar;

    [Header("Después de inspeccionar")]
    [Tooltip("Oculta el objeto del mundo al terminar.")]
    public bool ocultarAlTerminar = false;

    private bool inspeccionado;
    private bool mostrando;

    public bool CanInteract() => !mostrando && !(unaSolaVez && inspeccionado);
    public string GetDescription() => textoAccion;

    public void Interact()
    {
        if (!CanInteract()) return;
        inspeccionado = true;

        if (sonidoAlInspeccionar != null) sonidoAlInspeccionar.Play();

        Act2ManagerReestructura m = Act2ManagerReestructura.Instance;
        if (m != null)
        {
            if (!string.IsNullOrEmpty(dialogo)) m.MostrarDialogo(dialogo);
            m.SumarParanoia(paranoia);
        }

        if (usarInspector3D && Inspector3D.Instance != null)
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Transform punto = Inspector3D.Instance.PuntoInspeccion;
            if (punto != null)
            {
                transform.SetParent(punto);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            // Inspector3D solo muestra texto si hay Act1Manager; el diálogo ya lo mostró el Manager.
            Inspector3D.Instance.IniciarInspeccion(gameObject, dialogo);
            return;
        }

        if (imagenPrimerPlano != null)
            StartCoroutine(MostrarPrimerPlano());
        else if (ocultarAlTerminar)
            gameObject.SetActive(false);
    }

    IEnumerator MostrarPrimerPlano()
    {
        mostrando = true;
        Act2ManagerReestructura m = Act2ManagerReestructura.Instance;
        if (bloquearJugador && m != null) m.BloquearJugador(true);

        imagenPrimerPlano.SetActive(true);

        // Esperar a que suelte la [E] con la que interactuó
        yield return null;
        while (Input.GetKey(KeyCode.E)) yield return null;

        // Cerrar con E, clic o Escape
        while (!(Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape)))
            yield return null;

        imagenPrimerPlano.SetActive(false);
        if (bloquearJugador && m != null) m.BloquearJugador(false);
        mostrando = false;

        if (ocultarAlTerminar) gameObject.SetActive(false);
    }
}
