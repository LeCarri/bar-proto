using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Copia de NotaPuerta.cs adaptada a la reestructura.
/// (El original sigue intacto para la demo.)
///
/// Guion: "Se observa una Nota escrita a mano clavada con un cuchillo en el centro de la puerta.
/// Al interactuar agarramos la nota (desaparece de la puerta), se amplía en primer plano:
/// 'La llave está FUERA DE SERVICIO'."
///
/// Al cerrar el primer plano, el Act2ManagerReestructura arranca el "Regreso al baño"
/// (bar destruido, luces rojas, el cubículo fuera de servicio cede).
///
/// SETUP:
///  - En el papel pegado en la puerta del sótano (Collider + capa "Interactable").
///    El Manager lo ACTIVA solo después de los zapatos (podés dejarlo activado en la escena).
///  - imagenNota: imagen de UI a pantalla completa con la nota en grande. Empieza DESACTIVADA.
/// </summary>
public class NotaPuertaReestructura : MonoBehaviour, IInteractable
{
    [Header("Primer plano")]
    [Tooltip("Imagen de UI con la nota en grande (\"La llave está FUERA DE SERVICIO\"). Empieza DESACTIVADA.")]
    [FormerlySerializedAs("NotaImagen")]
    public GameObject imagenNota;
    [Tooltip("Si está marcado, la nota se cierra con [E], clic o Escape. Si no, se cierra sola.")]
    public bool cerrarConTecla = true;
    [Tooltip("Segundos que se ve la nota si se cierra sola (o máximo si se cierra con tecla).")]
    public float segundosEnPantalla = 6f;
    [Tooltip("Bloquea a Lucas mientras lee la nota.")]
    public bool bloquearJugador = true;

    [Header("Al agarrarla")]
    [Tooltip("\"Agarramos la nota (desaparece de la puerta)\". El cuchillo puede quedar clavado.")]
    public bool desaparecerDeLaPuerta = true;
    [Tooltip("Objetos extra que se ocultan al agarrar la nota (NO pongas el cuchillo si querés que quede).")]
    public GameObject[] ocultarAlAgarrar;

    [Header("Audio")]
    [Tooltip("Sonido de papel al agarrarla (opcional).")]
    public AudioSource sonidoPapel;

    private bool yaLeida;

    /// <summary>Solo se puede leer en la fase Sótano (después de los zapatos).</summary>
    public bool CanInteract()
    {
        if (yaLeida) return false;
        Act2ManagerReestructura m = Act2ManagerReestructura.Instance;
        return m != null && m.estadoActual == Act2ManagerReestructura.Act2State.Sotano;
    }

    public string GetDescription() => yaLeida ? "" : "Presiona [E] para leer la nota";

    public void Interact()
    {
        if (!CanInteract()) return;
        yaLeida = true;

        if (sonidoPapel != null) sonidoPapel.Play();

        // La nota se saca de la puerta (se ocultan renderers y colliders, no el GameObject,
        // para que la corrutina de abajo siga funcionando)
        if (desaparecerDeLaPuerta)
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
            if (ocultarAlAgarrar != null)
                foreach (GameObject g in ocultarAlAgarrar) if (g != null) g.SetActive(false);
        }

        StartCoroutine(LeerNota());
    }

    IEnumerator LeerNota()
    {
        Act2ManagerReestructura m = Act2ManagerReestructura.Instance;

        if (imagenNota != null)
        {
            if (bloquearJugador && m != null) m.BloquearJugador(true);
            imagenNota.SetActive(true);

            float t = 0f;
            yield return null;
            while (cerrarConTecla && Input.GetKey(KeyCode.E) && t < segundosEnPantalla) { t += Time.deltaTime; yield return null; }

            while (t < segundosEnPantalla)
            {
                if (cerrarConTecla && (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape)))
                    break;
                t += Time.deltaTime;
                yield return null;
            }

            imagenNota.SetActive(false);
            if (bloquearJugador && m != null) m.BloquearJugador(false);
        }
        else
        {
            Debug.LogWarning($"[NotaPuertaReestructura] '{name}': no hay 'Imagen Nota' asignada; se sigue sin primer plano.");
        }

        yield return new WaitForSeconds(0.3f);

        if (m != null) m.NotaLeida();
        else Debug.LogError("[NotaPuertaReestructura] No hay Act2ManagerReestructura en la escena.");
    }
}
